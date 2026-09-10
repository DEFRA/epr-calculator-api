BEGIN TRANSACTION;
UPDATE [material] SET [name] = N'Fibre composite', [description] = N'Fibre composite'
WHERE [code] = N'FC';
SELECT @@ROWCOUNT;


UPDATE [material] SET [name] = N'Paper or card', [description] = N'Paper or card'
WHERE [code] = N'PC';
SELECT @@ROWCOUNT;


UPDATE [material] SET [name] = N'Other materials', [description] = N'Other materials'
WHERE [code] = N'OT';
SELECT @@ROWCOUNT;


DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260810155245_UpdateMaterialName';

COMMIT;
GO

