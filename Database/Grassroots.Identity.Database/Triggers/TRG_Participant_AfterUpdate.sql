CREATE TRIGGER [app].[trg_Participant_AfterUpdate] 
ON [app].[Participant] 
AFTER UPDATE 
AS BEGIN
SET NOCOUNT ON

INSERT INTO [audit].[Participant]
SELECT 
SUSER_NAME(),
GetUTCDate(),
[ParticipantGuid],
[CricketId],
ParentCricketId,
FirstName,
LastName	,
[IsNameVisible]	,
IsSearchable,
[PurgeDate],
IsDelete,
CreatedDate,
CreatedFeedId,
ModifiedDate,
ModifiedFeedId		
FROM Deleted
SET NOCOUNT OFF
END