CREATE PROCEDURE [dbo].[usp_ExternalFeed_GetByMessageId]
	@MessageId nvarchar(200)
AS
BEGIN 
      --EXEC [usp_Feed_Get] 1BF3B152-AE65-4248-8662-D6B76CA9DEBE
      SET NOCOUNT ON

      SELECT		[FeedId] as FeedId,
					[FeedTypeId] as FeedTypeId,
					[MessageId] as MessageId,
					[BlobId] as BlobId,
					[Category] as Category,
					[ProcessingDateTime] as ProcessingDateTime
      FROM [raw].[ExternalFeed] WITH (NOLOCK) 
	  WHERE [MessageId] = @MessageId
			
      SET NOCOUNT OFF 
END 