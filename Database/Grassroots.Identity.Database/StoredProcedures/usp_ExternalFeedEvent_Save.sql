--EXEC [usp_ExternalFeedEvent_Get] 
--	@EventType = 'Competition.Created', 
--	@FeedType = 'Competition',
--	@SourceSyatem = 'playHQ',
--	@SourceEntityGuid = 1BF3B152-AE65-4248-8662-D6B76CA9DEBEy,
--	@DestinationEntityGuid = 4CN3B152-BE65-4248-9262-D6B76CA9ACDC,
--	@LastEventRaisedDateTime = '23/12/2021'

CREATE PROCEDURE [dbo].[usp_ExternalFeedEvent_Save]
	@Id BIGINT = NULL,
	@EventType NVARCHAR(40),
	@FeedType NVARCHAR(30),
	@SourceSystem NVARCHAR(30),
	@SourceEntityGuid UNIQUEIDENTIFIER,
	@DestinationEntityGuid UNIQUEIDENTIFIER,
	@LastEventRaisedDateTime DATETIME2
AS
BEGIN 
SET NOCOUNT ON

	  IF (@Id IS NULL)  BEGIN
	  DECLARE @NewIdTable table([NewId] BIGINT);
      INSERT INTO [raw].[ExternalFeedEvent]
			   ([EventType],
			    [FeedType],
			    [SourceSystem],
				[SourceEntityGuid],
				[DestinationEntityGuid],
				[LastEventRaisedDateTime])
		 OUTPUT INSERTED.Id INTO @NewIdTable
		 VALUES
			   (@EventType,
				@FeedType,
				@SourceSystem,
				@SourceEntityGuid,
				@DestinationEntityGuid,
				@LastEventRaisedDateTime)
		SELECT [NewId] AS Id FROM @NewIdTable
	  END

	  ELSE BEGIN
	  UPDATE [raw].[ExternalFeedEvent] SET
				[EventType] = @EventType,
				[SourceSystem] = @SourceSystem,
				[SourceEntityGuid] = @SourceEntityGuid,
				[LastEventRaisedDateTime] = @LastEventRaisedDateTime
	  WHERE [Id] = @Id
	  SELECT @Id AS Id
	  END

SET NOCOUNT OFF 
END 