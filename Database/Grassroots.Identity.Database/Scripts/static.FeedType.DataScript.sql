SET IDENTITY_INSERT [static].[FeedType] ON 

MERGE INTO [static].[FeedType] AS Target 
USING (VALUES 
  (1, N'Competition'),
  (2, N'Program'),
  (3, N'Account'),
  (4, N'Profile'),
  (5, N'Insider'),
  (6, N'OneCustomer')
) 
AS Source (FeedTypeId, FeedName) 
ON Target.FeedTypeId = Source.FeedTypeId 

WHEN MATCHED THEN 
UPDATE SET FeedName = Source.FeedName 

WHEN NOT MATCHED BY TARGET THEN 
INSERT (FeedTypeId, FeedName) 
VALUES (FeedTypeId, FeedName) 

WHEN NOT MATCHED BY SOURCE THEN 
DELETE;

SET IDENTITY_INSERT [static].[FeedType] OFF