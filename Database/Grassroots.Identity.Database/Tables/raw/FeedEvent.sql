CREATE TABLE raw.FeedEvent
(
Id						BIGINT				NOT NULL IDENTITY (1, 1),
EventType				NVARCHAR(40)		NOT NULL,
FeedType				NVARCHAR(30)		NOT NULL,
SourceSystem			NVARCHAR(30)		NOT NULL,
SourceEntityGuid		UNIQUEIDENTIFIER	NOT NULL,
KondoEntityGuid			UNIQUEIDENTIFIER	NULL,
LastEventRaisedDateTime	DATETIME2(7)		NOT NULL,
CONSTRAINT [PK_FeedEvent_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
)