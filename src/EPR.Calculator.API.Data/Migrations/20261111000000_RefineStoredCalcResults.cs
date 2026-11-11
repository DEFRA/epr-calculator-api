using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefineStoredCalcResults : Migration
    {
        private const string Money = "decimal(20, 3)";
        private const string Tonnage = "decimal(24, 6)";
        private const string FullPrecisionMoney = "decimal(38, 28)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The casts before dividing are kept small so SQL Server keeps 19 decimal places.
            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET lapcap = JSON_MODIFY(JSON_MODIFY(x.lapcap,
                    '$.Total', JSON_QUERY(t.total_json)),
                    '$.CountryApportionment', JSON_QUERY(a.apportionment_json))
                FROM dbo.calc_result_lapcap_data x
                CROSS APPLY (
                    SELECT CAST(ISNULL(SUM(m.England), 0)         AS {Money}) AS England,
                           CAST(ISNULL(SUM(m.Wales), 0)           AS {Money}) AS Wales,
                           CAST(ISNULL(SUM(m.Scotland), 0)        AS {Money}) AS Scotland,
                           CAST(ISNULL(SUM(m.NorthernIreland), 0) AS {Money}) AS NorthernIreland
                    FROM OPENJSON(x.lapcap, '$.MaterialCosts') WITH (
                        England         {Money} '$.Cost.England',
                        Wales           {Money} '$.Cost.Wales',
                        Scotland        {Money} '$.Cost.Scotland',
                        NorthernIreland {Money} '$.Cost.NorthernIreland') m
                ) s
                CROSS APPLY (SELECT CAST(s.England + s.Wales + s.Scotland + s.NorthernIreland AS {Money}) AS uk) u
                CROSS APPLY (
                    SELECT (SELECT s.England, s.Wales, s.Scotland, s.NorthernIreland FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS total_json
                ) t
                CROSS APPLY (
                    SELECT (
                        SELECT CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.England         * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS England,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.Wales           * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS Wales,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.Scotland        * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS Scotland,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.NorthernIreland * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS NorthernIreland
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS apportionment_json
                ) a
                WHERE JSON_QUERY(x.lapcap, '$.Total') IS NULL
                """));

            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET late_reporting_tonnage = JSON_MODIFY(x.late_reporting_tonnage, '$.Total', JSON_QUERY(t.total_json))
                FROM dbo.calc_result_late_reporting_tonnage x
                CROSS APPLY (
                    SELECT (
                        SELECT ISNULL(SUM(m.Total), 0) AS Total,
                               ISNULL(SUM(m.Red), 0)   AS Red,
                               ISNULL(SUM(m.Amber), 0) AS Amber,
                               ISNULL(SUM(m.Green), 0) AS Green
                        FROM OPENJSON(x.late_reporting_tonnage, '$.MaterialTonnages') WITH (
                            Total {Tonnage} '$.Detail.Total',
                            Red   {Tonnage} '$.Detail.Red',
                            Amber {Tonnage} '$.Detail.Amber',
                            Green {Tonnage} '$.Detail.Green') m
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS total_json
                ) t
                WHERE JSON_QUERY(x.late_reporting_tonnage, '$.Total') IS NULL
                """));

            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET la_disposal_cost = JSON_MODIFY(JSON_MODIFY(x.la_disposal_cost,
                    '$.MaterialCosts', JSON_QUERY(a.materials_json)),
                    '$.Total', JSON_QUERY(t.total_json))
                FROM dbo.calc_result_la_disposal_cost x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(JSON_MODIFY(m.value,
                                '$.Detail.TotalTonnage', d.TotalTonnage),
                                '$.Detail.DisposalCostPricePerTonne', d.DisposalCostPricePerTonne) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY m.ord), '') + ']' AS materials_json,
                           ISNULL(SUM(v.England), 0)         AS England,
                           ISNULL(SUM(v.Wales), 0)           AS Wales,
                           ISNULL(SUM(v.Scotland), 0)        AS Scotland,
                           ISNULL(SUM(v.NorthernIreland), 0) AS NorthernIreland,
                           ISNULL(SUM(v.Household), 0)       AS Household,
                           ISNULL(SUM(v.PublicBin), 0)       AS PublicBin,
                           ISNULL(SUM(v.DrinkContainers), 0) AS DrinkContainers,
                           ISNULL(SUM(v.LateReporting), 0)   AS LateReporting,
                           ISNULL(SUM(v.Smcw), 0)            AS Smcw
                    FROM (SELECT CAST([key] AS int) AS ord, value FROM OPENJSON(x.la_disposal_cost, '$.MaterialCosts')) m
                    CROSS APPLY OPENJSON(m.value, '$.Detail') WITH (
                        England         {Money} '$.Cost.England',
                        Wales           {Money} '$.Cost.Wales',
                        Scotland        {Money} '$.Cost.Scotland',
                        NorthernIreland {Money} '$.Cost.NorthernIreland',
                        Household       {Tonnage} '$.HouseholdPackagingWasteTonnage',
                        PublicBin       {Tonnage} '$.PublicBinTonnage',
                        DrinkContainers {Tonnage} '$.HouseholdDrinkContainersTonnage',
                        LateReporting   {Tonnage} '$.LateReportingTonnage',
                        Smcw            {Tonnage} '$.ActionedSelfManagedConsumerWasteTonnage') v
                    CROSS APPLY (
                        SELECT CAST(v.LateReporting + v.Household + v.PublicBin + v.DrinkContainers - ISNULL(v.Smcw, 0) AS {Tonnage}) AS TotalTonnage,
                               CAST(v.England + v.Wales + v.Scotland + v.NorthernIreland AS {Money}) AS Cost
                    ) c
                    CROSS APPLY (
                        SELECT c.TotalTonnage,
                               CASE WHEN c.TotalTonnage = 0 THEN NULL ELSE ROUND(c.Cost / c.TotalTonnage, 4) END AS DisposalCostPricePerTonne
                    ) d
                ) a
                CROSS APPLY (
                    SELECT CAST(a.LateReporting + a.Household + a.PublicBin + a.DrinkContainers - a.Smcw AS {Tonnage}) AS TotalTonnage,
                           CAST(a.England + a.Wales + a.Scotland + a.NorthernIreland AS {Money}) AS Cost
                ) tc
                CROSS APPLY (
                    SELECT (
                        SELECT a.England         AS [Cost.England],
                               a.Wales           AS [Cost.Wales],
                               a.Scotland        AS [Cost.Scotland],
                               a.NorthernIreland AS [Cost.NorthernIreland],
                               a.Household       AS HouseholdPackagingWasteTonnage,
                               a.PublicBin       AS PublicBinTonnage,
                               a.DrinkContainers AS HouseholdDrinkContainersTonnage,
                               a.LateReporting   AS LateReportingTonnage,
                               a.Smcw            AS ActionedSelfManagedConsumerWasteTonnage,
                               tc.TotalTonnage   AS TotalTonnage,
                               CASE WHEN tc.TotalTonnage = 0 THEN NULL ELSE ROUND(tc.Cost / tc.TotalTonnage, 4) END AS DisposalCostPricePerTonne
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS total_json
                ) t
                WHERE JSON_QUERY(x.la_disposal_cost, '$.Total') IS NULL
                """));

            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET comms_cost = JSON_MODIFY(JSON_MODIFY(x.comms_cost,
                    '$.MaterialCosts', JSON_QUERY(a.materials_json)),
                    '$.Total', JSON_QUERY(t.total_json))
                FROM dbo.calc_result_comms_cost x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(JSON_MODIFY(m.value,
                                '$.CommsCost.TotalTonnage', d.TotalTonnage),
                                '$.CommsCost.PricePerTonne', d.PricePerTonne) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY m.ord), '') + ']' AS materials_json,
                           ISNULL(SUM(v.England), 0)         AS England,
                           ISNULL(SUM(v.Wales), 0)           AS Wales,
                           ISNULL(SUM(v.Scotland), 0)        AS Scotland,
                           ISNULL(SUM(v.NorthernIreland), 0) AS NorthernIreland,
                           ISNULL(SUM(v.TotalCost), 0)       AS TotalCost,
                           ISNULL(SUM(v.Household), 0)       AS Household,
                           ISNULL(SUM(v.PublicBin), 0)       AS PublicBin,
                           ISNULL(SUM(v.DrinksContainers), 0) AS DrinksContainers,
                           ISNULL(SUM(v.LateReporting), 0)   AS LateReporting
                    FROM (SELECT CAST([key] AS int) AS ord, value FROM OPENJSON(x.comms_cost, '$.MaterialCosts')) m
                    CROSS APPLY OPENJSON(m.value, '$.CommsCost') WITH (
                        England          {FullPrecisionMoney} '$.Cost.England',
                        Wales            {FullPrecisionMoney} '$.Cost.Wales',
                        Scotland         {FullPrecisionMoney} '$.Cost.Scotland',
                        NorthernIreland  {FullPrecisionMoney} '$.Cost.NorthernIreland',
                        TotalCost        {Money}  '$.TotalCost',
                        Household        {Tonnage}  '$.HouseholdPackagingWasteTonnage',
                        PublicBin        {Tonnage}  '$.PublicBinTonnage',
                        DrinksContainers {Tonnage}  '$.HouseholdDrinksContainersTonnage',
                        LateReporting    {Tonnage}  '$.LateReportingTonnage') v
                    CROSS APPLY (
                        SELECT CAST(v.Household + v.LateReporting + v.PublicBin + v.DrinksContainers AS {Tonnage}) AS TotalTonnage
                    ) c
                    CROSS APPLY (
                        SELECT c.TotalTonnage,
                               CASE WHEN c.TotalTonnage = 0 THEN 0 ELSE ROUND(v.TotalCost / c.TotalTonnage, 4) END AS PricePerTonne
                    ) d
                ) a
                CROSS APPLY (
                    SELECT CAST(a.Household + a.LateReporting + a.PublicBin + a.DrinksContainers AS {Tonnage}) AS TotalTonnage,
                           CAST(a.TotalCost AS {Money}) AS TotalCost
                ) tc
                CROSS APPLY (
                    SELECT (
                        SELECT a.England          AS [Cost.England],
                               a.Wales            AS [Cost.Wales],
                               a.Scotland         AS [Cost.Scotland],
                               a.NorthernIreland  AS [Cost.NorthernIreland],
                               a.TotalCost        AS TotalCost,
                               a.Household        AS HouseholdPackagingWasteTonnage,
                               a.PublicBin        AS PublicBinTonnage,
                               a.DrinksContainers AS HouseholdDrinksContainersTonnage,
                               a.LateReporting    AS LateReportingTonnage,
                               tc.TotalTonnage    AS TotalTonnage,
                               CASE WHEN tc.TotalTonnage = 0 THEN 0 ELSE ROUND(tc.TotalCost / tc.TotalTonnage, 4) END AS PricePerTonne
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS total_json
                ) t
                WHERE JSON_QUERY(x.comms_cost, '$.Total') IS NULL
                """));

            // Kept as _bk tables so Down can restore them.
            migrationBuilder.RenameTable(
                name: "calc_result_smcw_producer",
                newName: "calc_result_smcw_producer_bk");

            migrationBuilder.RenameTable(
                name: "calc_result_smcw",
                newName: "calc_result_smcw_bk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "calc_result_smcw_bk",
                newName: "calc_result_smcw");

            migrationBuilder.RenameTable(
                name: "calc_result_smcw_producer_bk",
                newName: "calc_result_smcw_producer");

            migrationBuilder.Sql(Exec("""
                UPDATE dbo.calc_result_lapcap_data
                SET lapcap = JSON_MODIFY(JSON_MODIFY(lapcap, '$.Total', NULL), '$.CountryApportionment', NULL)
                """));

            migrationBuilder.Sql(Exec("""
                UPDATE dbo.calc_result_late_reporting_tonnage
                SET late_reporting_tonnage = JSON_MODIFY(late_reporting_tonnage, '$.Total', NULL)
                """));

            migrationBuilder.Sql(Exec("""
                UPDATE x
                SET la_disposal_cost = JSON_MODIFY(JSON_MODIFY(x.la_disposal_cost,
                    '$.MaterialCosts', JSON_QUERY(a.materials_json)),
                    '$.Total', NULL)
                FROM dbo.calc_result_la_disposal_cost x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(JSON_MODIFY(m.value,
                                '$.Detail.TotalTonnage', NULL),
                                '$.Detail.DisposalCostPricePerTonne', NULL) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY CAST(m.[key] AS int)), '') + ']' AS materials_json
                    FROM OPENJSON(x.la_disposal_cost, '$.MaterialCosts') m
                ) a
                """));

            migrationBuilder.Sql(Exec("""
                UPDATE x
                SET comms_cost = JSON_MODIFY(JSON_MODIFY(x.comms_cost,
                    '$.MaterialCosts', JSON_QUERY(a.materials_json)),
                    '$.Total', NULL)
                FROM dbo.calc_result_comms_cost x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(JSON_MODIFY(m.value,
                                '$.CommsCost.TotalTonnage', NULL),
                                '$.CommsCost.PricePerTonne', NULL) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY CAST(m.[key] AS int)), '') + ']' AS materials_json
                    FROM OPENJSON(x.comms_cost, '$.MaterialCosts') m
                ) a
                """));
        }

        private static string Exec(string sql) => $"EXEC(N'{sql.Replace("'", "''")}');";
    }
}
