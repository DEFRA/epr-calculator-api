using System.Collections.Immutable;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     Reads the H1 and H2 projected producers sections, which list each material's columns in the order their exporters write them.
/// </summary>
public static class ProjectedProducersParser
{
    /// <summary>Returns null when the file has no projected producers sections (runs before modulation).</summary>
    public static CalcResultProjectedProducers? Read(BillingCsv csv, IReadOnlyDictionary<string, string> materialCodes)
    {
        if (csv.TryFindRow("H2 Packaging Data") is not { } h2Section || csv.TryFindRow("H1 Packaging Data") is not { } h1Section)
            return null;

        return new CalcResultProjectedProducers
        {
            H2ProjectedProducers = [.. Rows(csv, h2Section).Select(row => ReadH2(csv, row.Row, row.Blocks(materialCodes)))],
            H1ProjectedProducers = [.. Rows(csv, h1Section).Select(row => ReadH1(csv, row.Row, row.Blocks(materialCodes)))]
        };
    }

    private sealed record ProducerRow(int Row, int Header, BillingCsv Csv)
    {
        /// <summary>Each material's submitted block and, for H1, its projected block, by the group row's headings.</summary>
        public IReadOnlyList<(string Code, int Start, int? ProjectedStart)> Blocks(IReadOnlyDictionary<string, string> materialCodes)
        {
            var groupRow = Header - 1;
            var headings = Enumerable.Range(0, Csv.Rows[groupRow].Length)
                .Select(c => (Column: c, Text: Csv.Cell(groupRow, c)))
                .Where(h => h.Text.EndsWith(" Breakdown", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return [.. headings
                .Where(h => !h.Text.StartsWith("Projected ", StringComparison.OrdinalIgnoreCase))
                .Select(h =>
                {
                    var material = h.Text[..^" Breakdown".Length];
                    var projected = headings.FirstOrDefault(p => p.Text.Equals($"Projected {h.Text}", StringComparison.OrdinalIgnoreCase));
                    return (materialCodes[material], h.Column, projected.Text is null ? (int?)null : projected.Column);
                })];
        }
    }

    private static IEnumerable<ProducerRow> Rows(BillingCsv csv, int section)
    {
        var header = csv.FindRow("Producer ID", section);
        for (var row = header + 1; row < csv.Rows.Count && int.TryParse(csv.Cell(row, 0), out _); row++)
            yield return new ProducerRow(row, header, csv);
    }

    private static CalcResultH2ProjectedProducer ReadH2(BillingCsv csv, int row, IReadOnlyList<(string Code, int Start, int? ProjectedStart)> blocks) => new()
    {
        ProducerId                   = (int)csv.Number(row, 0),
        SubsidiaryId                 = NullIfEmpty(csv.Cell(row, 1)),
        Level                        = csv.Cell(row, 2),
        SubmissionPeriodCode         = csv.Cell(row, 3),
        H2ProjectedTonnageByMaterial = blocks.ToDictionary(b => b.Code, b =>
        {
            var reader = new BlockReader(csv, row, b.Start);
            var submitted = reader.Submitted(b.Code);
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = submitted.Household.Tonnage,
                HouseholdRAMTonnage                         = submitted.Household.Ram,
                HouseholdTonnageWithoutRAM                  = submitted.Household.WithoutRam,
                PublicBinTonnage                            = submitted.PublicBin.Tonnage,
                PublicBinRAMTonnage                         = submitted.PublicBin.Ram,
                PublicBinTonnageWithoutRAM                  = submitted.PublicBin.WithoutRam,
                HouseholdDrinksContainerTonnage             = submitted.DrinksContainers?.Tonnage,
                HouseholdDrinksContainerRAMTonnage          = submitted.DrinksContainers?.Ram,
                HouseholdDrinksContainerTonnageWithoutRAM   = submitted.DrinksContainers?.WithoutRam,
                TotalTonnage                                = reader.Next(),
                // H2's projected tonnages aren't shown, so they're not known.
                ProjectedHouseholdTonnage                   = 0,
                ProjectedHouseholdRAMTonnage                = RamTonnage.Empty,
                ProjectedPublicBinTonnage                   = 0,
                ProjectedPublicBinRAMTonnage                = RamTonnage.Empty,
                ProjectedHouseholdDrinksContainerTonnage    = b.Code == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = b.Code == MaterialCodes.Glass ? RamTonnage.Empty : null
            };
        })
    };

    private static CalcResultH1ProjectedProducer ReadH1(BillingCsv csv, int row, IReadOnlyList<(string Code, int Start, int? ProjectedStart)> blocks) => new()
    {
        ProducerId                   = (int)csv.Number(row, 0),
        SubsidiaryId                 = NullIfEmpty(csv.Cell(row, 1)),
        Level                        = csv.Cell(row, 2),
        SubmissionPeriodCode         = csv.Cell(row, 3),
        H1ProjectedTonnageByMaterial = blocks.ToDictionary(b => b.Code, b =>
        {
            var reader = new BlockReader(csv, row, b.Start);
            var submitted = reader.Submitted(b.Code);

            // A proportion is shown as "-" when the producer had nothing without a RAM rating.
            decimal Proportion() => (reader.NextOrNull() ?? 0) / 100;
            var proportions = new RAMProportions
            {
                Red = Proportion(), Amber = Proportion(), Green = Proportion(),
                RedMedical = Proportion(), AmberMedical = Proportion(), GreenMedical = Proportion()
            };

            var projected = new BlockReader(csv, row, b.ProjectedStart ?? throw new FormatException($"No projected {b.Code} breakdown"));
            var (household, householdRam) = projected.TonnageAndRam();
            var (publicBin, publicBinRam) = projected.TonnageAndRam();
            var drinksContainers = b.Code == MaterialCodes.Glass ? projected.TonnageAndRam() : default((decimal, RamTonnage)?);

            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = submitted.Household.Tonnage,
                HouseholdRAMTonnage                         = submitted.Household.Ram,
                HouseholdTonnageWithoutRAM                  = submitted.Household.WithoutRam,
                PublicBinTonnage                            = submitted.PublicBin.Tonnage,
                PublicBinRAMTonnage                         = submitted.PublicBin.Ram,
                PublicBinTonnageWithoutRAM                  = submitted.PublicBin.WithoutRam,
                HouseholdDrinksContainerTonnage             = submitted.DrinksContainers?.Tonnage,
                HouseholdDrinksContainerRAMTonnage          = submitted.DrinksContainers?.Ram,
                HouseholdDrinksContainerTonnageWithoutRAM   = submitted.DrinksContainers?.WithoutRam,
                H2RamProportions                            = proportions,
                ProjectedHouseholdTonnage                   = household,
                ProjectedHouseholdRAMTonnage                = householdRam,
                ProjectedPublicBinTonnage                   = publicBin,
                ProjectedPublicBinRAMTonnage                = publicBinRam,
                ProjectedHouseholdDrinksContainerTonnage    = drinksContainers?.Item1,
                ProjectedHouseholdDrinksContainerRAMTonnage = drinksContainers?.Item2
            };
        })
    };

    private sealed class BlockReader(BillingCsv csv, int row, int column)
    {
        private int next = column;

        public decimal? NextOrNull() => csv.NullableNumber(row, next++);

        public decimal Next() => NextOrNull() ?? 0;

        public (decimal Tonnage, RamTonnage Ram) TonnageAndRam()
        {
            var tonnage = Next();
            var ram = new RamTonnage { Red = Next(), Amber = Next(), Green = Next(), RedMedical = Next(), AmberMedical = Next(), GreenMedical = Next() };
            return (tonnage, ram);
        }

        public record Packaging(decimal Tonnage, RamTonnage Ram, decimal WithoutRam);

        public (Packaging Household, Packaging PublicBin, Packaging? DrinksContainers) Submitted(string materialCode)
        {
            Packaging Read()
            {
                var (tonnage, ram) = TonnageAndRam();
                return new Packaging(tonnage, ram, Next());
            }

            return (Read(), Read(), materialCode == MaterialCodes.Glass ? Read() : null);
        }
    }

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
}
