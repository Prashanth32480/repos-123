CREATE PROC dbo.usp_Feed_Insert
@FeedTypeId tinyint ,
@MessageId nvarchar(200),
@BlobId nvarchar(200) = NULL,
@Category nvarchar(100) = NULL,
@EventRaisedDateTime DATETIME2(7)
AS
BEGIN

SET NOCOUNT ON

BEGIN
	INSERT INTO raw.Feed(
		[FeedTypeId],
		[MessageId],
		[BlobId],
		[Category],
		[EventRaisedDateTime]
	)
	SELECT	@FeedTypeId,
			@MessageId,
			@BlobId,
			@Category,
			@EventRaisedDateTime
	
	SELECT CAST(SCOPE_IDENTITY() AS bigint) AS Id
END

SET NOCOUNT OFF
END
GO