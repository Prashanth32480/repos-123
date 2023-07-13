CREATE PROCEDURE [dbo].[usp_ParticipantMapping_Delete]
	@ParticipantGuid UNIQUEIDENTIFIER,
	@FeedId bigint
AS
BEGIN	
SET NOCOUNT ON
		UPDATE app.[ParticipantMapping] SET 
			[IsDelete] = 1,
			[ModifiedFeedId] = @FeedId,
			[ModifiedDate] = GETUTCDATE()		
		WHERE ParticipantGuid=@ParticipantGuid	

SET NOCOUNT OFF
END
