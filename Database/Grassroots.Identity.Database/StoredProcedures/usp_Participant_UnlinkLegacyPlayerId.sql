CREATE PROCEDURE [dbo].[usp_Participant_UnlinkLegacyPlayerId]
	@ParticipantGuid UNIQUEIDENTIFIER,
	@FeedId bigint,
	@LegacyPlayerId int
AS
BEGIN	
SET NOCOUNT ON
		IF (@LegacyPlayerId is null OR @LegacyPlayerId = 0)
			BEGIN
				UPDATE app.[ParticipantMapping] SET 
					[IsDelete] = 1,
					[ModifiedFeedId] = @FeedId,
					[ModifiedDate] = GETUTCDATE()		
				WHERE ParticipantGuid=@ParticipantGuid	
				AND LegacyPlayerId is not null
				AND IsDelete = 0
			END
		ELSE 
			BEGIN
				UPDATE app.[ParticipantMapping] SET 
					[IsDelete] = 1,
					[ModifiedFeedId] = @FeedId,
					[ModifiedDate] = GETUTCDATE()		
				WHERE ParticipantGuid=@ParticipantGuid	
				AND LegacyPlayerId = @LegacyPlayerId
				AND IsDelete = 0
			END
SET NOCOUNT OFF
END

