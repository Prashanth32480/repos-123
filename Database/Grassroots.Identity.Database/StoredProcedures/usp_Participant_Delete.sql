CREATE PROCEDURE [dbo].[usp_Participant_Delete]
	@ParticipantGuid UNIQUEIDENTIFIER,
	@FeedId bigint
AS
BEGIN	
SET NOCOUNT ON
		UPDATE app.[Participant] SET 
			[IsDelete] = 1,
			[ModifiedFeedId] = @FeedId,
			[ModifiedDate] = GETUTCDATE()		
		WHERE ParticipantGuid=@ParticipantGuid	

SET NOCOUNT OFF
END