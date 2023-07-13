CREATE PROCEDURE [dbo].[usp_Participant_Get]
    @PlayHQProfileId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON
    SELECT A.[ParticipantGuid],
		  [CricketId],
		  [ParentCricketId]
	FROM [app].[Participant] A 
	JOIN [app].[ParticipantMapping] B 
	ON A.ParticipantGuid = B.ParticipantGuid
	WHERE A.[IsDelete] = 0 AND B.[IsDelete] = 0 AND B.PlayHQProfileId=@PlayHQProfileId
 SET NOCOUNT OFF  
END