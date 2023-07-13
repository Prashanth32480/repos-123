CREATE TABLE [app].[ParticipantMapping]
(
	[ParticipantMappingId]	INT NOT NULL CONSTRAINT [PK_ParticipantMapping] PRIMARY KEY Identity(1,1),
	[ParticipantGuid]		UNIQUEIDENTIFIER NOT NULL,
	[PlayHQProfileId]		UNIQUEIDENTIFIER NULL,
	[LegacyPlayerId]		INT NULL,
	[PurgeDate]				DATETIME2(7)		NULL,
	[IsDelete]				BIT NOT NULL  CONSTRAINT [DF_ParticipantMapping_IsDelete] DEFAULT 0,
	[CreatedDate]			DATETIME2(7) NOT NULL CONSTRAINT [DF_ParticipantMapping_CreatedDate] DEFAULT GETUTCDATE(),
	[CreatedFeedId]			BIGINT NOT NULL CONSTRAINT [DF_ParticipantMapping_CreatedFeedId] DEFAULT 0,
	[ModifiedDate]			DATETIME2(7) NULL,
	[ModifiedFeedId]		BIGINT NULL,
	CONSTRAINT FK_ParticipantMapping_ParticipantGuid FOREIGN KEY ([ParticipantGuid]) REFERENCES [app].[Participant]([ParticipantGuid])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_ParticipantMapping_PlayHQProfileId_Unique
ON [app].[ParticipantMapping]([PlayHQProfileId])
WHERE PlayHQProfileId is not null and IsDelete = 0 
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_ParticipantMapping_LegacyPlayerId_Unique
ON [app].[ParticipantMapping]([LegacyPlayerId])
WHERE LegacyPlayerId is not null and IsDelete = 0 
GO