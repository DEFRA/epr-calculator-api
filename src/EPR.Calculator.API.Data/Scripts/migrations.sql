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
    (N''LRET-AL'', N''Aluminium'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-FC'', N''Fibre composite'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-GL'', N''Glass'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-PC'', N''Paper or card'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-PL'', N''Plastic'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-ST'', N''Steel'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-WD'', N''Wood'', N''Late reporting tonnage'', 0.0, 999999999.99),
    (N''LRET-OT'', N''Other'', N''Late reporting tonnage'', 0.0, 999999999.99),
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

