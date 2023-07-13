CREATE PROCEDURE [dbo].[usp_ParticipantMapping_Save]
	@ParticipantGuid UNIQUEIDENTIFIER,
	@PlayHQProfileId UNIQUEIDENTIFIER,
	@LegacyPlayerId int,
	@FeedId bigint
AS
BEGIN	
SET NOCOUNT ON
DECLARE @ParticipantGuidTable table([newParticipantGuid] [uniqueidentifier]);
	BEGIN
		INSERT INTO [app].[ParticipantMapping]
					([ParticipantGuid],
					[PlayHQProfileId],
					[LegacyPlayerId],
					[IsDelete],
					[CreatedFeedId],
					[CreatedDate],
					[ModifiedFeedId],
					[ModifiedDate])
				OUTPUT INSERTED.ParticipantGuid INTO @ParticipantGuidTable
				VALUES
					(@ParticipantGuid,
					@PlayHQProfileId,
					@LegacyPlayerId,
					0,
					@FeedId,
					GETUTCDATE(),
					@FeedId,
					GETUTCDATE())

		SELECT [newParticipantGuid] AS ParticipantGuid FROM @ParticipantGuidTable

	END

SET NOCOUNT OFF
END