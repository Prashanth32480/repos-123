CREATE PROCEDURE [dbo].[usp_Participant_GetByParticipantIdForUnlink]
	@ParticipantGuid UNIQUEIDENTIFIER
AS
BEGIN	
SET NOCOUNT ON
    SELECT A.[ParticipantGuid],
		  [CricketId],
		  LegacyPlayerId,
		  PlayHQProfileId,
		  ParentCricketId,
		  IsSearchable,
		  IsNameVisible,
		  FirstName, 
		  LastName
	FROM [app].[Participant] A 
	JOIN [app].[ParticipantMapping] B 
	ON A.ParticipantGuid = B.ParticipantGuid
	WHERE A.[IsDelete] = 0 
	AND B.IsDelete = 0 
	AND A.ParticipantGuid=@ParticipantGuid
	AND B.LegacyPlayerId is not null
 SET NOCOUNT OFF  
END
