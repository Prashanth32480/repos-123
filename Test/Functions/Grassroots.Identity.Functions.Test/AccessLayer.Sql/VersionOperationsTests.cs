using System;
using System.Collections.Generic;
using System.Linq;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.AccessLayer.Sql
{
    public class VersionOperationsTests
    {
        [Fact]
        public void GetAppVersionFromDbGetValidResponse()
        {
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<Database.Model.DbEntity.Version, VersionRequestModel>(It.IsAny<string>(), It.IsAny<VersionRequestModel>()))
                   .ReturnsAsync(new List<Database.Model.DbEntity.Version> { new Database.Model.DbEntity.Version { VersionNumber = Convert.ToDecimal(1.0), Environment = "Dev" } });

            var db = new VersionOperations(dbFactory.Object);

            // Act
            var response = db.GetVersion().Result.FirstOrDefault();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(response.VersionNumber, Convert.ToDecimal(1.0));
        }
    }
}
