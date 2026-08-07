BEGIN TRANSACTION;
GO

DROP TABLE [calc_result_cancelled_producer];
GO

DROP TABLE [calc_result_comms_cost];
GO

DROP TABLE [calc_result_la_disposal_cost];
GO

DROP TABLE [calc_result_lapcap_data];
GO

DROP TABLE [calc_result_late_reporting_tonnage];
GO

DROP TABLE [calc_result_one_plus_four_apportionment];
GO

DROP TABLE [calc_result_parameter_other_cost];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260722145707_AddCostDataAndCancelledProducerTables';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [calc_result_modulation];
GO

DROP TABLE [calc_result_smcw_producer];
GO

DROP TABLE [calc_result_smcw];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260717111524_AddModulationAndSmcw';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [calc_result_producer_fee_detail];
GO

DROP TABLE [calc_result_producer_fees];
GO

EXEC sp_rename N'[producer_material_packaging].[IX_producer_material_packaging_material_id]', N'IX_producer_reported_material_projected_material_id', N'INDEX';
GO

EXEC sp_rename N'[producer_material_packaging].[IX_producer_material_packaging_producer_detail_id]', N'IX_producer_reported_material_projected_producer_detail_id', N'INDEX';
GO

EXEC sp_rename N'[producer_material_packaging]', N'producer_reported_material_projected';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260716130437_AddProducerFeeTables';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'COFF-DT';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_setting_detail]
WHERE [parameter_unique_ref] = N'COFF-DT';
SELECT @@ROWCOUNT;

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_setting_detail]') AND [c].[name] = N'parameter_value');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_setting_detail] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [default_parameter_setting_detail] ALTER COLUMN [parameter_value] decimal(18,3) NOT NULL;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260715154551_AddCutOffDateDefaultParameter';
GO

COMMIT;
GO

