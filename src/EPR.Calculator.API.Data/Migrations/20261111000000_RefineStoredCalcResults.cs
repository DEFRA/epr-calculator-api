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

        private static string Exec(string sql) => $"EXEC(N'{sql.Replace("'", "''")}');";
    }
}
