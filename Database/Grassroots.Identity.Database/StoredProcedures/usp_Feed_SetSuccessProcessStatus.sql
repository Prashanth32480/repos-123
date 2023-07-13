CREATE PROC dbo.usp_Feed_SetSuccessProcessStatus
@FeedId bigint
AS
BEGIN

SET NOCOUNT ON

UPDATE raw.Feed
	SET ProcessStatus = 1
	OUTPUT INSERTED.FeedId
	WHERE FeedId=@FeedId

SET NOCOUNT OFF
END
GO