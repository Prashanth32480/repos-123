--EXEC [usp_FeedEvent_Delete] 4703ef24-705a-402a-b6a9-227c32f8a2be
CREATE PROCEDURE [dbo].[usp_FeedEvent_Delete]
	@KondoEntityGuid UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON 

	  BEGIN
      DELETE FROM [raw].[FeedEvent]
	  OUTPUT DELETED.Id
        WHERE [KondoEntityGuid] = @KondoEntityGuid
	  END

SET NOCOUNT OFF 
END 