--EXEC [usp_ExternalFeedEvent_Get] DestinationEntityGuid = 1BF3B152-AE65-4248-8662-D6B76CA9DEBE
CREATE PROCEDURE [dbo].[usp_ExternalFeedEvent_Get]
	@Id BigInt = null,
	@DestinationEntityGuid UNIQUEIDENTIFIER = null,
	@SourceEntityGuid UNIQUEIDENTIFIER = null
AS
BEGIN 
SET NOCOUNT ON 

	  BEGIN
      SELECT	[Id],
				[EventType],
				[FeedType],
				[SourceSystem],
				[SourceEntityGuid],
				[DestinationEntityGuid],
				[LastEventRaisedDateTime]
      FROM [raw].[ExternalFeedEvent] WITH (NOLOCK) 
	  WHERE (@Id IS  NULL OR [Id] = @Id)
			 AND (@DestinationEntityGuid IS  NULL OR [DestinationEntityGuid] = @DestinationEntityGuid)
			 AND (@SourceEntityGuid IS  NULL OR [SourceEntityGuid] = @SourceEntityGuid)
	  END
SET NOCOUNT OFF 
END 