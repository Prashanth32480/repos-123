CREATE PROCEDURE [dbo].[usp_Participant_Save]
	@ParticipantGuid UNIQUEIDENTIFIER,
	@CricketId UNIQUEIDENTIFIER,
	@FirstName varchar(50),	
    @LastName varchar(50),
	@IsNameVisible bit,
	@IsSearchable bit,
	@FeedId bigint,
	@ParentCricketId UNIQUEIDENTIFIER
AS
BEGIN	
SET NOCOUNT ON
	IF (@ParticipantGuid ='00000000-0000-0000-0000-000000000000')
	BEGIN
		DECLARE @ParticipantGuidTable table([newParticipantGuid] [uniqueidentifier]);
	
		INSERT INTO [app].[Participant]
			   ([CricketId],
				[ParentCricketId],
			    [FirstName],
				[LastName],
				[IsNameVisible],
				[IsSearchable],
				[IsDelete],
				[CreatedFeedId],
				[CreatedDate],
				[ModifiedFeedId],
				[ModifiedDate])
		 OUTPUT INSERTED.ParticipantGuid INTO @ParticipantGuidTable
		 VALUES
			   (@CricketId,
				@ParentCricketId,
				@FirstName,
				@LastName,
				@IsNameVisible,
				@IsSearchable,
				0,
				@FeedId,
				GETUTCDATE(),
				@FeedId,
				GETUTCDATE())

		   SELECT [newParticipantGuid] AS ParticipantGuid FROM @ParticipantGuidTable
	END
	ELSE BEGIN
		UPDATE app.[Participant] SET 
				[CricketId] = @CricketId,
				[ParentCricketId] = @ParentCricketId,
				[FirstName] = @FirstName ,
				[LastName] = @LastName,
				[IsNameVisible] = @IsNameVisible,
				[IsSearchable] = @IsSearchable,
				[IsDelete] = 0,
				[ModifiedFeedId] = @FeedId,
				[ModifiedDate] = GETUTCDATE()
		WHERE ParticipantGuid=@ParticipantGuid

		SELECT @ParticipantGuid AS ParticipantGuid
	END

SET NOCOUNT OFF
END
