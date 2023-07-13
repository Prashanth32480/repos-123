CREATE PROCEDURE [dbo].[usp_ParticipantMappingByPlayHQProfileId_Delete]
	@PlayHQProfileId UNIQUEIDENTIFIER,
	@ParticipantGuid UNIQUEIDENTIFIER,
	@FeedId bigint
AS
BEGIN	
SET NOCOUNT ON
		UPDATE app.[ParticipantMapping] SET 
			[IsDelete] = 1,
			[ModifiedFeedId] = @FeedId,
			[ModifiedDate] = GETUTCDATE()		
		WHERE PlayHQProfileId=@PlayHQProfileId And ParticipantGuid = @ParticipantGuid

SET NOCOUNT OFF
END
