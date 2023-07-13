CREATE PROCEDURE [dbo].[usp_Participant_GetByMyCricketId]
	@MyCricketId Int
AS
BEGIN	
SET NOCOUNT ON
    SELECT A.[ParticipantGuid],
		  [CricketId],
		  LegacyPlayerId,
		  PlayHQProfileId,
		  FirstName,
		  LastName,
		  IsNameVisible,
		  IsSearchable
	FROM [app].[Participant] A 
	JOIN [app].[ParticipantMapping] B 
	ON A.ParticipantGuid = B.ParticipantGuid
	WHERE A.[IsDelete] = 0 AND B.IsDelete = 0 AND B.LegacyPlayerId=@MyCricketId
 SET NOCOUNT OFF  
END