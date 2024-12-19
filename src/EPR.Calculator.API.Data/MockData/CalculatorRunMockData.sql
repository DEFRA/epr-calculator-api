DECLARE @testUser NVARCHAR = 'Test User'

INSERT INTO [calculator_run] ([calculator_run_classification_id], [created_at], [created_by], [financial_year], [name], [updated_at], [updated_by])
VALUES (1, '2025-08-28T10:01:00.0000000', @testUser, N'2024-25', N'Default settings check', NULL, NULL),
(2, '2025-08-21T12:09:00.0000000', @testUser, N'2024-25', N'Alteration check', NULL, NULL),
(3, '2025-08-11T09:14:00.0000000', @testUser, N'2024-25', N'Test 10', NULL, NULL),
(4, '2025-07-13T11:18:00.0000000', @testUser, N'2024-25', N'June check', NULL, NULL),
(4, '2025-07-10T08:13:00.0000000', @testUser, N'2024-25', N'Pre June check', NULL, NULL),
(4, '2025-07-08T10:00:00.0000000', @testUser, N'2024-25', N'Local Authority data check 5', NULL, NULL),
(4, '2025-07-07T11:20:00.0000000', @testUser, N'2024-25', N'Local Authority data check 4', NULL, NULL),
(4, '2025-06-24T14:29:00.0000000', @testUser, N'2024-25', N'Local Authority data check 3', NULL, NULL),
(4, '2025-06-23T16:39:12.0000000', @testUser, N'2024-25', N'Local Authority data check 2', NULL, NULL),
(4, '2025-06-14T17:06:26.0000000', @testUser, N'2024-25', N'Local Authority data check', NULL, NULL),
(5, '2025-06-01T09:12:00.0000000', @testUser, N'2024-25', N'Fee adjustment check', NULL, NULL);