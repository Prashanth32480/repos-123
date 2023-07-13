--EXEC [usp_FeedEvent_Get] KondoEntityGuid = 1BF3B152-AE65-4248-8662-D6B76CA9DEBE
CREATE PROCEDURE [dbo].[usp_FeedEvent_Get]
	@Id BigInt = null,
	@KondoEntityGuid UNIQUEIDENTIFIER = null,
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
				[KondoEntityGuid],
				[LastEventRaisedDateTime]
      FROM [raw].[FeedEvent] WITH (NOLOCK) 
	  WHERE (@Id IS  NULL OR [Id] = @Id)
			 AND (@KondoEntityGuid IS  NULL OR [KondoEntityGuid] = @KondoEntityGuid)
			 AND (@SourceEntityGuid IS  NULL OR [SourceEntityGuid] = @SourceEntityGuid)
	  END
SET NOCOUNT OFF 
END 