IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
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

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [PK_calculator_run_pom_data_detail];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [PK_calculator_run_organization_data_detail];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_pom_data_detail] ADD [Id] int NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [Id] int NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_pom_data_detail] ADD CONSTRAINT [PK_calculator_run_pom_data_detail] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD CONSTRAINT [PK_calculator_run_organization_data_detail] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T12:07:01.5507348+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T12:07:01.5507355+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T12:07:01.5507360+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T12:07:01.5507365+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T12:07:01.5507370+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021110702_CalcRunPomAndOrganisationRemoveKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241021110702_CalcRunPomAndOrganisationRemoveKeys', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'subsidiary_id');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [subsidiary_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'organisation_id');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [organisation_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'subsidiary_id');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [subsidiary_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'organisation_id');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [organisation_id] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T14:00:55.5936723+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T14:00:55.5936733+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T14:00:55.5936740+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T14:00:55.5936746+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-21T14:00:55.5936754+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241021130056_OrganisationIdAndSubsidaryIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241021130056_OrganisationIdAndSubsidaryIdNullable', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'organisation_id');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [organisation_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'organisation_id');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [organisation_data] ALTER COLUMN [organisation_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'organisation_id');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [organisation_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'organisation_id');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [organisation_id] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T13:55:26.5895014+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T13:55:26.5895017+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T13:55:26.5895019+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T13:55:26.5895022+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T13:55:26.5895024+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024125526_OrganisationIDToInt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241024125526_OrganisationIDToInt', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'packaging_material_weight');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [packaging_material_weight] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'packaging_material');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [packaging_material] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'packaging_material_weight');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [packaging_material_weight] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T14:43:19.9617112+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T14:43:19.9617118+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T14:43:19.9617123+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T14:43:19.9617128+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-24T14:43:19.9617132+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241024134320_PackagingMaterialWeightToDouble'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241024134320_PackagingMaterialWeightToDouble', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [cost_type] (
        [id] int NOT NULL IDENTITY,
        [code] nvarchar(400) NOT NULL,
        [name] nvarchar(400) NOT NULL,
        [description] nvarchar(2000) NULL,
        CONSTRAINT [PK_cost_type] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [country] (
        [id] int NOT NULL IDENTITY,
        [code] nvarchar(400) NOT NULL,
        [name] nvarchar(400) NOT NULL,
        [description] nvarchar(2000) NULL,
        CONSTRAINT [PK_country] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [material] (
        [id] int NOT NULL IDENTITY,
        [code] nvarchar(400) NOT NULL,
        [name] nvarchar(400) NOT NULL,
        [description] nvarchar(2000) NULL,
        CONSTRAINT [PK_material] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [producer_detail] (
        [id] int NOT NULL IDENTITY,
        [producer_id] int NOT NULL,
        [subsidiary_id] nvarchar(400) NULL,
        [producer_name] nvarchar(400) NULL,
        [calculator_run_id] int NOT NULL,
        CONSTRAINT [PK_producer_detail] PRIMARY KEY ([id]),
        CONSTRAINT [FK_producer_detail_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [country_apportionment] (
        [id] int NOT NULL IDENTITY,
        [apportionment] decimal(18,2) NOT NULL,
        [country_id] int NOT NULL,
        [cost_type_id] int NOT NULL,
        [calculator_run_id] int NOT NULL,
        CONSTRAINT [PK_country_apportionment] PRIMARY KEY ([id]),
        CONSTRAINT [FK_country_apportionment_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_country_apportionment_cost_type_cost_type_id] FOREIGN KEY ([cost_type_id]) REFERENCES [cost_type] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_country_apportionment_country_country_id] FOREIGN KEY ([country_id]) REFERENCES [country] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE TABLE [producer_reported_material] (
        [id] int NOT NULL IDENTITY,
        [material_id] int NOT NULL,
        [producer_detail_id] int NOT NULL,
        [packaging_type] nvarchar(400) NOT NULL,
        [packaging_tonnage] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_producer_reported_material] PRIMARY KEY ([id]),
        CONSTRAINT [FK_producer_reported_material_material_material_id] FOREIGN KEY ([material_id]) REFERENCES [material] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_producer_reported_material_producer_detail_producer_detail_id] FOREIGN KEY ([producer_detail_id]) REFERENCES [producer_detail] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-25T15:04:19.9291118+01:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-25T15:04:19.9291125+01:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-25T15:04:19.9291130+01:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-25T15:04:19.9291135+01:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-25T15:04:19.9291140+01:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_country_apportionment_calculator_run_id] ON [country_apportionment] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_country_apportionment_cost_type_id] ON [country_apportionment] ([cost_type_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_country_apportionment_country_id] ON [country_apportionment] ([country_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_producer_detail_calculator_run_id] ON [producer_detail] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_producer_reported_material_material_id] ON [producer_reported_material] ([material_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    CREATE INDEX [IX_producer_reported_material_producer_detail_id] ON [producer_reported_material] ([producer_detail_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241025140420_CalculationResultsTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241025140420_CalculationResultsTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028092305_CreateMasterDataForCalcResultsTables'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[cost_type]'))
        SET IDENTITY_INSERT [cost_type] ON;
    EXEC(N'INSERT INTO [cost_type] ([id], [code], [name], [description])
    VALUES (1, N''1'', N''Fee for LA Disposal Costs'', N''Fee for LA Disposal Costs''),
    (2, N''4'', N''LA Data Prep Charge'', N''LA Data Prep Charge'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[cost_type]'))
        SET IDENTITY_INSERT [cost_type] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028092305_CreateMasterDataForCalcResultsTables'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[country]'))
        SET IDENTITY_INSERT [country] ON;
    EXEC(N'INSERT INTO [country] ([id], [code], [name], [description])
    VALUES (1, N''ENG'', N''England'', N''England''),
    (2, N''WLS'', N''Wales'', N''Wales''),
    (3, N''SCT'', N''Scotland'', N''Scotland''),
    (4, N''NIR'', N''Northern Ireland'', N''Northern Ireland'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[country]'))
        SET IDENTITY_INSERT [country] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028092305_CreateMasterDataForCalcResultsTables'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[material]'))
        SET IDENTITY_INSERT [material] ON;
    EXEC(N'INSERT INTO [material] ([id], [code], [name], [description])
    VALUES (1, N''AL'', N''Aluminium'', N''Aluminium''),
    (2, N''FC'', N''Fibre composite'', N''Fibre composite''),
    (3, N''GL'', N''Glass'', N''Glass''),
    (4, N''PC'', N''Paper or card'', N''Paper or card''),
    (5, N''PL'', N''Plastic'', N''Plastic''),
    (6, N''ST'', N''Steel'', N''Steel''),
    (7, N''WD'', N''Wood'', N''Wood''),
    (8, N''OT'', N''Other materials'', N''Other materials'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'code', N'name', N'description') AND [object_id] = OBJECT_ID(N'[material]'))
        SET IDENTITY_INSERT [material] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028092305_CreateMasterDataForCalcResultsTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241028092305_CreateMasterDataForCalcResultsTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    ALTER TABLE [pom_data] ADD [submission_period_desc] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [submission_period_desc] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    ALTER TABLE [calculator_run_pom_data_detail] ADD [submission_period_desc] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [submission_period_desc] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-28T11:43:13.4928134+00:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-28T11:43:13.4928137+00:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-28T11:43:13.4928139+00:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-28T11:43:13.4928141+00:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-28T11:43:13.4928143+00:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241028114313_AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    ALTER TABLE [default_parameter_setting_master] ADD [parameter_filename] nvarchar(256) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-29T11:03:02.8507790+00:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-29T11:03:02.8507797+00:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-29T11:03:02.8507802+00:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-29T11:03:02.8507808+00:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-10-29T11:03:02.8507813+00:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241029110303_AddNewColumnParameterFileNameToDefaultParamSettingMasterTable', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    ALTER TABLE [lapcap_data_master] ADD [lapcap_filename] nvarchar(256) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-11-04T13:00:24.9889020+00:00''
    WHERE [id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-11-04T13:00:24.9889023+00:00''
    WHERE [id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-11-04T13:00:24.9889025+00:00''
    WHERE [id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-11-04T13:00:24.9889027+00:00''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [created_at] = ''2024-11-04T13:00:24.9889029+00:00''
    WHERE [id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241104130025_AddNewColumnLapcapFileNameToLapcapDataMaster', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241114200611_RemoveCreateAtCalculatorRunClassification'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_classification]') AND [c].[name] = N'created_at');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_classification] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [calculator_run_classification] DROP COLUMN [created_at];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241114200611_RemoveCreateAtCalculatorRunClassification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241114200611_RemoveCreateAtCalculatorRunClassification', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241114200729_UpdateOtherParamLapcap'
)
BEGIN
    update dbo.lapcap_data_template_master 
    set material = 'Other materials'
    where material like 'Other'

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241114200729_UpdateOtherParamLapcap'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241114200729_UpdateOtherParamLapcap', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241115161403_AddPackagingTonnagePrecision'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [producer_reported_material] ALTER COLUMN [packaging_tonnage] decimal(18,3) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241115161403_AddPackagingTonnagePrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241115161403_AddPackagingTonnagePrecision', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-AL'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-FC'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-GL'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-OT'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-PC'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-PL'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-ST'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    EXEC(N'UPDATE [lapcap_data_template_master] SET [country] = N''Northern Ireland''
    WHERE [unique_ref] = N''NI-WD'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241120130054_LapCapNorthernIrelandRename'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241120130054_LapCapNorthernIrelandRename', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241129145454_AddNewClassficationStatus'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] ON;
    EXEC(N'INSERT INTO [calculator_run_classification] ([status], [created_by])
    VALUES (N''DELETED'', N''System User'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241129145454_AddNewClassficationStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241129145454_AddNewClassficationStatus', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'submission_period_desc');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [submission_period_desc] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'submission_period');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [pom_data] ALTER COLUMN [submission_period] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'submission_period_desc');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [submission_period_desc] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'submission_period');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [calculator_run_pom_data_detail] ALTER COLUMN [submission_period] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'submission_period_desc');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [submission_period_desc] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250117164554_AllowNullSubmissionPeriodForPom'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250117164554_AllowNullSubmissionPeriodForPom', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250122092344_PomAndOrganisationProcedures'
)
BEGIN
    declare @Sql varchar(max)
    SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunPom]
    (
        -- Add the parameters for the stored procedure here
        @RunId int,
    	@calendarYear varchar(400),
    	@createdBy varchar(400)
    )
    AS
    BEGIN
        -- SET NOCOUNT ON added to prevent extra result sets from
        -- interfering with SELECT statements.
        SET NOCOUNT ON

    	declare @DateNow datetime, @pomDataMasterid int
    	SET @DateNow = GETDATE()

    	declare @oldCalcRunPomMasterId int
        SET @oldCalcRunPomMasterId = (select top 1 id from dbo.calculator_run_pom_data_master order by id desc)
    	Update calculator_run_pom_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunPomMasterId

    	INSERT into dbo.calculator_run_pom_data_master
    	(calendar_year, created_at, created_by, effective_from, effective_to)
    	values
    	(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

    	SET @pomDataMasterid  = CAST(scope_identity() AS int);

    	INSERT into 
    		dbo.calculator_run_pom_data_detail
    		(calculator_run_pom_data_master_id, 
    			load_ts,
    			organisation_id,
    			packaging_activity,
    			packaging_type,
    			packaging_class,
    			packaging_material,
    			packaging_material_weight,
    			submission_period,
    			submission_period_desc,
    			subsidiary_id)
    	SELECT  @pomDataMasterid,
    			load_ts,
    			organisation_id,
    			packaging_activity,
    			packaging_type,
    			packaging_class,
    			packaging_material,
    			packaging_material_weight,
    			submission_period,
    			submission_period_desc,
    			subsidiary_id
    			from 
    			dbo.pom_data

    	 Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

    END'
    EXEC(@Sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250122092344_PomAndOrganisationProcedures'
)
BEGIN
    declare @Sql varchar(max)
    SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]
        (
            -- Add the parameters for the stored procedure here
            @RunId int,
        	@calendarYear varchar(400),
        	@createdBy varchar(400)
        )
        AS
        BEGIN
            -- SET NOCOUNT ON added to prevent extra result sets from
            -- interfering with SELECT statements.
            SET NOCOUNT ON

        	declare @DateNow datetime, @orgDataMasterid int
        	SET @DateNow = GETDATE()

        	declare @oldCalcRunOrgMasterId int
            SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)

        	Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId

        	INSERT into dbo.calculator_run_organization_data_master
        	(calendar_year, created_at, created_by, effective_from, effective_to)
        	values
        	(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

        	SET @orgDataMasterid  = CAST(scope_identity() AS int);

        	INSERT 
        	into 
        		dbo.calculator_run_organization_data_detail
        		(calculator_run_organization_data_master_id, 
        			load_ts,
        			organisation_id,
        			organisation_name,
        			submission_period_desc,
        			subsidiary_id)
        	SELECT  @orgDataMasterid, 
        			load_ts,
        			organisation_id,
        			organisation_name,
        			submission_period_desc,
        			subsidiary_id  
        			from 
        			dbo.organisation_data

        	Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId

        END'
    EXEC(@Sql) 
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250122092344_PomAndOrganisationProcedures'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250122092344_PomAndOrganisationProcedures', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250123134310_AddCalculatorRunCsvFileMetadata'
)
BEGIN
    CREATE TABLE [calculator_run_csvfile_metadata] (
        [id] int NOT NULL IDENTITY,
        [filename] nvarchar(400) NOT NULL,
        [blob_uri] nvarchar(2000) NOT NULL,
        [calculator_run_id] int NOT NULL,
        CONSTRAINT [PK_calculator_run_csvfile_metadata] PRIMARY KEY ([id]),
        CONSTRAINT [FK_calculator_run_csvfile_metadata_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250123134310_AddCalculatorRunCsvFileMetadata'
)
BEGIN
    CREATE INDEX [IX_calculator_run_csvfile_metadata_calculator_run_id] ON [calculator_run_csvfile_metadata] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250123134310_AddCalculatorRunCsvFileMetadata'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250123134310_AddCalculatorRunCsvFileMetadata', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250205150405_AddSubmissionPeriodLookup'
)
BEGIN
    CREATE TABLE [submission_period_lookup] (
        [submission_period] nvarchar(400) NOT NULL,
        [submission_period_desc] nvarchar(400) NOT NULL,
        [start_date] datetime2 NOT NULL,
        [end_date] datetime2 NOT NULL,
        [days_in_submission_period] int NOT NULL,
        [days_in_whole_period] int NOT NULL,
        [scaleup_factor] decimal(16,12) NOT NULL,
        CONSTRAINT [PK_submission_period_lookup] PRIMARY KEY ([submission_period])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250205150405_AddSubmissionPeriodLookup'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'submission_period', N'submission_period_desc', N'start_date', N'end_date', N'days_in_submission_period', N'days_in_whole_period', N'scaleup_factor') AND [object_id] = OBJECT_ID(N'[submission_period_lookup]'))
        SET IDENTITY_INSERT [submission_period_lookup] ON;
    EXEC(N'INSERT INTO [submission_period_lookup] ([submission_period], [submission_period_desc], [start_date], [end_date], [days_in_submission_period], [days_in_whole_period], [scaleup_factor])
    VALUES (N''2024-P1'', N''January to June 2024'', ''2024-01-01T00:00:00.0000000+00:00'', ''2024-06-30T23:59:59.0000000+01:00'', 182, 182, 1.0),
    (N''2024-P2'', N''April to June 2024'', ''2024-04-01T00:00:00.0000000+01:00'', ''2024-06-30T23:59:59.0000000+01:00'', 91, 182, 2.0),
    (N''2024-P3'', N''May to June 2024'', ''2024-05-01T00:00:00.0000000+01:00'', ''2024-06-30T23:59:59.0000000+01:00'', 61, 182, 2.983606557377),
    (N''2024-P4'', N''July to December 2024'', ''2024-07-01T00:00:00.0000000+01:00'', ''2024-12-31T23:59:59.0000000+00:00'', 184, 184, 1.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'submission_period', N'submission_period_desc', N'start_date', N'end_date', N'days_in_submission_period', N'days_in_whole_period', N'scaleup_factor') AND [object_id] = OBJECT_ID(N'[submission_period_lookup]'))
        SET IDENTITY_INSERT [submission_period_lookup] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250205150405_AddSubmissionPeriodLookup'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250205150405_AddSubmissionPeriodLookup', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[lapcap_data_master]') AND [c].[name] = N'projection_year');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [lapcap_data_master] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [lapcap_data_master] ALTER COLUMN [projection_year] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_setting_master]') AND [c].[name] = N'parameter_year');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_setting_master] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [default_parameter_setting_master] ALTER COLUMN [parameter_year] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'financial_year');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [calculator_run] ALTER COLUMN [financial_year] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    CREATE TABLE [calculator_run_financial_years] (
        [financial_Year] nvarchar(450) NOT NULL,
        [description] nvarchar(max) NULL,
        CONSTRAINT [PK_calculator_run_financial_years] PRIMARY KEY ([financial_Year])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    CREATE INDEX [IX_lapcap_data_master_projection_year] ON [lapcap_data_master] ([projection_year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    CREATE INDEX [IX_default_parameter_setting_master_parameter_year] ON [default_parameter_setting_master] ([parameter_year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    CREATE INDEX [IX_calculator_run_financial_year] ON [calculator_run] ([financial_year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318112342_AddFinancialYearTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250318112342_AddFinancialYearTable', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2023-24'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2024-25'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2025-26'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2026-27'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2027-28'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2028-29'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2029-30'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year], [description])
    VALUES (N''2030-31'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250318130352_FinancialYearMigrations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250318130352_FinancialYearMigrations', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250319095159_AddFinancialYearForeignKeys'
)
BEGIN
    ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_financial_years_financial_year] FOREIGN KEY ([financial_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250319095159_AddFinancialYearForeignKeys'
)
BEGIN
    ALTER TABLE [default_parameter_setting_master] ADD CONSTRAINT [FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year] FOREIGN KEY ([parameter_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250319095159_AddFinancialYearForeignKeys'
)
BEGIN
    ALTER TABLE [lapcap_data_master] ADD CONSTRAINT [FK_lapcap_data_master_calculator_run_financial_years_projection_year] FOREIGN KEY ([projection_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250319095159_AddFinancialYearForeignKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250319095159_AddFinancialYearForeignKeys', N'8.0.7');
END;
GO

COMMIT;
GO

