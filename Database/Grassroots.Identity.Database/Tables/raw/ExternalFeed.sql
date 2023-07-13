CREATE TABLE [raw].[ExternalFeed] (
    [FeedId]                BIGINT          NOT NULL IDENTITY (1, 1),
    [FeedTypeId]            TINYINT         NOT NULL,
    [MessageId]             NVARCHAR(200)   NULL,
    [BlobId]                NVARCHAR(200)   NULL, 
    [Category]              NVARCHAR(100)   NULL, 
    [EventRaisedDateTime]   DATETIME2(7)    NULL, 
    [ProcessStatus]         SMALLINT        NOT NULL CONSTRAINT [DF_ExternalFeed_ProcessStatus] DEFAULT (0),
    [ProcessingDateTime]    DATETIME2(7)    NULL CONSTRAINT [DF_ExternalFeed_ProcessingDateTime] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ExternalFeed] PRIMARY KEY CLUSTERED ([FeedId] ASC),
    CONSTRAINT [FK_ExternalFeed_FeedType] FOREIGN KEY ([FeedTypeId]) REFERENCES [static].[FeedType] ([FeedTypeId])
);
GO

CREATE NONCLUSTERED INDEX [IDX_ExternalFeed_FeedId] 
ON [raw].[ExternalFeed] ([FeedId]) 
GO

CREATE NONCLUSTERED INDEX [IDX_ExternalFeed_MessageId] 
ON [raw].[ExternalFeed] ([MessageId]) 
GO