CREATE PROCEDURE [dbo].[usp_Version_Get]
@Limit INT
AS
BEGIN
    SET NOCOUNT ON
   
    SELECT TOP (@Limit) [VersionNumber], [Environment] FROM [app].[Version]

    SET NOCOUNT OFF  
END