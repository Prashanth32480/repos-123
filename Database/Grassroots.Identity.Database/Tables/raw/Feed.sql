CREATE TABLE [raw].[Feed] (
    [FeedId]                BIGINT          NOT NULL IDENTITY (1, 1),
    [FeedTypeId]            TINYINT         NOT NULL,
    [MessageId]             NVARCHAR(200)   NULL,
    [BlobId]                NVARCHAR(200)   NULL, 
    [Category]              NVARCHAR(100)   NULL, 
    [EventRaisedDateTime]   DATETIME2(7)    NULL, 
    [ProcessStatus]         SMALLINT        NOT NULL CONSTRAINT [DF_Feed_ProcessStatus] DEFAULT (0),
    [ProcessingDateTime]    DATETIME2(7)    NULL CONSTRAINT [DF_Feed_ProcessingDateTime] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Feed] PRIMARY KEY CLUSTERED ([FeedId] ASC),
    CONSTRAINT [FK_Feed_FeedType] FOREIGN KEY ([FeedTypeId]) REFERENCES [static].[FeedType] ([FeedTypeId])
);
GO

CREATE NONCLUSTERED INDEX [IDX_Feed_FeedId] 
ON [raw].[Feed] ([FeedId]) 
GO

CREATE NONCLUSTERED INDEX [IDX_Feed_MessageId] 
ON [raw].[Feed] ([MessageId]) 
GO