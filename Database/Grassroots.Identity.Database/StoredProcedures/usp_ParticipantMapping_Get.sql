CREATE PROCEDURE [dbo].[usp_ParticipantMapping_Get]
    @ParticipantGuid UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON
   SELECT [ParticipantGuid],
		  [PlayHQProfileId],	
		  [LegacyPlayerId]
	FROM [app].[ParticipantMapping] WITH(NOLOCK)
	WHERE ParticipantGuid=@ParticipantGuid
	AND IsDelete = 0
 SET NOCOUNT OFF  
END