--EXEC [usp_ExternalFeedEvent_Delete] 4703ef24-705a-402a-b6a9-227c32f8a2be
CREATE PROCEDURE [dbo].[usp_ExternalFeedEvent_Delete]
	@DestinationEntityGuid UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON 

	  BEGIN
      DELETE FROM [raw].[ExternalFeedEvent]
	  OUTPUT DELETED.Id
        WHERE [DestinationEntityGuid] = @DestinationEntityGuid
	  END

SET NOCOUNT OFF 
END 