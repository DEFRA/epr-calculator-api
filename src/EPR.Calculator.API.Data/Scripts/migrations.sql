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

                    IF NOT EXISTS (
                        SELECT 1 FROM [default_parameter_template_master]
                        WHERE [parameter_unique_ref] = 'TONT-AD'
                    )
                    BEGIN
                        IF EXISTS (
                            SELECT * FROM [sys].[identity_columns]
                            WHERE [object_id] = OBJECT_ID('default_parameter_template_master')
                        )
                            SET IDENTITY_INSERT [default_parameter_template_master] ON;

                        INSERT INTO [default_parameter_template_master]
                        ([parameter_unique_ref], [parameter_type], [parameter_category], [valid_Range_from], [valid_Range_to])
                        VALUES ('TONT-AD', 'Amount Decrease', 'Tonnage change threshold', 0.00, 999999999.99);

                        IF EXISTS (
                            SELECT * FROM [sys].[identity_columns]
                            WHERE [object_id] = OBJECT_ID('default_parameter_template_master')
                        )
                            SET IDENTITY_INSERT [default_parameter_template_master] OFF;
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN

                    UPDATE [default_parameter_setting_detail]
                    SET [parameter_unique_ref] = 'TONT-AD'
                    WHERE [parameter_unique_ref] = 'TONT-DI';
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240731130652_202407311405_UpdateTemplateMaster'
)
BEGIN

                    DELETE FROM [default_parameter_template_master]
                    WHERE [parameter_unique_ref] = 'TONT-DI';
                
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

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250424155648_AddingHasBillingFileGeneratedColumn'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [HasBillingFileGenerated] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250424155648_AddingHasBillingFileGeneratedColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250424155648_AddingHasBillingFileGeneratedColumn', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427142501_AddNewRunClassification'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] ON;
    EXEC(N'INSERT INTO [calculator_run_classification] ([status], [created_by])
    VALUES (N''INITIAL RUN COMPLETED'', N''System User'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427142501_AddNewRunClassification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250427142501_AddNewRunClassification', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250428101935_AddNewRunClassifications'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] ON;
    EXEC(N'INSERT INTO [calculator_run_classification] ([status], [created_by])
    VALUES (N''INITIAL RUN'', N''Test user''),
    (N''INTERIM RE-CALCULATION RUN'', N''Test user''),
    (N''FINAL RUN'', N''Test user''),
    (N''FINAL RE-CALCULATION RUN'', N''Test user'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'status', N'created_by') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250428101935_AddNewRunClassifications'
)
BEGIN
    EXEC(N'UPDATE [calculator_run_classification] SET [status] = N''TEST RUN''
    WHERE [id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250428101935_AddNewRunClassifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250428101935_AddNewRunClassifications', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250507155935_AddCalculatorRunBillingFileMetadataTable'
)
BEGIN
    CREATE TABLE [calculator_run_billing_file_metadata] (
        [id] int NOT NULL IDENTITY,
        [billing_csv_filename] nvarchar(400) NULL,
        [billing_json_filename] nvarchar(400) NULL,
        [billing_file_created_date] datetime2 NOT NULL,
        [billing_file_created_by] nvarchar(400) NOT NULL,
        [billing_file_authorised_date] datetime2 NULL,
        [billing_file_authorised_by] nvarchar(400) NULL,
        [calculator_run_id] int NOT NULL,
        CONSTRAINT [PK_calculator_run_billing_file_metadata] PRIMARY KEY ([id]),
        CONSTRAINT [FK_calculator_run_billing_file_metadata_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250507155935_AddCalculatorRunBillingFileMetadataTable'
)
BEGIN
    CREATE INDEX [IX_calculator_run_billing_file_metadata_calculator_run_id] ON [calculator_run_billing_file_metadata] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250507155935_AddCalculatorRunBillingFileMetadataTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250507155935_AddCalculatorRunBillingFileMetadataTable', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250509113737_AddTradingName'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [trading_name] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250509113737_AddTradingName'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [trading_name] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250509113737_AddTradingName'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250509113737_AddTradingName', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250516101138_AddTradingNameToProducerDetail'
)
BEGIN
    ALTER TABLE [producer_detail] ADD [trading_name] nvarchar(4000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250516101138_AddTradingNameToProducerDetail'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250516101138_AddTradingNameToProducerDetail', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250516101257_UpdateOrganisationSproc'
)
BEGIN

                    IF OBJECT_ID(N'[dbo].[CreateRunOrganization]', N'P') IS NOT NULL
                        DROP PROCEDURE [dbo].[CreateRunOrganization]
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250516101257_UpdateOrganisationSproc'
)
BEGIN

                    DECLARE @Sql NVARCHAR(MAX)
                    SET @Sql = N'
                    CREATE PROCEDURE [dbo].[CreateRunOrganization]
                    (
                        @RunId int,
                        @calendarYear varchar(400),
                        @createdBy varchar(400)
                    )
                    AS
                    BEGIN
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
                                trading_name,
                                submission_period_desc,
                                subsidiary_id)
                        SELECT  @orgDataMasterid, 
                                load_ts,
                                organisation_id,
                                organisation_name,
                                trading_name,
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
    WHERE [MigrationId] = N'20250516101257_UpdateOrganisationSproc'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250516101257_UpdateOrganisationSproc', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145003_AddProducerInvoicedMaterialNetTonnage'
)
BEGIN
    CREATE TABLE [producer_invoiced_material_net_tonnage] (
        [id] int NOT NULL IDENTITY,
        [calculator_run_id] int NOT NULL,
        [material_id] int NOT NULL,
        [producer_id] int NOT NULL,
        [invoiced_net_tonnage] decimal(18,2) NULL,
        CONSTRAINT [PK_producer_invoiced_material_net_tonnage] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145003_AddProducerInvoicedMaterialNetTonnage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250521145003_AddProducerInvoicedMaterialNetTonnage', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145147_AddProducerDesignatedRunInvoiceInstruction'
)
BEGIN
    CREATE TABLE [producer_designated_run_invoice_instruction] (
        [id] int NOT NULL IDENTITY,
        [producer_id] int NOT NULL,
        [calculator_run_id] int NOT NULL,
        [current_year_invoiced_total_after_this_run] decimal(18,2) NULL,
        [invoice_amount] decimal(18,2) NULL,
        [outstanding_balance] decimal(18,2) NULL,
        [billing_instruction_id] nvarchar(4000) NULL,
        [instruction_confirmed_date] datetime2 NULL,
        [instruction_confirmed_by] nvarchar(4000) NULL,
        CONSTRAINT [PK_producer_designated_run_invoice_instruction] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145147_AddProducerDesignatedRunInvoiceInstruction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250521145147_AddProducerDesignatedRunInvoiceInstruction', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145341_AddProducerResultFileSuggestedBillingInstruction'
)
BEGIN
    CREATE TABLE [producer_resultfile_suggested_billing_instruction] (
        [id] int NOT NULL IDENTITY,
        [calculator_run_id] int NOT NULL,
        [producer_id] int NOT NULL,
        [total_producer_bill_with_bad_debt] decimal(18,2) NOT NULL,
        [current_year_invoice_total_to_date] decimal(18,2) NULL,
        [tonnage_change_since_last_invoice] nvarchar(4000) NULL,
        [amount_liability_difference_calc_vs_prev] decimal(18,2) NULL,
        [material_pound_threshold_breached] nvarchar(4000) NULL,
        [tonnage_pound_threshold_breached] nvarchar(4000) NULL,
        [percentage_liability_difference_calc_vs_prev] decimal(18,2) NULL,
        [material_percentage_threshold_breached] nvarchar(4000) NULL,
        [tonnage_percentage_threshold_breached] nvarchar(4000) NULL,
        [suggested_billing_instruction] nvarchar(4000) NOT NULL,
        [suggested_invoice_amount] decimal(18,2) NOT NULL,
        [billing_instruction_accept_reject] nvarchar(4000) NULL,
        [reason_for_rejection] nvarchar(4000) NULL,
        [last_modified_accept_reject_by] nvarchar(4000) NULL,
        [last_modified_accept_reject] datetime2 NULL,
        CONSTRAINT [PK_producer_resultfile_suggested_billing_instruction] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250521145341_AddProducerResultFileSuggestedBillingInstruction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250521145341_AddProducerResultFileSuggestedBillingInstruction', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    CREATE INDEX [IX_producer_resultfile_suggested_billing_instruction_calculator_run_id] ON [producer_resultfile_suggested_billing_instruction] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    CREATE INDEX [IX_producer_invoiced_material_net_tonnage_calculator_run_id] ON [producer_invoiced_material_net_tonnage] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    CREATE INDEX [IX_producer_invoiced_material_net_tonnage_material_id] ON [producer_invoiced_material_net_tonnage] ([material_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    CREATE INDEX [IX_producer_designated_run_invoice_instruction_calculator_run_id] ON [producer_designated_run_invoice_instruction] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    ALTER TABLE [producer_designated_run_invoice_instruction] ADD CONSTRAINT [FK_producer_designated_run_invoice_instruction_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    ALTER TABLE [producer_invoiced_material_net_tonnage] ADD CONSTRAINT [FK_producer_invoiced_material_net_tonnage_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    ALTER TABLE [producer_invoiced_material_net_tonnage] ADD CONSTRAINT [FK_producer_invoiced_material_net_tonnage_material_material_id] FOREIGN KEY ([material_id]) REFERENCES [material] ([id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    ALTER TABLE [producer_resultfile_suggested_billing_instruction] ADD CONSTRAINT [FK_producer_resultfile_suggested_billing_instruction_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093001_AddProducerDesignatedRunHistoryRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250522093001_AddProducerDesignatedRunHistoryRelationships', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620144219_AddNewColumnIsBillingFileGenerating'
)
BEGIN
    ALTER TABLE [calculator_run] ADD [is_billing_file_generating] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620144219_AddNewColumnIsBillingFileGenerating'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620144219_AddNewColumnIsBillingFileGenerating', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250709123908_RemoveColumnHasBillingFileGenerated'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'HasBillingFileGenerated');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [calculator_run] DROP COLUMN [HasBillingFileGenerated];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250709123908_RemoveColumnHasBillingFileGenerated'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250709123908_RemoveColumnHasBillingFileGenerated', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    EXEC(N'DELETE FROM [calculator_run_financial_years]
    WHERE [financial_Year] = N''2026-27'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    EXEC(N'DELETE FROM [calculator_run_financial_years]
    WHERE [financial_Year] = N''2027-28'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    EXEC(N'DELETE FROM [calculator_run_financial_years]
    WHERE [financial_Year] = N''2028-29'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    EXEC(N'DELETE FROM [calculator_run_financial_years]
    WHERE [financial_Year] = N''2029-30'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    EXEC(N'DELETE FROM [calculator_run_financial_years]
    WHERE [financial_Year] = N''2030-31'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250717154431_DeleteFinancialYearsData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250717154431_DeleteFinancialYearsData', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721090348_AddNewPostInitialStatuses'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] ON;
    EXEC(N'INSERT INTO [calculator_run_classification] ([id], [created_by], [status])
    VALUES (12, N''System User'', N''INTERIM RE-CALCULATION RUN COMPLETED''),
    (13, N''System User'', N''FINAL RE-CALCULATION RUN COMPLETED''),
    (14, N''System User'', N''FINAL RUN COMPLETED'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
        SET IDENTITY_INSERT [calculator_run_classification] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721090348_AddNewPostInitialStatuses'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721090348_AddNewPostInitialStatuses', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721154526_AddFunctionGetInvoiceAmount'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL
        DROP FUNCTION [dbo].[GetInvoiceAmount]
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721154526_AddFunctionGetInvoiceAmount'
)
BEGIN
    declare @sql nvarchar(max)
    SET @sql = N'
    CREATE FUNCTION [dbo].[GetInvoiceAmount] ( 
        @billingInstructionAcceptReject VARCHAR(250),
        @suggestedBillingInstruction    VARCHAR(250),
        @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
        @LiabilityDifference           DECIMAL(18,2)
    )
    RETURNS DECIMAL(18,2)
    AS
    BEGIN
        IF @billingInstructionAcceptReject <> ''Accepted''
            RETURN NULL;

        RETURN 
            CASE 
                WHEN @suggestedBillingInstruction IN (''INITIAL'', ''REBILL'') THEN @totalProducerBillWithBadDebtProvision
                WHEN @suggestedBillingInstruction = ''DELTA'' THEN @LiabilityDifference
                ELSE NULL
            END;
    END'

    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721154526_AddFunctionGetInvoiceAmount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721154526_AddFunctionGetInvoiceAmount', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161113_GetCurrentYearInvoicedTotalAfterThisRun'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL  
    DROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161113_GetCurrentYearInvoicedTotalAfterThisRun'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] ( 
        @billingInstructionAcceptReject      VARCHAR(250),
        @suggestedBillingInstruction         VARCHAR(250),
        @currentYearInvoicedTotalToDate      DECIMAL(18,2),
        @invoiceAmount                       DECIMAL(18,2)
    )
    RETURNS DECIMAL(18,2)
    AS
    BEGIN
        -- Rule 1: Cancelled instruction always returns NULL
        IF @suggestedBillingInstruction = ''CANCEL''
            RETURN NULL;

        -- Rule 2: Rejected INITIAL returns NULL
        IF @billingInstructionAcceptReject = ''Rejected'' AND @suggestedBillingInstruction = ''INITIAL''
            RETURN NULL;

        -- Rule 3: Rejected (but not INITIAL) returns current total as-is
        IF @billingInstructionAcceptReject = ''Rejected''
            RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

        -- Rule 4: Accepted or any other case adds invoice amount
        RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);
    END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161113_GetCurrentYearInvoicedTotalAfterThisRun'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721161113_GetCurrentYearInvoicedTotalAfterThisRun', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161345_GetOutstandingBalance'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL  
    DROP FUNCTION [dbo].GetOutstandingBalance
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161345_GetOutstandingBalance'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE FUNCTION [dbo].[GetOutstandingBalance] (
        @billingInstructionAcceptReject        VARCHAR(250),
        @suggestedBillingInstruction           VARCHAR(250),
        @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
        @LiabilityDifference                   DECIMAL(18,2)
    )
    RETURNS DECIMAL(18,2)
    AS
    BEGIN
        RETURN 
            CASE 
                WHEN @billingInstructionAcceptReject <> ''Accepted'' AND @suggestedBillingInstruction = ''INITIAL''
                    THEN @totalProducerBillWithBadDebtProvision

                WHEN @billingInstructionAcceptReject <> ''Accepted''
                    THEN @LiabilityDifference

                ELSE NULL
            END;
    END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161345_GetOutstandingBalance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721161345_GetOutstandingBalance', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161415_GetInvoiceDetailsAtProducerLevel'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  
    DROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161415_GetInvoiceDetailsAtProducerLevel'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]
    (
        @instructionConfirmedBy NVARCHAR(4000),
        @instructionConfirmedDate DATETIME2(7),
        @calculatorRunID INT
    )
    AS
    BEGIN
        SET NOCOUNT OFF
        -- Temp table to hold calculated values
        CREATE TABLE #CalculatedValues (
            calculator_run_id INT,
            producer_id INT,
            total_producer_bill_with_bad_debt DECIMAL(18,2),
            current_year_invoice_total_to_date DECIMAL(18,2),
            amount_liability_difference_calc_vs_prev DECIMAL(18,2),
            suggested_billing_instruction NVARCHAR(4000),
            billing_instruction_accept_reject NVARCHAR(4000),
            invoice_amount DECIMAL(18,2),
            instruction_confirmed_by NVARCHAR(4000),
            instruction_confirmed_date DATETIME2(7),
            billing_instruction_id NVARCHAR(4000)
        );

        -- Insert into temp table
        INSERT INTO #CalculatedValues
        SELECT 
            prsbi.calculator_run_id,
            prsbi.producer_id,
            prsbi.total_producer_bill_with_bad_debt, 
            prsbi.current_year_invoice_total_to_date,
            prsbi.amount_liability_difference_calc_vs_prev,
            prsbi.suggested_billing_instruction,
            prsbi.billing_instruction_accept_reject,
            dbo.GetInvoiceAmount(
                prsbi.billing_instruction_accept_reject,
                prsbi.suggested_billing_instruction,
                prsbi.total_producer_bill_with_bad_debt,
                prsbi.amount_liability_difference_calc_vs_prev
            ) AS invoice_amount,
            @instructionConfirmedBy AS instruction_confirmed_by,
            @instructionConfirmedDate AS instruction_confirmed_date,
            CONCAT(prsbi.calculator_run_id, ''-'', prsbi.producer_id) AS billing_instruction_id
        FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi
        WHERE prsbi.calculator_run_id = @calculatorRunID AND LOWER(prsbi.billing_instruction_accept_reject) = ''accepted'';

        -- First SELECT using the temp table
    	INSERT INTO [dbo].[producer_designated_run_invoice_instruction] (
    		producer_id,
    		calculator_run_id,
    		current_year_invoiced_total_after_this_run,
    		invoice_amount,
    		outstanding_balance,
    		billing_instruction_id,
    		instruction_confirmed_date,
    		instruction_confirmed_by
    		)
    			SELECT 
    				cv.producer_id,
    				cv.calculator_run_id,
    				dbo.GetCurrentYearInvoicedTotalAfterThisRun(
    					cv.billing_instruction_accept_reject,
    					cv.suggested_billing_instruction,
    					cv.current_year_invoice_total_to_date,
    					cv.invoice_amount
    				) AS current_year_invoiced_total_after_this_run,
    				cv.invoice_amount,
    				dbo.GetOutstandingBalance(
    					cv.billing_instruction_accept_reject,
    					cv.suggested_billing_instruction,
    					cv.total_producer_bill_with_bad_debt,
    					cv.amount_liability_difference_calc_vs_prev
    				) AS outstanding_balance,
    				cv.billing_instruction_id,
    				cv.instruction_confirmed_date,
    				cv.instruction_confirmed_by			
    			FROM #CalculatedValues AS cv;

        DROP TABLE #CalculatedValues;
    END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721161415_GetInvoiceDetailsAtProducerLevel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721161415_GetInvoiceDetailsAtProducerLevel', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723131102_MarkInitialRunsAsDeleted'
)
BEGIN
    update dbo.calculator_run SET calculator_run_classification_id  = 6 where calculator_run_classification_id in (7,8)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723131102_MarkInitialRunsAsDeleted'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250723131102_MarkInitialRunsAsDeleted', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723132540_UpdateInvoiceNetTonnageColumnPrecision'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_invoiced_material_net_tonnage]') AND [c].[name] = N'invoiced_net_tonnage');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [producer_invoiced_material_net_tonnage] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [producer_invoiced_material_net_tonnage] ALTER COLUMN [invoiced_net_tonnage] decimal(18,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723132540_UpdateInvoiceNetTonnageColumnPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250723132540_UpdateInvoiceNetTonnageColumnPrecision', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year])
    VALUES (N''2026-27'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year])
    VALUES (N''2027-28'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year])
    VALUES (N''2028-29'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year])
    VALUES (N''2029-30'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] ON;
    EXEC(N'INSERT INTO [calculator_run_financial_years] ([financial_Year])
    VALUES (N''2030-31'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'financial_Year') AND [object_id] = OBJECT_ID(N'[calculator_run_financial_years]'))
        SET IDENTITY_INSERT [calculator_run_financial_years] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250723143822_AddFinancialYears'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250723143822_AddFinancialYears', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724153138_UpdateNetTonnagePrecision'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_invoiced_material_net_tonnage]') AND [c].[name] = N'invoiced_net_tonnage');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [producer_invoiced_material_net_tonnage] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [producer_invoiced_material_net_tonnage] ALTER COLUMN [invoiced_net_tonnage] decimal(18,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250724153138_UpdateNetTonnagePrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250724153138_UpdateNetTonnagePrecision', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250725133129_AlterInsertInvoiceDetails'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  
    DROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250725133129_AlterInsertInvoiceDetails'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]
        (
            @instructionConfirmedBy NVARCHAR(4000),
            @instructionConfirmedDate DATETIME2(7),
            @calculatorRunID INT
        )
        AS
        BEGIN
            SET NOCOUNT OFF
            -- Temp table to hold calculated values
            CREATE TABLE #CalculatedValues (
                calculator_run_id INT,
                producer_id INT,
                total_producer_bill_with_bad_debt DECIMAL(18,2),
                current_year_invoice_total_to_date DECIMAL(18,2),
                amount_liability_difference_calc_vs_prev DECIMAL(18,2),
                suggested_billing_instruction NVARCHAR(4000),
                billing_instruction_accept_reject NVARCHAR(4000),
                invoice_amount DECIMAL(18,2),
                instruction_confirmed_by NVARCHAR(4000),
                instruction_confirmed_date DATETIME2(7),
                billing_instruction_id NVARCHAR(4000)
            );

            -- Insert into temp table
            INSERT INTO #CalculatedValues
            SELECT 
                prsbi.calculator_run_id,
                prsbi.producer_id,
                prsbi.total_producer_bill_with_bad_debt, 
                prsbi.current_year_invoice_total_to_date,
                prsbi.amount_liability_difference_calc_vs_prev,
                prsbi.suggested_billing_instruction,
                prsbi.billing_instruction_accept_reject,
                dbo.GetInvoiceAmount(
                    prsbi.billing_instruction_accept_reject,
                    prsbi.suggested_billing_instruction,
                    prsbi.total_producer_bill_with_bad_debt,
                    prsbi.amount_liability_difference_calc_vs_prev
                ) AS invoice_amount,
                @instructionConfirmedBy AS instruction_confirmed_by,
                @instructionConfirmedDate AS instruction_confirmed_date,
                CONCAT(prsbi.calculator_run_id, ''-'', prsbi.producer_id) AS billing_instruction_id
            FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi
            WHERE prsbi.calculator_run_id = @calculatorRunID

            -- First SELECT using the temp table
        	INSERT INTO [dbo].[producer_designated_run_invoice_instruction] (
        		producer_id,
        		calculator_run_id,
        		current_year_invoiced_total_after_this_run,
        		invoice_amount,
        		outstanding_balance,
        		billing_instruction_id,
        		instruction_confirmed_date,
        		instruction_confirmed_by
        		)
        			SELECT 
        				cv.producer_id,
        				cv.calculator_run_id,
        				dbo.GetCurrentYearInvoicedTotalAfterThisRun(
        					cv.billing_instruction_accept_reject,
        					cv.suggested_billing_instruction,
        					cv.current_year_invoice_total_to_date,
        					cv.invoice_amount
        				) AS current_year_invoiced_total_after_this_run,
        				cv.invoice_amount,
        				dbo.GetOutstandingBalance(
        					cv.billing_instruction_accept_reject,
        					cv.suggested_billing_instruction,
        					cv.total_producer_bill_with_bad_debt,
        					cv.amount_liability_difference_calc_vs_prev
        				) AS outstanding_balance,
        				cv.billing_instruction_id,
        				cv.instruction_confirmed_date,
        				cv.instruction_confirmed_by			
        			FROM #CalculatedValues AS cv;

            DROP TABLE #CalculatedValues;
        END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250725133129_AlterInsertInvoiceDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250725133129_AlterInsertInvoiceDetails', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728083102_UpdateBillingInstructionId'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  
    DROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728083102_UpdateBillingInstructionId'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]
        (
            @instructionConfirmedBy NVARCHAR(4000),
            @instructionConfirmedDate DATETIME2(7),
            @calculatorRunID INT
        )
        AS
        BEGIN
            SET NOCOUNT OFF
            -- Temp table to hold calculated values
            CREATE TABLE #CalculatedValues (
                calculator_run_id INT,
                producer_id INT,
                total_producer_bill_with_bad_debt DECIMAL(18,2),
                current_year_invoice_total_to_date DECIMAL(18,2),
                amount_liability_difference_calc_vs_prev DECIMAL(18,2),
                suggested_billing_instruction NVARCHAR(4000),
                billing_instruction_accept_reject NVARCHAR(4000),
                invoice_amount DECIMAL(18,2),
                instruction_confirmed_by NVARCHAR(4000),
                instruction_confirmed_date DATETIME2(7),
                billing_instruction_id NVARCHAR(4000)
            );

            -- Insert into temp table
            INSERT INTO #CalculatedValues
            SELECT 
                prsbi.calculator_run_id,
                prsbi.producer_id,
                prsbi.total_producer_bill_with_bad_debt, 
                prsbi.current_year_invoice_total_to_date,
                prsbi.amount_liability_difference_calc_vs_prev,
                prsbi.suggested_billing_instruction,
                prsbi.billing_instruction_accept_reject,
                dbo.GetInvoiceAmount(
                    prsbi.billing_instruction_accept_reject,
                    prsbi.suggested_billing_instruction,
                    prsbi.total_producer_bill_with_bad_debt,
                    prsbi.amount_liability_difference_calc_vs_prev
                ) AS invoice_amount,
                @instructionConfirmedBy AS instruction_confirmed_by,
                @instructionConfirmedDate AS instruction_confirmed_date,
                CONCAT(prsbi.calculator_run_id, ''_'', prsbi.producer_id) AS billing_instruction_id
            FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi
            WHERE prsbi.calculator_run_id = @calculatorRunID

            -- First SELECT using the temp table
        	INSERT INTO [dbo].[producer_designated_run_invoice_instruction] (
        		producer_id,
        		calculator_run_id,
        		current_year_invoiced_total_after_this_run,
        		invoice_amount,
        		outstanding_balance,
        		billing_instruction_id,
        		instruction_confirmed_date,
        		instruction_confirmed_by
        		)
        			SELECT 
        				cv.producer_id,
        				cv.calculator_run_id,
        				dbo.GetCurrentYearInvoicedTotalAfterThisRun(
        					cv.billing_instruction_accept_reject,
        					cv.suggested_billing_instruction,
        					cv.current_year_invoice_total_to_date,
        					cv.invoice_amount
        				) AS current_year_invoiced_total_after_this_run,
        				cv.invoice_amount,
        				dbo.GetOutstandingBalance(
        					cv.billing_instruction_accept_reject,
        					cv.suggested_billing_instruction,
        					cv.total_producer_bill_with_bad_debt,
        					cv.amount_liability_difference_calc_vs_prev
        				) AS outstanding_balance,
        				cv.billing_instruction_id,
        				cv.instruction_confirmed_date,
        				cv.instruction_confirmed_by			
        			FROM #CalculatedValues AS cv;

            DROP TABLE #CalculatedValues;
        END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250728083102_UpdateBillingInstructionId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250728083102_UpdateBillingInstructionId', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814125052_AlterCreateRunPomProcedure'
)
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[CreateRunPom]'))
    DROP PROCEDURE [dbo].[CreateRunPom];
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
    			CASE			
    			WHEN LTRIM(RTRIM(subsidiary_id)) = ''''
    			THEN NULL
    			ELSE subsidiary_id
    			END			
    			as subsidiary_id
    			from 
    			dbo.pom_data

    	 Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

    END'
    EXEC(@Sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814125052_AlterCreateRunPomProcedure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814125052_AlterCreateRunPomProcedure', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814130248_AlterCreateRunOrganisationProcedure'
)
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[CreateRunOrganization]'))
     DROP PROCEDURE[dbo].[CreateRunOrganization];
    declare @Sql varchar(max)
    SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]
                    (
                        @RunId int,
                        @calendarYear varchar(400),
                        @createdBy varchar(400)
                    )
                    AS
                    BEGIN
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
                                trading_name,
                                submission_period_desc,
                                subsidiary_id)
                        SELECT  @orgDataMasterid, 
                                load_ts,
                                organisation_id,
                                organisation_name,
                                trading_name,
                                submission_period_desc,
                                CASE			
    							WHEN LTRIM(RTRIM(subsidiary_id)) = ''''
    							THEN NULL
    							ELSE subsidiary_id
    							END			
    							as subsidiary_id 
                                from 
                                dbo.organisation_data

                        Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId

                    END'
    EXEC(@Sql) 
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814130248_AlterCreateRunOrganisationProcedure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814130248_AlterCreateRunOrganisationProcedure', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815134048_AddProducerIdIndex'
)
BEGIN
    CREATE INDEX [IX_producer_resultfile_suggested_billing_instruction_producer_id] ON [producer_resultfile_suggested_billing_instruction] ([producer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815134048_AddProducerIdIndex'
)
BEGIN
    CREATE INDEX [IX_producer_detail_producer_id] ON [producer_detail] ([producer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815134048_AddProducerIdIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250815134048_AddProducerIdIndex', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909091510_UpdateProducerSuggestedBilling'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_resultfile_suggested_billing_instruction]') AND [c].[name] = N'total_producer_bill_with_bad_debt');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [producer_resultfile_suggested_billing_instruction] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [producer_resultfile_suggested_billing_instruction] ALTER COLUMN [total_producer_bill_with_bad_debt] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909091510_UpdateProducerSuggestedBilling'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_resultfile_suggested_billing_instruction]') AND [c].[name] = N'suggested_invoice_amount');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [producer_resultfile_suggested_billing_instruction] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [producer_resultfile_suggested_billing_instruction] ALTER COLUMN [suggested_invoice_amount] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909091510_UpdateProducerSuggestedBilling'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909091510_UpdateProducerSuggestedBilling', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250911160340_CorrectMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250911160340_CorrectMigration', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251003125307_AlterGetCurrentYearInvoicedTotalAfterThisRunFunction'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL  
    DROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251003125307_AlterGetCurrentYearInvoicedTotalAfterThisRunFunction'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) 
    SET @sql = N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] ( 
        @billingInstructionAcceptReject      VARCHAR(250),
        @suggestedBillingInstruction         VARCHAR(250),
        @currentYearInvoicedTotalToDate      DECIMAL(18,2),
        @invoiceAmount                       DECIMAL(18,2)
    )
    RETURNS DECIMAL(18,2)
    AS
    BEGIN
        -- Rule 1: Cancelled and Rejected instruction always returns last invoiced values
        IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Rejected''
             RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

        -- Rule 2: Cancelled instruction always returns NULL
        IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Accepted''
            RETURN NULL;

        -- Rule 3: Rejected INITIAL returns NULL
        IF @billingInstructionAcceptReject = ''Rejected'' AND @suggestedBillingInstruction = ''INITIAL''
            RETURN NULL;

        -- Rule 4: Rejected (but not INITIAL) returns current total as-is
        IF @billingInstructionAcceptReject = ''Rejected''
            RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

        -- Rule 5: Accepted or any other case adds invoice amount
        RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);
    END'
    EXEC(@sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251003125307_AlterGetCurrentYearInvoicedTotalAfterThisRunFunction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251003125307_AlterGetCurrentYearInvoicedTotalAfterThisRunFunction', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    CREATE TABLE [error_type] (
        [id] int NOT NULL IDENTITY,
        [name] nvarchar(250) NOT NULL,
        [description] nvarchar(max) NULL,
        CONSTRAINT [PK_error_type] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    CREATE TABLE [error_report] (
        [id] int NOT NULL IDENTITY,
        [producer_id] int NOT NULL,
        [subsidiary_id] nvarchar(400) NULL,
        [calculator_run_id] int NOT NULL,
        [leaver_code] nvarchar(max) NULL,
        [error_type_id] int NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] nvarchar(400) NOT NULL,
        CONSTRAINT [PK_error_report] PRIMARY KEY ([id]),
        CONSTRAINT [FK_error_report_calculator_run_calculator_run_id] FOREIGN KEY ([calculator_run_id]) REFERENCES [calculator_run] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_error_report_error_type_error_type_id] FOREIGN KEY ([error_type_id]) REFERENCES [error_type] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    CREATE INDEX [IX_error_report_calculator_run_id] ON [error_report] ([calculator_run_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    CREATE INDEX [IX_error_report_error_type_id] ON [error_report] ([error_type_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_error_type_name] ON [error_type] ([name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023100303_AddErrorTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251023100303_AddErrorTables', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    DROP INDEX [IX_producer_designated_run_invoice_instruction_calculator_run_id] ON [producer_designated_run_invoice_instruction];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    DROP INDEX [IX_calculator_run_calculator_run_classification_id] ON [calculator_run];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_index_producer_invoiced_material_net_tonnage] ON [producer_invoiced_material_net_tonnage] ([producer_id], [calculator_run_id], [id]) INCLUDE ([material_id], [invoiced_net_tonnage]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_index_producer_designated_run_invoice] ON [producer_designated_run_invoice_instruction] ([calculator_run_id], [producer_id], [id]) INCLUDE ([current_year_invoiced_total_after_this_run], [invoice_amount], [outstanding_balance], [billing_instruction_id], [instruction_confirmed_date], [instruction_confirmed_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_index_calculator_run] ON [calculator_run] ([calculator_run_classification_id], [financial_year], [is_billing_file_generating], [id]) INCLUDE ([name], [created_by], [created_at], [updated_by], [updated_at], [calculator_run_organization_data_master_id], [calculator_run_pom_data_master_id], [default_parameter_setting_master_id], [lapcap_data_master_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023123530_AddNonClusteredIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251023123530_AddNonClusteredIndexes', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024094126_AddErrorTypesReferenceData'
)
BEGIN
    DROP INDEX [IX_error_type_name] ON [error_type];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024094126_AddErrorTypesReferenceData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'description', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] ON;
    EXEC(N'INSERT INTO [error_type] ([id], [description], [name])
    VALUES (1, N''Where there is a misalignment between the POM files structure and the Registration file structure re Producer IDs and Subsidiary IDs'', N''Missing Registration Data''),
    (2, N''Where there is more than one entry for a producer that has an obligated leaver code.'', N''Conflicting Obligations (Leaver Codes)''),
    (3, N''Where there is more than one entry for a producer that has a blank leaver code and where there are no obligated leaver code entries'', N''Conflicting Obligations (Blank)''),
    (4, N''Where a producer is flagged with a leaver code of 11 (Insolvent) or 12 (No longer performing a producer function)'', N''No longer trading''),
    (5, N''Where a producer only appears with Not Obligated leaver codes.'', N''Not Obligated''),
    (6, N''Where the producer is only flagged as a leaver of a compliance scheme (Leaver Code 13 and 14)'', N''Compliance Scheme Leaver''),
    (7, N''Where a producer leaves a compliance scheme and is obligated as a direct producer.'', N''Compliance Scheme to Direct Producer''),
    (8, N''Where a producer has an entry for a non-valid leaver code.'', N''Invalid Leaver Code''),
    (9, N''Catch all for other errors'', N''Unknown error'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'description', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024094126_AddErrorTypesReferenceData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251024094126_AddErrorTypesReferenceData', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029132348_UpdateErrorTypesSeedDataReplaceUnknownType'
)
BEGIN
    EXEC(N'UPDATE [error_type] SET [description] = N''Where a leaver or joiner date falls outside of the calendar year boundary.'', [name] = N''Date input issue''
    WHERE [id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029132348_UpdateErrorTypesSeedDataReplaceUnknownType'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'description', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] ON;
    EXEC(N'INSERT INTO [error_type] ([id], [description], [name])
    VALUES (10, N''Where a Organisation (Producer or Subsidiary) ID does not conform to the 6 digit structure.'', N''Invalid Organisation ID'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'description', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251029132348_UpdateErrorTypesSeedDataReplaceUnknownType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251029132348_UpdateErrorTypesSeedDataReplaceUnknownType', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251031131142_RemoveErrorTypeDescriptionColumnAndSeedData'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[error_type]') AND [c].[name] = N'description');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [error_type] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [error_type] DROP COLUMN [description];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251031131142_RemoveErrorTypeDescriptionColumnAndSeedData'
)
BEGIN
    CREATE UNIQUE INDEX [IX_error_type_name] ON [error_type] ([name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251031131142_RemoveErrorTypeDescriptionColumnAndSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251031131142_RemoveErrorTypeDescriptionColumnAndSeedData', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [pom_data] ADD [submitter_id] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [obligation_status] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [submitter_id] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [calculator_run_pom_data_detail] ADD [submitter_id] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [obligation_status] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [submitter_id] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117130042_ModifyCreateRunOrgAndCreateRunPOMSprocs'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
    				DROP PROCEDURE [dbo].[CreateRunOrganization];
                    declare @Sql varchar(max);
    				SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]                
    				(                    @RunId int,                    @calendarYear varchar(400),                    @createdBy varchar(400)                )                
    				AS                
    				BEGIN                    
    				SET NOCOUNT ON                    
    					declare @DateNow datetime, @orgDataMasterid int                    
    						SET @DateNow = GETDATE()                    
    					declare @oldCalcRunOrgMasterId int                    
    						SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)                    
    					Update calculator_run_organization_data_master SET effective_to = @DateNow 
    						WHERE id = @oldCalcRunOrgMasterId                    
    					INSERT into dbo.calculator_run_organization_data_master                    
    						(calendar_year, created_at, created_by, effective_from, effective_to)                    
    					values                    
    						(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)                    
    					SET @orgDataMasterid  = CAST(scope_identity() AS int);                    
    					INSERT  into dbo.calculator_run_organization_data_detail                        
    						(calculator_run_organization_data_master_id,
    						load_ts,organisation_id,
    						organisation_name,
    						trading_name,                            
    						submission_period_desc,                            
    						subsidiary_id,
    						obligation_status,
    						submitter_id)                    
    					SELECT  @orgDataMasterid,                             
    					load_ts,                            
    					organisation_id,                            
    					organisation_name,                            
    					trading_name,                            
    					submission_period_desc,                            
    					CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
    					obligation_status,
    					submitter_id
    					from                             
    						dbo.organisation_data                    
    					Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
    					END'
    				EXEC(@Sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117130042_ModifyCreateRunOrgAndCreateRunPOMSprocs'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
    			DROP PROCEDURE [dbo].[CreateRunPom];
                    declare @Sql varchar(max);
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
    					subsidiary_id,
    					submitter_id)
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
    					CASE			
    					WHEN LTRIM(RTRIM(subsidiary_id)) = ''''
    					THEN NULL
    					ELSE subsidiary_id
    					END			
    					as subsidiary_id,
    					submitter_id
    					from 
    					dbo.pom_data

    			 Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

    		END'
    		EXEC(@Sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117130042_ModifyCreateRunOrgAndCreateRunPOMSprocs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117130042_ModifyCreateRunOrgAndCreateRunPOMSprocs', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    UPDATE organisation_data SET organisation_id = 0 WHERE organisation_id IS NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    UPDATE calculator_run_pom_data_detail SET organisation_id = 0 WHERE organisation_id IS NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'submission_period_desc');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [organisation_data] DROP COLUMN [submission_period_desc];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    EXEC sp_rename N'[calculator_run_organization_data_detail].[submission_period_desc]', N'status_code', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'organisation_id');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [organisation_data] ALTER COLUMN [organisation_id] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'obligation_status');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [organisation_data] ALTER COLUMN [obligation_status] nvarchar(10) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [error_code] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [num_days_obligated] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    ALTER TABLE [organisation_data] ADD [status_code] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'organisation_id');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [organisation_id] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'obligation_status');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [obligation_status] nvarchar(10) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [error_code] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    ALTER TABLE [calculator_run_organization_data_detail] ADD [num_days_obligated] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] ON;
    EXEC(N'INSERT INTO [error_type] ([id], [name])
    VALUES (11, N''Missing POM data'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
        SET IDENTITY_INSERT [error_type] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
    				DROP PROCEDURE [dbo].[CreateRunOrganization];
                    declare @Sql varchar(max);
    				SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]                
    				(                    @RunId int,                    @calendarYear varchar(400),                    @createdBy varchar(400)                )                
    				AS                
    				BEGIN                    
    				SET NOCOUNT ON                    
    					declare @DateNow datetime, @orgDataMasterid int                    
    						SET @DateNow = GETDATE()                    
    					declare @oldCalcRunOrgMasterId int                    
    						SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)                    
    					Update calculator_run_organization_data_master SET effective_to = @DateNow 
    						WHERE id = @oldCalcRunOrgMasterId                    
    					INSERT into dbo.calculator_run_organization_data_master                    
    						(calendar_year, created_at, created_by, effective_from, effective_to)                    
    					values                    
    						(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)                    
    					SET @orgDataMasterid  = CAST(scope_identity() AS int);                    
    					INSERT  into dbo.calculator_run_organization_data_detail                        
    						(calculator_run_organization_data_master_id,
    						load_ts,organisation_id,
    						organisation_name,
    						trading_name,                            
    						subsidiary_id,
    						obligation_status,
    						submitter_id,
                            status_code,
                            num_days_obligated,
                            error_code)                    
    					SELECT  @orgDataMasterid,                             
    					load_ts,                            
    					organisation_id,                            
    					organisation_name,                            
    					trading_name,                            
    					CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
    					obligation_status,
    					submitter_id,
                        status_code,
                        num_days_obligated,
                        error_code
    					from                             
    						dbo.organisation_data                    
    					Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
    					END'
    				EXEC(@Sql)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251204164331_AddColumnsToOrgDetailAndModifySproc', N'8.0.7');
END;
GO

COMMIT;
GO

