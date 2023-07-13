CREATE PROC dbo.usp_ExternalFeed_SetSuccessProcessStatus
@FeedId bigint
AS
BEGIN

SET NOCOUNT ON

UPDATE raw.ExternalFeed
	SET ProcessStatus = 1
	OUTPUT INSERTED.FeedId
	WHERE FeedId=@FeedId

SET NOCOUNT OFF
END
GO