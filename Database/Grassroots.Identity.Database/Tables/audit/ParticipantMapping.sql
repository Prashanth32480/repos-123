CREATE TABLE [audit].[ParticipantMapping]
(
	[AuditParticipantMappingId]	BIGINT NOT NULL CONSTRAINT [PK_ParticipantMapping] PRIMARY KEY IDENTITY(1,1),
	[AuditUserName]			NVARCHAR(50)	NULL,
	[AuditDate]				DATETIME2(7)	NULL,
	[ParticipantMappingId]	INT NOT NULL,
	[ParticipantGuid]		UNIQUEIDENTIFIER NOT NULL,
	[PlayHQProfileId]		UNIQUEIDENTIFIER NULL,
	[LegacyPlayerId]		INT NULL,
	[PurgeDate]				DATETIME2(7) NULL,
	[IsDelete]				BIT NOT NULL,
	[CreatedDate]			DATETIME2(7) NOT NULL,
	[CreatedFeedId]			BIGINT NOT NULL,
	[ModifiedDate]			DATETIME2(7) NULL,
	[ModifiedFeedId]		BIGINT NULL
)