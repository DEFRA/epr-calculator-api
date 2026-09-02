using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.CommonDataService.DataApi.CommonDataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.Calculator.API.BackgroundService.Services.DataLoading
{
    internal static class CommonDataApiLoaderMapper
    {
        /// <summary>
        ///     Creates a mapper function to convert PayCalPom entities to AlignmentPom records.
        ///     POMs are never persisted - they exist only for the duration of a run's error-checking
        ///     and alignment.
        /// </summary>
        /// <returns>A mapper function that throws FormatException if SubmitterId is invalid.</returns>
        internal static Func<PayCalPom, AlignmentPom> MapPom(ILogger logger)
        {
            return r => new AlignmentPom
            {
                SubmissionPeriod = r.SubmissionPeriod,
                OrganisationId = r.OrganisationId,
                SubsidiaryId = r.SubsidiaryId,
                PackagingType = r.PackagingType,
                PackagingMaterial = r.PackagingMaterial,
                PackagingMaterialWeight = r.PackagingMaterialWeight,
                RamRagRating = SafeParseRamRagRating(r, logger),
                SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
                    ? guid
                    : throw new FormatException(
                        $"Invalid {nameof(PayCalPom)}.{nameof(PayCalPom.SubmitterId)}: {r.SubmitterId}")
            };
        }


        private static string? SafeParseRamRagRating(PayCalPom pom, ILogger logger)
        {
            try
            {
                return string.IsNullOrWhiteSpace(pom.RamRagRating)
                    ? null
                    : RagRatingExtensions.ParseRag(pom.RamRagRating.Trim()).ToDbValue();
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Invalid RAG rating OrganisationId: '{OrganisationId}' SubsidiaryId: '{SubsidiaryId}' SubmitterId: '{SubmitterId}' RamRagRating '{RamRagRating}' Material '{Material}' - treating as Red",
                    pom.OrganisationId,
                    pom.SubsidiaryId,
                    pom.SubmitterId,
                    pom.RamRagRating,
                    pom.PackagingMaterial);

                return RagRating.Red.ToDbValue();
            }
        }


        /// <summary>
        ///     Creates a mapper function to convert PayCalOrganisation entities to CalculatorRunOrganisation
        ///     database entities.
        /// </summary>
        /// <returns>A mapper function that throws FormatException if required fields are null or invalid.</returns>
        internal static Func<PayCalOrganisation, CalculatorRunOrganisation> MapOrganisation()
        {
            return r => new CalculatorRunOrganisation
            {
                OrganisationId = r.OrganisationId ?? throw new FormatException(
                    $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.OrganisationId)}: {r.OrganisationId}"),
                SubsidiaryId = r.SubsidiaryId,
                OrganisationName = r.OrganisationName ?? throw new FormatException(
                    $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.OrganisationName)}: {r.OrganisationName}"),
                TradingName = r.TradingName,
                StatusCode = r.StatusCode,
                ErrorCode = r.ErrorCode,
                JoinerDate = r.JoinerDate,
                LeaverDate = r.LeaverDate,
                ObligationStatus = r.ObligationStatus ?? throw new FormatException(
                    $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.ObligationStatus)}: {r.ObligationStatus}"),
                DaysObligated = r.NumDaysObligated,
                SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
                    ? guid
                    : throw new FormatException(
                        $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.SubmitterId)}: {r.SubmitterId}"),
                HasH1 = r.HasH1,
                HasH2 = r.HasH2
            };
        }
    }
}
