BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[material]') AND [c].[name] = N'display_order');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [material] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [material] DROP COLUMN [display_order];

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261026125521_AddDisplayOrderToMaterial';

COMMIT;
GO

