CREATE TRIGGER [app].[trg_ParticipantMapping_AfterUpdate] 
ON [app].[ParticipantMapping] 
AFTER UPDATE 
AS BEGIN
SET NOCOUNT ON

INSERT INTO [audit].[ParticipantMapping]
SELECT 
SUSER_NAME(),
GetUTCDate(),
ParticipantMappingId,
ParticipantGuid,
PlayHQProfileId,
LegacyPlayerId,
PurgeDate,
IsDelete,
CreatedDate,
CreatedFeedId,
ModifiedDate,
ModifiedFeedId
FROM Deleted
SET NOCOUNT OFF
END
