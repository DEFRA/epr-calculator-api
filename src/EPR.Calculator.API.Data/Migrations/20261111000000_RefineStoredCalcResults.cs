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
        private const string Utf8Varchar = "varchar(max) COLLATE Latin1_General_100_CI_AS_SC_UTF8";

        private static readonly string[] PartialObligationColumns =
        [
            "partial_household_tonnage",
            "partial_household_tonnage_red",
            "partial_household_tonnage_amber",
            "partial_household_tonnage_green",
            "partial_household_tonnage_red_medical",
            "partial_household_tonnage_amber_medical",
            "partial_household_tonnage_green_medical",
            "partial_public_bin_tonnage",
            "partial_public_bin_tonnage_red",
            "partial_public_bin_tonnage_amber",
            "partial_public_bin_tonnage_green",
            "partial_public_bin_tonnage_red_medical",
            "partial_public_bin_tonnage_amber_medical",
            "partial_public_bin_tonnage_green_medical",
            "partial_hdc_tonnage",
            "partial_hdc_tonnage_red",
            "partial_hdc_tonnage_amber",
            "partial_hdc_tonnage_green",
            "partial_hdc_tonnage_red_medical",
            "partial_hdc_tonnage_amber_medical",
            "partial_hdc_tonnage_green_medical",
            "partial_smcw_tonnage",
        ];

        private static readonly string[] RequiredPartialObligationColumns = ["partial_household_tonnage", "partial_public_bin_tonnage", "partial_smcw_tonnage"];

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

            // Comms stores the same % the run worked out, so it is copied exactly where a comms row exists.
            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET one_plus_four_apppointment = JSON_MODIFY(JSON_MODIFY(x.one_plus_four_apppointment,
                    '$.TotalOnePlusFour', JSON_QUERY(t.total_json)),
                    '$.OnePlusFourApportionment', JSON_QUERY(COALESCE(c.apportionment_json, a.apportionment_json)))
                FROM dbo.calc_result_one_plus_four_apportionment x
                CROSS APPLY OPENJSON(x.one_plus_four_apppointment) WITH (
                    LaEngland           {Money} '$.LaDisposalCost.England',
                    LaWales             {Money} '$.LaDisposalCost.Wales',
                    LaScotland          {Money} '$.LaDisposalCost.Scotland',
                    LaNorthernIreland   {Money} '$.LaDisposalCost.NorthernIreland',
                    PrepEngland         {Money} '$.LADataPrepCharge.England',
                    PrepWales           {Money} '$.LADataPrepCharge.Wales',
                    PrepScotland        {Money} '$.LADataPrepCharge.Scotland',
                    PrepNorthernIreland {Money} '$.LADataPrepCharge.NorthernIreland') v
                CROSS APPLY (
                    SELECT CAST(v.LaEngland         + v.PrepEngland         AS {Money}) AS England,
                           CAST(v.LaWales           + v.PrepWales           AS {Money}) AS Wales,
                           CAST(v.LaScotland        + v.PrepScotland        AS {Money}) AS Scotland,
                           CAST(v.LaNorthernIreland + v.PrepNorthernIreland AS {Money}) AS NorthernIreland
                ) s
                CROSS APPLY (SELECT CAST(s.England + s.Wales + s.Scotland + s.NorthernIreland AS {Money}) AS uk) u
                CROSS APPLY (
                    SELECT (SELECT s.England, s.Wales, s.Scotland, s.NorthernIreland FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS total_json
                ) t
                OUTER APPLY (
                    SELECT TOP 1 JSON_QUERY(cc.comms_cost, '$.OnePlusFourApportionment') AS apportionment_json
                    FROM dbo.calc_result_comms_cost cc
                    WHERE cc.calculator_run_id = x.calculator_run_id
                    ORDER BY cc.id
                ) c
                CROSS APPLY (
                    SELECT (
                        SELECT CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.England         * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS England,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.Wales           * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS Wales,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.Scotland        * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS Scotland,
                               CASE WHEN u.uk = 0 THEN 0 ELSE CAST(s.NorthernIreland * 100 AS decimal(19, 3)) / CAST(u.uk AS decimal(15, 3)) END AS NorthernIreland
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS apportionment_json
                ) a
                WHERE JSON_QUERY(x.one_plus_four_apppointment, '$.TotalOnePlusFour') IS NULL
                """));

            migrationBuilder.AddColumn<string>(
                name: "total",
                table: "calc_result_modulation",
                type: Utf8Varchar,
                nullable: true);

            migrationBuilder.Sql(Exec($"""
                UPDATE x
                SET material_modulations = a.materials_json,
                    total = t.total_json
                FROM dbo.calc_result_modulation x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(m.value,
                                '$.ModulationDetail.TotalMaterialTonnages', v.Red + v.Amber + v.Green) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY m.ord), '') + ']' AS materials_json,
                           ISNULL(SUM(v.Red), 0)                  AS Red,
                           ISNULL(SUM(v.Amber), 0)                AS Amber,
                           ISNULL(SUM(v.Green), 0)                AS Green,
                           ISNULL(SUM(v.Red + v.Amber + v.Green), 0) AS Total,
                           ISNULL(SUM(v.RedAtAmber), 0)           AS RedAtAmber,
                           ISNULL(SUM(v.GreenAtAmber), 0)         AS GreenAtAmber
                    FROM (SELECT CAST([key] AS int) AS ord, value FROM OPENJSON(x.material_modulations)) m
                    CROSS APPLY OPENJSON(m.value, '$.ModulationDetail') WITH (
                        Red          {Tonnage} '$.RedMaterialTonnages',
                        Amber        {Tonnage} '$.AmberMaterialTonnages',
                        Green        {Tonnage} '$.GreenMaterialTonnages',
                        RedAtAmber   {Money}   '$.TotalRedMaterialAtAmberDisposalCost',
                        GreenAtAmber {Money}   '$.TotalGreenMaterialAtAmberDisposalCost') v
                ) a
                CROSS APPLY (
                    SELECT (
                        SELECT a.Red          AS RedMaterialTonnages,
                               a.Amber        AS AmberMaterialTonnages,
                               a.Green        AS GreenMaterialTonnages,
                               a.Total        AS TotalMaterialTonnages,
                               a.RedAtAmber   AS TotalRedMaterialAtAmberDisposalCost,
                               a.GreenAtAmber AS TotalGreenMaterialAtAmberDisposalCost
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ) AS total_json
                ) t
                WHERE x.total IS NULL
                """));

            migrationBuilder.Sql(Exec($"ALTER TABLE dbo.calc_result_modulation ALTER COLUMN total {Utf8Varchar} NOT NULL"));

            migrationBuilder.Sql(Exec("EXEC sp_rename N'dbo.PK_transform_scaled', N'PK_transform_scaled_bk', N'OBJECT'"));

            migrationBuilder.RenameIndex(
                name: "IX_transform_scaled_calculator_run_id",
                table: "transform_scaled",
                newName: "IX_transform_scaled_bk_calculator_run_id");

            migrationBuilder.RenameTable(
                name: "transform_scaled",
                newName: "transform_scaled_bk");

            migrationBuilder.CreateTable(
                name: "transform_scaled",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    producer_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    submission_period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    is_subtotal = table.Column<bool>(type: "bit", nullable: false),
                    is_total = table.Column<bool>(type: "bit", nullable: false),
                    days_in_submission_period = table.Column<int>(type: "int", nullable: false),
                    days_in_whole_period = table.Column<int>(type: "int", nullable: false),
                    scaled_factor = table.Column<decimal>(type: "decimal(16,12)", precision: 16, scale: 12, nullable: false),
                    material_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    total_reported_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    smcw_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    net_reported_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_total_reported_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_smcw_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_net_reported_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_scaled", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transform_scaled_calculator_run_id",
                table: "transform_scaled",
                column: "calculator_run_id");

            migrationBuilder.Sql(Exec("""
                INSERT INTO dbo.transform_scaled (
                    calculator_run_id, producer_id, subsidiary_id, producer_name, trading_name, submission_period, level,
                    is_subtotal, is_total, days_in_submission_period, days_in_whole_period, scaled_factor, material_code,
                    household_tonnage, public_bin_tonnage, hdc_tonnage, total_reported_tonnage, smcw_tonnage, net_reported_tonnage,
                    scaled_household_tonnage, scaled_public_bin_tonnage, scaled_hdc_tonnage, scaled_total_reported_tonnage, scaled_smcw_tonnage, scaled_net_reported_tonnage)
                SELECT p.calculator_run_id, p.producer_id, p.subsidiary_id, p.producer_name, p.trading_name, p.submission_period, p.level,
                       p.is_subtotal, 0, p.days_in_submission_period, p.days_in_whole_period, p.scaled_factor, p.material_code,
                       p.hh, p.pb, p.hdc, p.hh + p.pb + p.hdc, p.cw, p.hh + p.pb + p.hdc - p.cw,
                       p.scaled_hh, p.scaled_pb, p.scaled_hdc, p.scaled_hh + p.scaled_pb + p.scaled_hdc, p.scaled_cw, p.scaled_hh + p.scaled_pb + p.scaled_hdc - p.scaled_cw
                FROM (
                    SELECT b.calculator_run_id, b.producer_id, b.subsidiary_id, b.producer_name, b.trading_name, b.submission_period, b.level,
                           b.is_subtotal, b.days_in_submission_period, b.days_in_whole_period, b.scaled_factor, m.code AS material_code,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'HH' THEN b.tonnage END), 0)                         AS hh,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'PB' THEN b.tonnage END), 0)                         AS pb,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'HDC' AND m.code = 'GL' THEN b.tonnage END), 0)        AS hdc,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'CW' THEN b.tonnage END), 0)                         AS cw,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'HH' THEN b.scaled_tonnage END), 0)                  AS scaled_hh,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'PB' THEN b.scaled_tonnage END), 0)                  AS scaled_pb,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'HDC' AND m.code = 'GL' THEN b.scaled_tonnage END), 0) AS scaled_hdc,
                           ISNULL(SUM(CASE WHEN b.packaging_type = 'CW' THEN b.scaled_tonnage END), 0)                  AS scaled_cw
                    FROM dbo.transform_scaled_bk b
                    JOIN dbo.material m ON m.id = b.material_id
                    GROUP BY b.calculator_run_id, b.producer_id, b.subsidiary_id, b.producer_name, b.trading_name, b.submission_period, b.level,
                             b.is_subtotal, b.days_in_submission_period, b.days_in_whole_period, b.scaled_factor, m.code
                ) p
                """));

            migrationBuilder.Sql(Exec("""
                INSERT INTO dbo.transform_scaled (
                    calculator_run_id, producer_id, subsidiary_id, producer_name, trading_name, submission_period, level,
                    is_subtotal, is_total, days_in_submission_period, days_in_whole_period, scaled_factor, material_code,
                    household_tonnage, public_bin_tonnage, hdc_tonnage, total_reported_tonnage, smcw_tonnage, net_reported_tonnage,
                    scaled_household_tonnage, scaled_public_bin_tonnage, scaled_hdc_tonnage, scaled_total_reported_tonnage, scaled_smcw_tonnage, scaled_net_reported_tonnage)
                SELECT calculator_run_id, 0, NULL, NULL, NULL, '', '',
                       0, 1, 0, 0, 0, material_code,
                       SUM(household_tonnage), SUM(public_bin_tonnage), SUM(hdc_tonnage), SUM(total_reported_tonnage), SUM(smcw_tonnage), SUM(net_reported_tonnage),
                       SUM(scaled_household_tonnage), SUM(scaled_public_bin_tonnage), SUM(scaled_hdc_tonnage), SUM(scaled_total_reported_tonnage), SUM(scaled_smcw_tonnage), SUM(scaled_net_reported_tonnage)
                FROM dbo.transform_scaled
                WHERE is_subtotal = 0 AND is_total = 0
                GROUP BY calculator_run_id, material_code
                """));

            // Down leaves these: the previous exporter always shows the level 1 row's proportions instead.
            migrationBuilder.Sql(Exec("""
                UPDATE c
                SET h2_ram_proportions_red           = p.h2_ram_proportions_red,
                    h2_ram_proportions_amber         = p.h2_ram_proportions_amber,
                    h2_ram_proportions_green         = p.h2_ram_proportions_green,
                    h2_ram_proportions_red_medical   = p.h2_ram_proportions_red_medical,
                    h2_ram_proportions_amber_medical = p.h2_ram_proportions_amber_medical,
                    h2_ram_proportions_green_medical = p.h2_ram_proportions_green_medical
                FROM dbo.transform_projected_h1 c
                JOIN dbo.transform_projected_h1 p
                  ON p.calculator_run_id = c.calculator_run_id
                 AND p.producer_id = c.producer_id
                 AND p.material_code = c.material_code
                 AND p.level = '1'
                WHERE c.level <> '1'
                """));

            foreach (var column in PartialObligationColumns)
                migrationBuilder.AddColumn<decimal>(name: column, table: "transform_partial", type: "decimal(18,3)", precision: 18, scale: 3, nullable: true);

            // The partial tonnages are summed from the rows the fees were worked out on; runs without those rows fall back to tonnage x factor.
            migrationBuilder.Sql(Exec("""
                UPDATE t
                SET partial_household_tonnage = CASE WHEN fees.has_rows = 1 THEN ISNULL(f.household, 0) ELSE CASE WHEN t.household_tonnage_red IS NOT NULL THEN ROUND(t.household_tonnage_red * t.obligated_factor, 3) + ROUND(t.household_tonnage_amber * t.obligated_factor, 3) + ROUND(t.household_tonnage_green * t.obligated_factor, 3) + ROUND(t.household_tonnage_red_medical * t.obligated_factor, 3) + ROUND(t.household_tonnage_amber_medical * t.obligated_factor, 3) + ROUND(t.household_tonnage_green_medical * t.obligated_factor, 3) ELSE ROUND(t.household_tonnage * t.obligated_factor, 3) END END,
                    partial_household_tonnage_red = CASE WHEN t.household_tonnage_red IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_red, 0) ELSE ROUND(t.household_tonnage_red * t.obligated_factor, 3) END,
                    partial_household_tonnage_amber = CASE WHEN t.household_tonnage_amber IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_amber, 0) ELSE ROUND(t.household_tonnage_amber * t.obligated_factor, 3) END,
                    partial_household_tonnage_green = CASE WHEN t.household_tonnage_green IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_green, 0) ELSE ROUND(t.household_tonnage_green * t.obligated_factor, 3) END,
                    partial_household_tonnage_red_medical = CASE WHEN t.household_tonnage_red_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_red_medical, 0) ELSE ROUND(t.household_tonnage_red_medical * t.obligated_factor, 3) END,
                    partial_household_tonnage_amber_medical = CASE WHEN t.household_tonnage_amber_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_amber_medical, 0) ELSE ROUND(t.household_tonnage_amber_medical * t.obligated_factor, 3) END,
                    partial_household_tonnage_green_medical = CASE WHEN t.household_tonnage_green_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.household_green_medical, 0) ELSE ROUND(t.household_tonnage_green_medical * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage = CASE WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin, 0) ELSE CASE WHEN t.public_bin_tonnage_red IS NOT NULL THEN ROUND(t.public_bin_tonnage_red * t.obligated_factor, 3) + ROUND(t.public_bin_tonnage_amber * t.obligated_factor, 3) + ROUND(t.public_bin_tonnage_green * t.obligated_factor, 3) + ROUND(t.public_bin_tonnage_red_medical * t.obligated_factor, 3) + ROUND(t.public_bin_tonnage_amber_medical * t.obligated_factor, 3) + ROUND(t.public_bin_tonnage_green_medical * t.obligated_factor, 3) ELSE ROUND(t.public_bin_tonnage * t.obligated_factor, 3) END END,
                    partial_public_bin_tonnage_red = CASE WHEN t.public_bin_tonnage_red IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_red, 0) ELSE ROUND(t.public_bin_tonnage_red * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage_amber = CASE WHEN t.public_bin_tonnage_amber IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_amber, 0) ELSE ROUND(t.public_bin_tonnage_amber * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage_green = CASE WHEN t.public_bin_tonnage_green IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_green, 0) ELSE ROUND(t.public_bin_tonnage_green * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage_red_medical = CASE WHEN t.public_bin_tonnage_red_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_red_medical, 0) ELSE ROUND(t.public_bin_tonnage_red_medical * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage_amber_medical = CASE WHEN t.public_bin_tonnage_amber_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_amber_medical, 0) ELSE ROUND(t.public_bin_tonnage_amber_medical * t.obligated_factor, 3) END,
                    partial_public_bin_tonnage_green_medical = CASE WHEN t.public_bin_tonnage_green_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.public_bin_green_medical, 0) ELSE ROUND(t.public_bin_tonnage_green_medical * t.obligated_factor, 3) END,
                    partial_hdc_tonnage = CASE WHEN fees.has_rows = 1 THEN CASE WHEN t.hdc_tonnage IS NULL THEN NULL ELSE ISNULL(f.hdc, 0) END ELSE CASE WHEN t.hdc_tonnage_red IS NOT NULL THEN ROUND(t.hdc_tonnage_red * t.obligated_factor, 3) + ROUND(t.hdc_tonnage_amber * t.obligated_factor, 3) + ROUND(t.hdc_tonnage_green * t.obligated_factor, 3) + ROUND(t.hdc_tonnage_red_medical * t.obligated_factor, 3) + ROUND(t.hdc_tonnage_amber_medical * t.obligated_factor, 3) + ROUND(t.hdc_tonnage_green_medical * t.obligated_factor, 3) WHEN t.hdc_tonnage IS NULL THEN NULL ELSE ROUND(t.hdc_tonnage * t.obligated_factor, 3) END END,
                    partial_hdc_tonnage_red = CASE WHEN t.hdc_tonnage_red IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_red, 0) ELSE ROUND(t.hdc_tonnage_red * t.obligated_factor, 3) END,
                    partial_hdc_tonnage_amber = CASE WHEN t.hdc_tonnage_amber IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_amber, 0) ELSE ROUND(t.hdc_tonnage_amber * t.obligated_factor, 3) END,
                    partial_hdc_tonnage_green = CASE WHEN t.hdc_tonnage_green IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_green, 0) ELSE ROUND(t.hdc_tonnage_green * t.obligated_factor, 3) END,
                    partial_hdc_tonnage_red_medical = CASE WHEN t.hdc_tonnage_red_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_red_medical, 0) ELSE ROUND(t.hdc_tonnage_red_medical * t.obligated_factor, 3) END,
                    partial_hdc_tonnage_amber_medical = CASE WHEN t.hdc_tonnage_amber_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_amber_medical, 0) ELSE ROUND(t.hdc_tonnage_amber_medical * t.obligated_factor, 3) END,
                    partial_hdc_tonnage_green_medical = CASE WHEN t.hdc_tonnage_green_medical IS NULL THEN NULL WHEN fees.has_rows = 1 THEN ISNULL(f.hdc_green_medical, 0) ELSE ROUND(t.hdc_tonnage_green_medical * t.obligated_factor, 3) END,
                    partial_smcw_tonnage = CASE WHEN fees.has_rows = 1 THEN ISNULL(f.smcw, 0) ELSE ROUND(t.smcw_tonnage * t.obligated_factor, 3) END
                FROM dbo.transform_partial t
                OUTER APPLY (
                    SELECT MIN(pd.id) AS producer_detail_id
                    FROM dbo.producer_detail pd
                    WHERE pd.calculator_run_id = t.calculator_run_id AND pd.producer_id = t.producer_id
                      AND ISNULL(pd.subsidiary_id, '') = ISNULL(t.subsidiary_id, '')
                ) d
                OUTER APPLY (
                    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.producer_material_packaging x WHERE x.producer_detail_id = d.producer_detail_id) THEN 1 ELSE 0 END AS has_rows
                ) fees
                OUTER APPLY (
                    SELECT SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage END) AS household,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_red END) AS household_red,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_amber END) AS household_amber,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_green END) AS household_green,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_red_medical END) AS household_red_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_amber_medical END) AS household_amber_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'HH' THEN pmp.packaging_tonnage_green_medical END) AS household_green_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage END) AS public_bin,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_red END) AS public_bin_red,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_amber END) AS public_bin_amber,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_green END) AS public_bin_green,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_red_medical END) AS public_bin_red_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_amber_medical END) AS public_bin_amber_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'PB' THEN pmp.packaging_tonnage_green_medical END) AS public_bin_green_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage END) AS hdc,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_red END) AS hdc_red,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_amber END) AS hdc_amber,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_green END) AS hdc_green,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_red_medical END) AS hdc_red_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_amber_medical END) AS hdc_amber_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'HDC' THEN pmp.packaging_tonnage_green_medical END) AS hdc_green_medical,
                           SUM(CASE WHEN pmp.packaging_type = 'CW' THEN pmp.packaging_tonnage END) AS smcw
                    FROM dbo.producer_material_packaging pmp
                    JOIN dbo.material m ON m.id = pmp.material_id
                    WHERE pmp.producer_detail_id = d.producer_detail_id AND m.code = t.material_code
                ) f
                """));

            foreach (var column in RequiredPartialObligationColumns)
                migrationBuilder.Sql(Exec($"ALTER TABLE dbo.transform_partial ALTER COLUMN {column} decimal(18,3) NOT NULL"));

            SetCountryTotals(migrationBuilder, remove: false);

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
            SetCountryTotals(migrationBuilder, remove: true);

            migrationBuilder.RenameTable(
                name: "calc_result_smcw_bk",
                newName: "calc_result_smcw");

            migrationBuilder.RenameTable(
                name: "calc_result_smcw_producer_bk",
                newName: "calc_result_smcw_producer");

            migrationBuilder.DropTable(
                name: "transform_scaled");

            migrationBuilder.RenameTable(
                name: "transform_scaled_bk",
                newName: "transform_scaled");

            migrationBuilder.RenameIndex(
                name: "IX_transform_scaled_bk_calculator_run_id",
                table: "transform_scaled",
                newName: "IX_transform_scaled_calculator_run_id");

            migrationBuilder.Sql(Exec("EXEC sp_rename N'dbo.PK_transform_scaled_bk', N'PK_transform_scaled', N'OBJECT'"));

            foreach (var column in PartialObligationColumns)
                migrationBuilder.DropColumn(name: column, table: "transform_partial");

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

            migrationBuilder.Sql(Exec("""
                UPDATE dbo.calc_result_one_plus_four_apportionment
                SET one_plus_four_apppointment = JSON_MODIFY(JSON_MODIFY(one_plus_four_apppointment, '$.TotalOnePlusFour', NULL), '$.OnePlusFourApportionment', NULL)
                """));

            migrationBuilder.DropColumn(
                name: "total",
                table: "calc_result_modulation");

            migrationBuilder.Sql(Exec("""
                UPDATE x
                SET material_modulations = a.materials_json
                FROM dbo.calc_result_modulation x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(JSON_MODIFY(m.value,
                                '$.ModulationDetail.TotalMaterialTonnages', NULL) AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY CAST(m.[key] AS int)), '') + ']' AS materials_json
                    FROM OPENJSON(x.material_modulations) m
                ) a
                """));
        }

        private static readonly string[] FeeDetailCountryBreakdowns =
        [
            .. new[] { "LADisposalCostsSection1", "CommsCostsSection2a", "CommsCostsSection2b", "CommsCostsSection2c", "SaOperatingCostsSection3", "LaDataPrepSection4", "SaSetupCostsSection5", "TotalBillBreakdown" }
                .Select(section => $"$.{section}.ByCountry"),
            "$.MaterialFees[]$.DisposalFee.FeeWithBadDebtByCountry",
            "$.MaterialFees[]$.CommFee.Costs.ByCountry",
        ];

        // JSON paths of the country breakdowns in each column; "[]" splits an array's path from each element's.
        private static readonly (string Table, string Column, string[] Paths)[] CountryBreakdowns =
        [
            ("calc_result_lapcap_data", "lapcap", ["$.Total", "$.CountryApportionment", "$.MaterialCosts[]$.Cost"]),
            ("calc_result_la_disposal_cost", "la_disposal_cost", ["$.Total.Cost", "$.MaterialCosts[]$.Detail.Cost"]),
            ("calc_result_comms_cost", "comms_cost", ["$.OnePlusFourApportionment", "$.CommsCostUkWide", "$.CommsCostByCountry", "$.Total.Cost", "$.MaterialCosts[]$.CommsCost.Cost"]),
            ("calc_result_one_plus_four_apportionment", "one_plus_four_apppointment", ["$.LaDisposalCost", "$.LADataPrepCharge", "$.TotalOnePlusFour", "$.OnePlusFourApportionment"]),
            ("calc_result_parameter_other_cost", "parameter_other_cost", ["$.SaOperatingCost", "$.LaDataPrepCharge", "$.CountryApportionment", "$.SchemeSetupCost"]),
            ("calc_result_producer_fees", "total", FeeDetailCountryBreakdowns),
            ("calc_result_producer_fee_detail", "detail", FeeDetailCountryBreakdowns),
        ];

        // Adds each country breakdown's Total, summed exactly, to runs stored before the model held it.
        private static void SetCountryTotals(MigrationBuilder migrationBuilder, bool remove)
        {
            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_number(@value decimal(38, 28)) RETURNS nvarchar(50) AS
                BEGIN
                    DECLARE @text nvarchar(50) = CONVERT(nvarchar(50), @value);
                    SET @text = LEFT(@text, LEN(@text) - PATINDEX(N'%[^0]%', REVERSE(@text)) + 1);
                    RETURN CASE WHEN RIGHT(@text, 1) = N'.' THEN LEFT(@text, LEN(@text) - 1) ELSE @text END;
                END
                """));

            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_country_total(@doc nvarchar(max), @path nvarchar(400), @remove bit) RETURNS nvarchar(max) AS
                BEGIN
                    DECLARE @breakdown nvarchar(max) = JSON_QUERY(@doc, @path);
                    IF @breakdown IS NULL RETURN @doc;
                    IF @remove = 1 RETURN JSON_MODIFY(@doc, @path + N'.Total', NULL);
                    IF JSON_VALUE(@breakdown, N'$.Total') IS NOT NULL RETURN @doc;
                    DECLARE @total decimal(38, 28) = (
                        SELECT SUM(CAST([value] AS decimal(38, 28))) FROM OPENJSON(@breakdown)
                        WHERE [key] IN (N'England', N'Wales', N'Scotland', N'NorthernIreland'));
                    RETURN JSON_MODIFY(@doc, @path, JSON_QUERY(STUFF(@breakdown, LEN(@breakdown), 0, N',"Total":' + dbo.ecv931_number(@total))));
                END
                """));

            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_country_totals_in_array(@doc nvarchar(max), @arrayPath nvarchar(400), @path nvarchar(400), @remove bit) RETURNS nvarchar(max) AS
                BEGIN
                    IF JSON_QUERY(@doc, @arrayPath) IS NULL RETURN @doc;
                    DECLARE @elements nvarchar(max) = (
                        SELECT N'[' + ISNULL(STRING_AGG(CAST(dbo.ecv931_country_total([value], @path, @remove) AS nvarchar(max)), N',') WITHIN GROUP (ORDER BY CAST([key] AS int)), N'') + N']'
                        FROM OPENJSON(@doc, @arrayPath));
                    RETURN JSON_MODIFY(@doc, @arrayPath, JSON_QUERY(@elements));
                END
                """));

            // The fee details also hold 1 + 2a + 2b + 2c: the sum of those four sections' country breakdowns.
            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_one_plus_2a_2b_2c_total(@doc nvarchar(max), @remove bit) RETURNS nvarchar(max) AS
                BEGIN
                    IF @remove = 1 RETURN JSON_MODIFY(@doc, N'$.TotalOnePlus2A2B2CWithBadDebt', NULL);
                    IF JSON_VALUE(@doc, N'$.TotalOnePlus2A2B2CWithBadDebt') IS NOT NULL RETURN @doc;
                    DECLARE @total decimal(38, 28) = (
                        SELECT SUM(CAST([value] AS decimal(38, 28))) FROM (
                            SELECT [key], [value] FROM OPENJSON(@doc, N'$.LADisposalCostsSection1.ByCountry')
                            UNION ALL SELECT [key], [value] FROM OPENJSON(@doc, N'$.CommsCostsSection2a.ByCountry')
                            UNION ALL SELECT [key], [value] FROM OPENJSON(@doc, N'$.CommsCostsSection2b.ByCountry')
                            UNION ALL SELECT [key], [value] FROM OPENJSON(@doc, N'$.CommsCostsSection2c.ByCountry')
                        ) c
                        WHERE [key] IN (N'England', N'Wales', N'Scotland', N'NorthernIreland'));
                    RETURN STUFF(@doc, LEN(@doc), 0, N',"TotalOnePlus2A2B2CWithBadDebt":' + dbo.ecv931_number(ISNULL(@total, 0)));
                END
                """));

            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_round_2dp(@doc nvarchar(max), @path nvarchar(400)) RETURNS nvarchar(max) AS
                BEGIN
                    IF JSON_VALUE(@doc, @path) IS NULL RETURN @doc;
                    RETURN JSON_MODIFY(@doc, @path, CAST(CAST(JSON_VALUE(@doc, @path) AS decimal(38, 28)) AS decimal(38, 2)));
                END
                """));

            var flag = remove ? "1" : "0";

            foreach (var (table, column, paths) in CountryBreakdowns)
            {
                var value = paths.Aggregate($"[{column}]", (doc, path) => path.Split("[]") switch
                {
                    [var arrayPath, var elementPath] => $"dbo.ecv931_country_totals_in_array({doc}, N'{arrayPath}', N'{elementPath}', {flag})",
                    _ => $"dbo.ecv931_country_total({doc}, N'{path}', {flag})"
                });

                if (column is "total" or "detail")
                    value = $"dbo.ecv931_one_plus_2a_2b_2c_total({value}, {flag})";

                // Only shown and invoiced at 2 dp; the total row was summed from the unrounded amounts. Down leaves it rounded.
                if (column is "total" or "detail" && !remove)
                    value = $"dbo.ecv931_round_2dp({value}, N'$.BillingInstruction.SuggestedInvoiceAmount')";

                migrationBuilder.Sql(Exec($"UPDATE dbo.{table} SET [{column}] = {value}"));
            }

            foreach (var function in new[] { "ecv931_round_2dp", "ecv931_one_plus_2a_2b_2c_total", "ecv931_country_totals_in_array", "ecv931_country_total", "ecv931_number" })
                migrationBuilder.Sql(Exec($"DROP FUNCTION dbo.{function}"));

            if (!remove)
                RoundShownOnlyFigures(migrationBuilder);
        }

        // Figures only shown go to the precision the files show, after their totals are taken. Down leaves them rounded.
        private static void RoundShownOnlyFigures(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_round_countries_2dp(@doc nvarchar(max), @path nvarchar(400)) RETURNS nvarchar(max) AS
                BEGIN
                    IF JSON_QUERY(@doc, @path) IS NULL RETURN @doc;
                    SET @doc = JSON_MODIFY(@doc, @path + N'.England', CAST(CAST(JSON_VALUE(@doc, @path + N'.England') AS decimal(38, 28)) AS decimal(38, 2)));
                    SET @doc = JSON_MODIFY(@doc, @path + N'.Wales', CAST(CAST(JSON_VALUE(@doc, @path + N'.Wales') AS decimal(38, 28)) AS decimal(38, 2)));
                    SET @doc = JSON_MODIFY(@doc, @path + N'.Scotland', CAST(CAST(JSON_VALUE(@doc, @path + N'.Scotland') AS decimal(38, 28)) AS decimal(38, 2)));
                    SET @doc = JSON_MODIFY(@doc, @path + N'.NorthernIreland', CAST(CAST(JSON_VALUE(@doc, @path + N'.NorthernIreland') AS decimal(38, 28)) AS decimal(38, 2)));
                    RETURN JSON_MODIFY(@doc, @path + N'.Total', CAST(CAST(JSON_VALUE(@doc, @path + N'.Total') AS decimal(38, 28)) AS decimal(38, 2)));
                END
                """));

            migrationBuilder.Sql(Exec("""
                CREATE FUNCTION dbo.ecv931_round_8dp(@doc nvarchar(max), @path nvarchar(400)) RETURNS nvarchar(max) AS
                BEGIN
                    IF JSON_VALUE(@doc, @path) IS NULL RETURN @doc;
                    RETURN JSON_MODIFY(@doc, @path, CAST(CAST(JSON_VALUE(@doc, @path) AS decimal(38, 28)) AS decimal(38, 8)));
                END
                """));

            migrationBuilder.Sql(Exec("""
                UPDATE x
                SET comms_cost = dbo.ecv931_round_8dp(dbo.ecv931_round_countries_2dp(JSON_MODIFY(x.comms_cost, '$.MaterialCosts', JSON_QUERY(m.materials_json)), '$.Total.Cost'), '$.OnePlusFourApportionment.Total')
                FROM dbo.calc_result_comms_cost x
                CROSS APPLY (
                    SELECT '[' + ISNULL(STRING_AGG(CAST(dbo.ecv931_round_countries_2dp(c.value, '$.CommsCost.Cost') AS nvarchar(max)), ',')
                                WITHIN GROUP (ORDER BY CAST(c.[key] AS int)), '') + ']' AS materials_json
                    FROM OPENJSON(x.comms_cost, '$.MaterialCosts') c
                ) m
                WHERE JSON_QUERY(x.comms_cost, '$.MaterialCosts') IS NOT NULL
                """));

            migrationBuilder.Sql(Exec("UPDATE dbo.calc_result_lapcap_data SET lapcap = dbo.ecv931_round_8dp(lapcap, '$.CountryApportionment.Total')"));
            migrationBuilder.Sql(Exec("UPDATE dbo.calc_result_parameter_other_cost SET parameter_other_cost = dbo.ecv931_round_8dp(parameter_other_cost, '$.CountryApportionment.Total')"));
            migrationBuilder.Sql(Exec("UPDATE dbo.calc_result_one_plus_four_apportionment SET one_plus_four_apppointment = dbo.ecv931_round_8dp(one_plus_four_apppointment, '$.OnePlusFourApportionment.Total')"));

            migrationBuilder.Sql(Exec("DROP FUNCTION dbo.ecv931_round_8dp"));
            migrationBuilder.Sql(Exec("DROP FUNCTION dbo.ecv931_round_countries_2dp"));
        }

        private static string Exec(string sql) => $"EXEC(N'{sql.Replace("'", "''")}');";
    }
}
