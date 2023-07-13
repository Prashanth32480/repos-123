--EXEC [usp_FeedEvent_Get] 
--	@EventType = 'Competition.Created', 
--	@FeedType = 'Competition',
--	@SourceSyatem = 'playHQ',
--	@SourceEntityGuid = 1BF3B152-AE65-4248-8662-D6B76CA9DEBEy,
--	@KondoEntityGuid = 4CN3B152-BE65-4248-9262-D6B76CA9ACDC,
--	@LastEventRaisedDateTime = '23/12/2021'
CREATE PROCEDURE [dbo].[usp_FeedEvent_Save]
	@Id BIGINT = NULL,
	@EventType NVARCHAR(40),
	@FeedType NVARCHAR(30),
	@SourceSystem NVARCHAR(30),
	@SourceEntityGuid UNIQUEIDENTIFIER,
	@KondoEntityGuid UNIQUEIDENTIFIER,
	@LastEventRaisedDateTime DATETIME2
AS
BEGIN 
SET NOCOUNT ON

	  IF (@Id IS NULL)  BEGIN
	  DECLARE @NewIdTable table([NewId] BIGINT);
      INSERT INTO [raw].[FeedEvent]
			   ([EventType],
			    [FeedType],
			    [SourceSystem],
				[SourceEntityGuid],
				[KondoEntityGuid],
				[LastEventRaisedDateTime])
		 OUTPUT INSERTED.Id INTO @NewIdTable
		 VALUES
			   (@EventType,
				@FeedType,
				@SourceSystem,
				@SourceEntityGuid,
				@KondoEntityGuid,
				@LastEventRaisedDateTime)
		SELECT [NewId] AS Id FROM @NewIdTable
	  END

	  ELSE BEGIN
	  UPDATE [raw].[FeedEvent] SET
				[EventType] = @EventType,
				[SourceSystem] = @SourceSystem,
				[SourceEntityGuid] = @SourceEntityGuid,
				[LastEventRaisedDateTime] = @LastEventRaisedDateTime
	  WHERE [Id] = @Id
	  SELECT @Id AS Id
	  END

SET NOCOUNT OFF 
END 