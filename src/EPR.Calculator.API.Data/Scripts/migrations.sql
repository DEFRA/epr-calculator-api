﻿IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    CREATE TABLE [default_parameter_setting_master] (
        [Id] int NOT NULL IDENTITY,
        [parameter_year] nvarchar(250) NOT NULL,
        [effective_from] datetime2 NOT NULL,
        [effective_to] datetime2 NULL,
        [created_by] nvarchar(400) NOT NULL,
        [created_at] datetime2 NOT NULL,
        CONSTRAINT [PK_default_parameter_setting_master] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    CREATE TABLE [default_parameter_template_master] (
        [parameter_unique_ref] nvarchar(450) NOT NULL,
        [parameter_type] nvarchar(250) NOT NULL,
        [parameter_category] nvarchar(250) NOT NULL,
        [valid_Range_from] decimal(18,2) NOT NULL,
        [valid_Range_to] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_default_parameter_template_master] PRIMARY KEY ([parameter_unique_ref])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    CREATE TABLE [default_parameter_setting_detail] (
        [Id] int NOT NULL IDENTITY,
        [default_parameter_setting_master_id] int NOT NULL,
        [parameter_unique_ref] nvarchar(450) NOT NULL,
        [parameter_value] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_default_parameter_setting_detail] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_default_parameter_setting_detail_default_parameter_setting_master_default_parameter_setting_master_id] FOREIGN KEY ([default_parameter_setting_master_id]) REFERENCES [default_parameter_setting_master] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_default_parameter_setting_detail_default_parameter_template_master_parameter_unique_ref] FOREIGN KEY ([parameter_unique_ref]) REFERENCES [default_parameter_template_master] ([parameter_unique_ref]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] ON;
    EXEC(N'INSERT INTO [default_parameter_template_master] ([parameter_unique_ref], [parameter_category], [parameter_type], [valid_Range_from], [valid_Range_to])
    VALUES (N''BADEBT-P'', N''Communication costs'', N''Aluminium'', 0.0, 999999999.99),
    (N''COMC-AL'', N''Communication costs'', N''Aluminium'', 0.0, 999999999.99),
    (N''COMC-FC'', N''Communication costs'', N''Fibre composite'', 0.0, 999999999.99),
    (N''COMC-GL'', N''Communication costs'', N''Glass'', 0.0, 999999999.99),
    (N''COMC-OT'', N''Communication costs'', N''Other'', 0.0, 999999999.99),
    (N''COMC-PC'', N''Communication costs'', N''Paper or card'', 0.0, 999999999.99),
    (N''COMC-PL'', N''Communication costs'', N''Plastic'', 0.0, 999999999.99),
    (N''COMC-ST'', N''Communication costs'', N''Steel'', 0.0, 999999999.99),
    (N''COMC-WD'', N''Communication costs'', N''Wood'', 0.0, 999999999.99),
    (N''LAPC-ENG'', N''Local authority data preparation costs'', N''England'', 0.0, 999999999.99),
    (N''LAPC-NIR'', N''Local authority data preparation costs'', N''Northern Ireland'', 0.0, 999999999.99),
    (N''LAPC-SCT'', N''Local authority data preparation costs'', N''Scotland'', 0.0, 999999999.99),
    (N''LAPC-WLS'', N''Local authority data preparation costs'', N''Wales'', 0.0, 999999999.99),
    (N''LEVY-ENG'', N''Levy'', N''England'', 0.0, 999999999.99),
    (N''LEVY-NIR'', N''Levy'', N''Northern Ireland'', 0.0, 999999999.99),
    (N''LEVY-SCT'', N''Levy'', N''Scotland'', 0.0, 999999999.99),
    (N''LEVY-WLS'', N''Levy'', N''Wales'', 0.0, 999999999.99),
    (N''LRET-AL'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-FC'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-GL'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-OT'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-PC'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-PL'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-ST'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''LRET-WD'', N''Late reporting tonnage'', N''Aluminium'', 0.0, 999999999.99),
    (N''MATT-AD'', N''Materiality threshold'', N''Amount Decrease'', 0.0, 999999999.99),
    (N''MATT-AI'', N''Materiality threshold'', N''Amount Increase'', 0.0, 999999999.99),
    (N''MATT-PD'', N''Materiality threshold'', N''Percent Decrease'', 0.0, -1000.0),
    (N''MATT-PI'', N''Materiality threshold'', N''Percent Increase'', 0.0, 1000.0),
    (N''SAOC-ENG'', N''Scheme administrator operating costs'', N''England'', 0.0, 999999999.99),
    (N''SAOC-NIR'', N''Scheme administrator operating costs'', N''Northern Ireland'', 0.0, 999999999.99),
    (N''SAOC-SCT'', N''Scheme administrator operating costs'', N''Scotland'', 0.0, 999999999.99),
    (N''SAOC-WLS'', N''Scheme administrator operating costs'', N''Wales'', 0.0, 999999999.99),
    (N''SCSC-ENG'', N''Scheme setup costs'', N''England'', 0.0, 999999999.99),
    (N''SCSC-NIR'', N''Scheme setup costs'', N''Northern Ireland'', 0.0, 999999999.99),
    (N''SCSC-SCT'', N''Scheme setup costs'', N''Scotland'', 0.0, 999999999.99),
    (N''SCSC-WLS'', N''Scheme setup costs'', N''Wales'', 0.0, 999999999.99),
    (N''TONT-AI'', N''Tonnage change threshold'', N''Amount Increase'', 0.0, 999999999.99),
    (N''TONT-DI'', N''Tonnage change threshold'', N''Amount Decrease'', 0.0, 999999999.99),
    (N''TONT-PD'', N''Tonnage change threshold'', N''Percent Decrease'', 0.0, -1000.0),
    (N''TONT-PI'', N''Tonnage change threshold'', N''Percent Increase'', 0.0, 1000.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    CREATE INDEX [IX_default_parameter_setting_detail_default_parameter_setting_master_id] ON [default_parameter_setting_detail] ([default_parameter_setting_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    CREATE INDEX [IX_default_parameter_setting_detail_parameter_unique_ref] ON [default_parameter_setting_detail] ([parameter_unique_ref]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240730085820_AddInitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240730085820_AddInitialMigration', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_type', N'parameter_category', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] ON;
    EXEC(N'INSERT INTO [default_parameter_template_master] ([parameter_unique_ref], [parameter_type], [parameter_category], [valid_Range_from], [valid_Range_to])
    VALUES (N''TONT-AD'', N''Amount Decrease'', N''Tonnage change threshold'', 0.0, 999999999.99)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_type', N'parameter_category', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_setting_detail] SET [parameter_unique_ref] = N''TONT-AD''
    WHERE [parameter_unique_ref] = N''TONT-DI'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN
    EXEC(N'DELETE FROM [default_parameter_template_master]
    WHERE [parameter_unique_ref] = N''TONT-DI'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240731130652_202407311405_UpdateTemplateMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731135218_202407311451_UpdateTemplateMasterValues'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [valid_Range_from] = -1000.0
    WHERE [parameter_unique_ref] = N''MATT-PD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731135218_202407311451_UpdateTemplateMasterValues'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [valid_Range_to] = 0.0
    WHERE [parameter_unique_ref] = N''MATT-PD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731135218_202407311451_UpdateTemplateMasterValues'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [valid_Range_from] = -1000.0
    WHERE [parameter_unique_ref] = N''TONT-PD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731135218_202407311451_UpdateTemplateMasterValues'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [valid_Range_to] = 0.0
    WHERE [parameter_unique_ref] = N''TONT-PD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731135218_202407311451_UpdateTemplateMasterValues'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240731135218_202407311451_UpdateTemplateMasterValues', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Fibre Composite''
    WHERE [parameter_unique_ref] = N''LRET-FC'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Glass''
    WHERE [parameter_unique_ref] = N''LRET-GL'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Other''
    WHERE [parameter_unique_ref] = N''LRET-OT'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Paper Or Card''
    WHERE [parameter_unique_ref] = N''LRET-PC'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Plastic''
    WHERE [parameter_unique_ref] = N''LRET-PL'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Steel''
    WHERE [parameter_unique_ref] = N''LRET-ST'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_type] = N''Wood''
    WHERE [parameter_unique_ref] = N''LRET-WD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731140140_202407311501_UpdateTemplateMasterType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240731140140_202407311501_UpdateTemplateMasterType', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240808120743_UpdateTemplateMaster'
)
BEGIN
    delete dbo.default_parameter_setting_detail where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240808120743_UpdateTemplateMaster'
)
BEGIN
    delete dbo.default_parameter_setting_master where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240808120743_UpdateTemplateMaster'
)
BEGIN
    delete dbo.default_parameter_template_master where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240808120743_UpdateTemplateMaster'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] ON;
    EXEC(N'INSERT INTO [default_parameter_template_master] ([parameter_unique_ref], [parameter_category], [parameter_type], [valid_Range_from], [valid_Range_to])
    VALUES (N''COMC-AL'', N''Aluminium'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-FC'', N''Fibre composite'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-GL'', N''Glass'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-PC'', N''Paper or card'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-PL'', N''Plastic'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-ST'', N''Steel'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-WD'', N''Wood'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-OT'', N''Other'', N''Communication costs'', 0.0, 999999999.99),
    (N''SAOC-ENG'', N''England'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-WLS'', N''Wales'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-SCT'', N''Scotland'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-NIR'', N''Northern Ireland'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''LAPC-ENG'', N''England'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-WLS'', N''Wales'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-SCT'', N''Scotland'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-NIR'', N''Northern Ireland'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''SCSC-ENG'', N''England'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-WLS'', N''Wales'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-SCT'', N''Scotland'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-NIR'', N''Northern Ireland'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''LRET-AL'', N''Aluminium'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-FC'', N''Fibre composite'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-GL'', N''Glass'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-PC'', N''Paper or card'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-PL'', N''Plastic'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-ST'', N''Steel'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-WD'', N''Wood'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-OT'', N''Other'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''BADEBT-P'', N''BadDebt'', N''Bad debt provision percentage'', 0.0, 999.99),
    (N''MATT-AI'', N''Amount Increase'', N''Materiality threshold'', 0.0, 999999999.99),
    (N''MATT-AD'', N''Amount Decrease'', N''Materiality threshold'', -999999999.99, 0.0),
    (N''MATT-PI'', N''Percent Increase'', N''Materiality threshold'', 0.0, 999.99),
    (N''MATT-PD'', N''Percent Decrease'', N''Materiality threshold'', -999.99, 0.0),
    (N''TONT-AI'', N''Amount Increase'', N''Tonnage change threshold'', 0.0, 999999999.99),
    (N''TONT-AD'', N''Amount Decrease'', N''Tonnage change threshold'', -999999999.99, 0.0),
    (N''TONT-PI'', N''Percent Increase'', N''Tonnage change threshold'', 0.0, 999.99),
    (N''TONT-PD'', N''Percent Decrease'', N''Tonnage change threshold'', -999.99, 0.0),
    (N''LEVY-ENG'', N''England'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-WLS'', N''Wales'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-SCT'', N''Scotland'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-NIR'', N''Northern Ireland'', N''Levy'', 0.0, 999999999.99)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240808120743_UpdateTemplateMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240808120743_UpdateTemplateMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809123714_decimalPrecision'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_template_master]') AND [c].[name] = N'valid_Range_to');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_template_master] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [default_parameter_template_master] ALTER COLUMN [valid_Range_to] decimal(18,3) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809123714_decimalPrecision'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_template_master]') AND [c].[name] = N'valid_Range_from');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_template_master] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [default_parameter_template_master] ALTER COLUMN [valid_Range_from] decimal(18,3) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809123714_decimalPrecision'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_setting_detail]') AND [c].[name] = N'parameter_value');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_setting_detail] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [default_parameter_setting_detail] ALTER COLUMN [parameter_value] decimal(18,3) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809123714_decimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240809123714_decimalPrecision', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809131657_UpdateTemplateMasterData'
)
BEGIN
    delete dbo.default_parameter_setting_detail where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809131657_UpdateTemplateMasterData'
)
BEGIN
    delete dbo.default_parameter_setting_master where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809131657_UpdateTemplateMasterData'
)
BEGIN
    delete dbo.default_parameter_template_master where 1=1
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809131657_UpdateTemplateMasterData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] ON;
    EXEC(N'INSERT INTO [default_parameter_template_master] ([parameter_unique_ref], [parameter_category], [parameter_type], [valid_Range_from], [valid_Range_to])
    VALUES (N''COMC-AL'', N''Aluminium'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-FC'', N''Fibre composite'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-GL'', N''Glass'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-PC'', N''Paper or card'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-PL'', N''Plastic'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-ST'', N''Steel'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-WD'', N''Wood'', N''Communication costs'', 0.0, 999999999.99),
    (N''COMC-OT'', N''Other'', N''Communication costs'', 0.0, 999999999.99),
    (N''SAOC-ENG'', N''England'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-WLS'', N''Wales'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-SCT'', N''Scotland'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''SAOC-NIR'', N''Northern Ireland'', N''Scheme administrator operating costs'', 0.0, 999999999.99),
    (N''LAPC-ENG'', N''England'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-WLS'', N''Wales'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-SCT'', N''Scotland'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''LAPC-NIR'', N''Northern Ireland'', N''Local authority data preparation costs'', 0.0, 999999999.99),
    (N''SCSC-ENG'', N''England'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-WLS'', N''Wales'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-SCT'', N''Scotland'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''SCSC-NIR'', N''Northern Ireland'', N''Scheme setup costs'', 0.0, 999999999.99),
    (N''LRET-AL'', N''Aluminium'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-FC'', N''Fibre composite'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-GL'', N''Glass'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-PC'', N''Paper or card'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-PL'', N''Plastic'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-ST'', N''Steel'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-WD'', N''Wood'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''LRET-OT'', N''Other'', N''Late reporting tonnage'', 0.0, 999999999.999),
    (N''BADEBT-P'', N''BadDebt'', N''Bad debt provision percentage'', 0.0, 999.99),
    (N''MATT-AI'', N''Amount Increase'', N''Materiality threshold'', 0.0, 999999999.99),
    (N''MATT-AD'', N''Amount Decrease'', N''Materiality threshold'', -999999999.99, 0.0),
    (N''MATT-PI'', N''Percent Increase'', N''Materiality threshold'', 0.0, 999.99),
    (N''MATT-PD'', N''Percent Decrease'', N''Materiality threshold'', -999.99, 0.0),
    (N''TONT-AI'', N''Amount Increase'', N''Tonnage change threshold'', 0.0, 999999999.99),
    (N''TONT-AD'', N''Amount Decrease'', N''Tonnage change threshold'', -999999999.99, 0.0),
    (N''TONT-PI'', N''Percent Increase'', N''Tonnage change threshold'', 0.0, 999.99),
    (N''TONT-PD'', N''Percent Decrease'', N''Tonnage change threshold'', -999.99, 0.0),
    (N''LEVY-ENG'', N''England'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-WLS'', N''Wales'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-SCT'', N''Scotland'', N''Levy'', 0.0, 999999999.99),
    (N''LEVY-NIR'', N''Northern Ireland'', N''Levy'', 0.0, 999999999.99)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240809131657_UpdateTemplateMasterData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240809131657_UpdateTemplateMasterData', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240814103125_UpdateBadDebtInTemplateMaster'
)
BEGIN
    update dbo.default_parameter_template_master
    set parameter_type = 'Bad debt provision',
    parameter_category = 'Percentage'
    where parameter_type like '%Bad debt provision percentage%'

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240814103125_UpdateBadDebtInTemplateMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240814103125_UpdateBadDebtInTemplateMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240902153316_AddCalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run_classification] (
        [id] int NOT NULL IDENTITY,
        [status] nvarchar(250) NOT NULL,
        [created_by] nvarchar(400) NOT NULL,
        [created_at] datetime2 NOT NULL,
        CONSTRAINT [PK_calculator_run_classification] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240902153316_AddCalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run] (
        [id] int NOT NULL IDENTITY,
        [calculator_run_classification_id] int NOT NULL,
        [name] nvarchar(250) NOT NULL,
        [financial_year] nvarchar(250) NOT NULL,
        [created_by] nvarchar(400) NOT NULL,
        [created_at] datetime2 NOT NULL,
        [updated_by] nvarchar(400) NULL,
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK_calculator_run] PRIMARY KEY ([id]),
        CONSTRAINT [FK_calculator_run_calculator_run_classification_calculator_run_classification_id] FOREIGN KEY ([calculator_run_classification_id]) REFERENCES [calculator_run_classification] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240902153316_AddCalcRunTables'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_at', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] ON;
    EXEC(N'INSERT INTO [calculator_run_classification] ([id], [created_at], [created_by], [status])
    VALUES (1, ''2024-09-02T16:33:15.3358091+01:00'', N''Test User'', N''IN THE QUEUE''),
    (2, ''2024-09-02T16:33:15.3358097+01:00'', N''Test User'', N''RUNNING''),
    (3, ''2024-09-02T16:33:15.3358102+01:00'', N''Test User'', N''UNCLASSIFIED''),
    (4, ''2024-09-02T16:33:15.3358106+01:00'', N''Test User'', N''PLAY''),
    (5, ''2024-09-02T16:33:15.3358110+01:00'', N''Test User'', N''ERROR'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_at', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240902153316_AddCalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_calculator_run_classification_id] ON [calculator_run] ([calculator_run_classification_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240902153316_AddCalcRunTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240902153316_AddCalcRunTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    CREATE TABLE [lapcap_data_master] (
        [id] int NOT NULL IDENTITY,
        [year] nvarchar(50) NOT NULL,
        [effective_from] datetime2 NOT NULL,
        [effective_to] datetime2 NULL,
        [created_by] nvarchar(400) NOT NULL,
        [created_at] datetime2 NOT NULL,
        CONSTRAINT [PK_lapcap_data_master] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    CREATE TABLE [lapcap_data_template_master] (
        [unique_ref] nvarchar(400) NOT NULL,
        [country] nvarchar(400) NOT NULL,
        [material] nvarchar(400) NOT NULL,
        [total_cost_from] decimal(18,2) NOT NULL,
        [total_cost_to] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_lapcap_data_template_master] PRIMARY KEY ([unique_ref])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    CREATE TABLE [lapcap_data_detail] (
        [id] int NOT NULL IDENTITY,
        [lapcap_data_master_id] int NOT NULL,
        [lapcap_data_template_master_unique_ref] nvarchar(400) NOT NULL,
        [total_cost] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_lapcap_data_detail] PRIMARY KEY ([id]),
        CONSTRAINT [FK_lapcap_data_detail_lapcap_data_master_lapcap_data_master_id] FOREIGN KEY ([lapcap_data_master_id]) REFERENCES [lapcap_data_master] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_lapcap_data_detail_lapcap_data_template_master_lapcap_data_template_master_unique_ref] FOREIGN KEY ([lapcap_data_template_master_unique_ref]) REFERENCES [lapcap_data_template_master] ([unique_ref]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T12:05:03.7732558+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T12:05:03.7732561+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T12:05:03.7732563+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T12:05:03.7732565+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T12:05:03.7732567+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    CREATE INDEX [IX_lapcap_data_detail_lapcap_data_master_id] ON [lapcap_data_detail] ([lapcap_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    CREATE UNIQUE INDEX [IX_lapcap_data_detail_lapcap_data_template_master_unique_ref] ON [lapcap_data_detail] ([lapcap_data_template_master_unique_ref]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905110503_LapcapData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240905110503_LapcapData', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T13:15:21.5953009+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T13:15:21.5953012+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T13:15:21.5953014+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T13:15:21.5953017+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-05T13:15:21.5953019+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'unique_ref', N'country', N'material', N'total_cost_from', N'total_cost_to') AND [object_id] = OBJECT_ID(N'[lapcap_data_template_master]'))
        SET IDENTITY_INSERT [lapcap_data_template_master] ON;
    EXEC(N'INSERT INTO [lapcap_data_template_master] ([unique_ref], [country], [material], [total_cost_from], [total_cost_to])
    VALUES (N''ENG-AL'', N''England'', N''Aluminium'', 0.0, 999999999.99),
    (N''ENG-FC'', N''England'', N''Fibre composite'', 0.0, 999999999.99),
    (N''ENG-GL'', N''England'', N''Glass'', 0.0, 999999999.99),
    (N''ENG-OT'', N''England'', N''Other'', 0.0, 999999999.99),
    (N''ENG-PC'', N''England'', N''Paper or card'', 0.0, 999999999.99),
    (N''ENG-PL'', N''England'', N''Plastic'', 0.0, 999999999.99),
    (N''ENG-ST'', N''England'', N''Steel'', 0.0, 999999999.99),
    (N''ENG-WD'', N''England'', N''Wood'', 0.0, 999999999.99),
    (N''NI-AL'', N''NI'', N''Aluminium'', 0.0, 999999999.99),
    (N''NI-FC'', N''NI'', N''Fibre composite'', 0.0, 999999999.99),
    (N''NI-GL'', N''NI'', N''Glass'', 0.0, 999999999.99),
    (N''NI-OT'', N''NI'', N''Other'', 0.0, 999999999.99),
    (N''NI-PC'', N''NI'', N''Paper or card'', 0.0, 999999999.99),
    (N''NI-PL'', N''NI'', N''Plastic'', 0.0, 999999999.99),
    (N''NI-ST'', N''NI'', N''Steel'', 0.0, 999999999.99),
    (N''NI-WD'', N''NI'', N''Wood'', 0.0, 999999999.99),
    (N''SCT-AL'', N''Scotland'', N''Aluminium'', 0.0, 999999999.99),
    (N''SCT-FC'', N''Scotland'', N''Fibre composite'', 0.0, 999999999.99),
    (N''SCT-GL'', N''Scotland'', N''Glass'', 0.0, 999999999.99),
    (N''SCT-OT'', N''Scotland'', N''Other'', 0.0, 999999999.99),
    (N''SCT-PC'', N''Scotland'', N''Paper or card'', 0.0, 999999999.99),
    (N''SCT-PL'', N''Scotland'', N''Plastic'', 0.0, 999999999.99),
    (N''SCT-ST'', N''Scotland'', N''Steel'', 0.0, 999999999.99),
    (N''SCT-WD'', N''Scotland'', N''Wood'', 0.0, 999999999.99),
    (N''WLS-AL'', N''Wales'', N''Aluminium'', 0.0, 999999999.99),
    (N''WLS-FC'', N''Wales'', N''Fibre composite'', 0.0, 999999999.99),
    (N''WLS-GL'', N''Wales'', N''Glass'', 0.0, 999999999.99),
    (N''WLS-OT'', N''Wales'', N''Other'', 0.0, 999999999.99),
    (N''WLS-PC'', N''Wales'', N''Paper or card'', 0.0, 999999999.99),
    (N''WLS-PL'', N''Wales'', N''Plastic'', 0.0, 999999999.99),
    (N''WLS-ST'', N''Wales'', N''Steel'', 0.0, 999999999.99),
    (N''WLS-WD'', N''Wales'', N''Wood'', 0.0, 999999999.99)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'unique_ref', N'country', N'material', N'total_cost_from', N'total_cost_to') AND [object_id] = OBJECT_ID(N'[lapcap_data_template_master]'))
        SET IDENTITY_INSERT [lapcap_data_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240905121521_LapcapDataSeed'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240905121521_LapcapDataSeed', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    DROP INDEX [IX_lapcap_data_detail_lapcap_data_template_master_unique_ref] ON [lapcap_data_detail];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-09T10:58:28.7309507+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-09T10:58:28.7309513+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-09T10:58:28.7309519+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-09T10:58:28.7309523+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-09-09T10:58:28.7309528+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    CREATE INDEX [IX_lapcap_data_detail_lapcap_data_template_master_unique_ref] ON [lapcap_data_detail] ([lapcap_data_template_master_unique_ref]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240909095829_LapcapRelationship'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240909095829_LapcapRelationship', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924094510_DeleteLevyFromDefaultParamaterMaster'
)
BEGIN
    delete d from dbo.default_parameter_setting_detail d
    inner join dbo.default_parameter_template_master m
    on d.parameter_unique_ref = m.parameter_unique_ref
    where m.parameter_type = 'Levy'

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924094510_DeleteLevyFromDefaultParamaterMaster'
)
BEGIN
    delete from dbo.default_parameter_template_master where parameter_type = 'Levy'
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924094510_DeleteLevyFromDefaultParamaterMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240924094510_DeleteLevyFromDefaultParamaterMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924110427_AddCommunicationCostsDefaultParamaterMaster'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] ON;
    EXEC(N'INSERT INTO [default_parameter_template_master] ([parameter_unique_ref], [parameter_category], [parameter_type], [valid_Range_from], [valid_Range_to])
    VALUES (N''COMC-UK'', N''United Kingdom'', N''Communication costs by country'', 0.0, 999999999.99),
    (N''COMC-ENG'', N''England'', N''Communication costs by country'', 0.0, 999999999.99),
    (N''COMC-WLS'', N''Wales'', N''Communication costs by country'', 0.0, 999999999.99),
    (N''COMC-SCT'', N''Scotland'', N''Communication costs by country'', 0.0, 999999999.99),
    (N''COMC-NIR'', N''Northern Ireland'', N''Communication costs by country'', 0.0, 999999999.99)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'parameter_unique_ref', N'parameter_category', N'parameter_type', N'valid_Range_from', N'valid_Range_to') AND [object_id] = OBJECT_ID(N'[default_parameter_template_master]'))
        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924110427_AddCommunicationCostsDefaultParamaterMaster'
)
BEGIN
    update dbo.default_parameter_template_master
    set parameter_type = 'Communication costs by material'
    where parameter_type = 'Communication costs'

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924110427_AddCommunicationCostsDefaultParamaterMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240924110427_AddCommunicationCostsDefaultParamaterMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924194409_UpdateLapcapDataMasterYearColumnName'
)
BEGIN
    EXEC sp_rename N'[dbo].[lapcap_data_master].[year]', N'projection_year', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240924194409_UpdateLapcapDataMasterYearColumnName'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240924194409_UpdateLapcapDataMasterYearColumnName', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [calculator_run_organization_data_master_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [calculator_run_pom_data_master_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [default_parameter_setting_master_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [lapcap_data_master_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run_organization_data_master] (
        [id] int NOT NULL IDENTITY,
        [calendar_year] nvarchar(max) NOT NULL,
        [effective_from] datetime2 NOT NULL,
        [effective_to] datetime2 NULL,
        [created_by] nvarchar(max) NOT NULL,
        [created_at] datetime2 NOT NULL,
        CONSTRAINT [PK_calculator_run_organization_data_master] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run_pom_data_master] (
        [id] int NOT NULL IDENTITY,
        [calendar_year] nvarchar(max) NOT NULL,
        [effective_from] datetime2 NOT NULL,
        [effective_to] datetime2 NULL,
        [created_by] nvarchar(max) NOT NULL,
        [created_at] datetime2 NOT NULL,
        CONSTRAINT [PK_calculator_run_pom_data_master] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [organization_data] (
        [organisation_id] nvarchar(400) NOT NULL,
        [subsidiary_id] nvarchar(400) NOT NULL,
        [organisation_name] nvarchar(400) NOT NULL,
        [load_ts] datetime2 NOT NULL,
        CONSTRAINT [PK_organization_data] PRIMARY KEY ([organisation_id], [subsidiary_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [pom_data] (
        [organisation_id] nvarchar(400) NOT NULL,
        [subsidiary_id] nvarchar(400) NOT NULL,
        [submission_period] nvarchar(400) NOT NULL,
        [packaging_activity] nvarchar(400) NULL,
        [packaging_type] nvarchar(400) NULL,
        [packaging_class] nvarchar(400) NULL,
        [packaging_material] nvarchar(400) NULL,
        [packaging_material_weight] nvarchar(400) NULL,
        [load_ts] datetime2 NOT NULL,
        CONSTRAINT [PK_pom_data] PRIMARY KEY ([organisation_id], [subsidiary_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run_organization_data_detail] (
        [organisation_id] nvarchar(400) NOT NULL,
        [subsidiary_id] nvarchar(400) NOT NULL,
        [organisation_name] nvarchar(400) NOT NULL,
        [load_ts] datetime2 NOT NULL,
        [calculator_run_organization_data_master_id] int NOT NULL,
        CONSTRAINT [PK_calculator_run_organization_data_detail] PRIMARY KEY ([organisation_id], [subsidiary_id]),
        CONSTRAINT [FK_calculator_run_organization_data_detail_calculator_run_organization_data_master_calculator_run_organization_data_master_id] FOREIGN KEY ([calculator_run_organization_data_master_id]) REFERENCES [calculator_run_organization_data_master] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE TABLE [calculator_run_pom_data_detail] (
        [organisation_id] nvarchar(400) NOT NULL,
        [subsidiary_id] nvarchar(400) NOT NULL,
        [submission_period] nvarchar(400) NOT NULL,
        [packaging_activity] nvarchar(400) NULL,
        [packaging_type] nvarchar(400) NULL,
        [packaging_class] nvarchar(400) NULL,
        [packaging_material] nvarchar(400) NULL,
        [packaging_material_weight] nvarchar(400) NULL,
        [load_ts] datetime2 NOT NULL,
        [calculator_run_pom_data_master_id] int NOT NULL,
        CONSTRAINT [PK_calculator_run_pom_data_detail] PRIMARY KEY ([organisation_id], [subsidiary_id]),
        CONSTRAINT [FK_calculator_run_pom_data_detail_calculator_run_pom_data_master_calculator_run_pom_data_master_id] FOREIGN KEY ([calculator_run_pom_data_master_id]) REFERENCES [calculator_run_pom_data_master] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-04T14:46:49.1790461+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-04T14:46:49.1790464+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-04T14:46:49.1790466+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-04T14:46:49.1790469+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-04T14:46:49.1790471+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_calculator_run_organization_data_master_id] ON [calculator_run] ([calculator_run_organization_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_calculator_run_pom_data_master_id] ON [calculator_run] ([calculator_run_pom_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_default_parameter_setting_master_id] ON [calculator_run] ([default_parameter_setting_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_lapcap_data_master_id] ON [calculator_run] ([lapcap_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_organization_data_detail_calculator_run_organization_data_master_id] ON [calculator_run_organization_data_detail] ([calculator_run_organization_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    CREATE INDEX [IX_calculator_run_pom_data_detail_calculator_run_pom_data_master_id] ON [calculator_run_pom_data_detail] ([calculator_run_pom_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id] FOREIGN KEY ([calculator_run_organization_data_master_id]) REFERENCES [calculator_run_organization_data_master] ([id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id] FOREIGN KEY ([calculator_run_pom_data_master_id]) REFERENCES [calculator_run_pom_data_master] ([id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_default_parameter_setting_master_default_parameter_setting_master_id] FOREIGN KEY ([default_parameter_setting_master_id]) REFERENCES [default_parameter_setting_master] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_lapcap_data_master_lapcap_data_master_id] FOREIGN KEY ([lapcap_data_master_id]) REFERENCES [lapcap_data_master] ([id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241004134649_CalcRunTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241004134649_CalcRunTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-07T09:25:51.2638112+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-07T09:25:51.2638115+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-07T09:25:51.2638118+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-07T09:25:51.2638120+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-07T09:25:51.2638127+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241007082551_CalcRunMinorChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241007082551_CalcRunMinorChanges', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    ALTER TABLE [pom_data] DROP CONSTRAINT [PK_pom_data];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    ALTER TABLE [organization_data] DROP CONSTRAINT [PK_organization_data];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC sp_rename N'[organization_data]', N'organisation_data';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'subsidiary_id');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [subsidiary_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'organisation_id');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [organisation_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'subsidiary_id');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [organisation_data] ALTER COLUMN [subsidiary_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'organisation_id');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [organisation_data] ALTER COLUMN [organisation_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-18T12:04:37.8212280+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-18T12:04:37.8212289+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-18T12:04:37.8212296+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-18T12:04:37.8212302+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-18T12:04:37.8212308+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018110438_OrganisationAndPomChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241018110438_OrganisationAndPomChanges', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018130224_UpdateOtherMaterialsDefaultParameterMaster'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_category] = N''Other materials''
    WHERE [parameter_unique_ref] = N''COMC-OT'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241018130224_UpdateOtherMaterialsDefaultParameterMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241018130224_UpdateOtherMaterialsDefaultParameterMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021102314_UpdateOtherMaterialsLateReportingTonnage'
)
BEGIN
    EXEC(N'UPDATE [default_parameter_template_master] SET [parameter_category] = N''Other materials''
    WHERE [parameter_unique_ref] = N''LRET-OT'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021102314_UpdateOtherMaterialsLateReportingTonnage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241021102314_UpdateOtherMaterialsLateReportingTonnage', N'8.0.7');
END;
GO

COMMIT;
GO

