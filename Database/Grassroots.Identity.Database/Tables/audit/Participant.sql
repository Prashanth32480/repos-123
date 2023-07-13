CREATE TABLE [audit].[Participant](
	[AuditParticipantId]	BIGINT NOT NULL CONSTRAINT [PK_Participant] PRIMARY KEY IDENTITY(1,1),
	[AuditUserName]			NVARCHAR(50)	NULL,
	[AuditDate]				DATETIME2(7)	NULL,
	[ParticipantGuid] [uniqueidentifier] NOT NULL,
	[CricketId] UNIQUEIDENTIFIER NULL,
	[ParentCricketId] UNIQUEIDENTIFIER NULL,
	[FirstName] NVARCHAR(50) NOT NULL,
	[LastName] NVARCHAR(50) NOT NULL,
	[IsNameVisible] [BIT] NOT NULL,
	[IsSearchable] [BIT]	NOT NULL,
	[PurgeDate]			DATETIME2(7)		NULL,
	[IsDelete] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[CreatedFeedId] [bigint] NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[ModifiedFeedId] [bigint] NULL
)