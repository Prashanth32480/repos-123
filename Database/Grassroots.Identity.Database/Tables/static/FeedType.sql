CREATE TABLE [static].[FeedType] (
    [FeedTypeId]    TINYINT         NOT NULL IDENTITY,
    [FeedName]      VARCHAR (30)    NOT NULL,
    CONSTRAINT [PK_FeedType] PRIMARY KEY CLUSTERED ([FeedTypeId] ASC)
);
GO