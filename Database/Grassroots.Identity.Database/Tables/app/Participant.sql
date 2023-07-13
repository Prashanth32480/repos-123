CREATE TABLE [app].[Participant]
(
ParticipantGuid		UNIQUEIDENTIFIER	NOT NULL CONSTRAINT [PK_Participant] PRIMARY KEY DEFAULT NEWID(),
CricketId 	UNIQUEIDENTIFIER	NULL,
ParentCricketId UNIQUEIDENTIFIER NULL,
FirstName				NVARCHAR(50)		NOT NULL,
LastName				NVARCHAR(50)		NOT NULL,
[IsNameVisible]	BIT				NOT NULL CONSTRAINT [DF_Participant_IsNameSuppressed] DEFAULT(0),
IsSearchable	BIT				NOT NULL CONSTRAINT [DF_Participant_IsSearchable] DEFAULT(0),
[PurgeDate]			DATETIME2(7)		NULL,
IsDelete			BIT					NOT NULL CONSTRAINT [DF_Participant_IsDelete] DEFAULT(0),
CreatedDate			DATETIME2(7)		NOT NULL CONSTRAINT [DF_Participant_CreatedDate] Default GETUTCDATE(),
CreatedFeedId		BIGINT				NOT NULL CONSTRAINT [DF_Participant_CreatedFeedId] DEFAULT 0,
ModifiedDate		DATETIME2(7)		NULL,
ModifiedFeedId		BIGINT				NULL
)