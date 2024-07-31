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

