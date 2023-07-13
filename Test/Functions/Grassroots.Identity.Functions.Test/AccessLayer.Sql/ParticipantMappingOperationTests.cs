using System.Collections.Generic;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.Model.DbEntity;
using Moq;
using Xunit;
using System;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using System.Diagnostics;

namespace Grassroots.Identity.Functions.Test.AccessLayer.Sql
{
    public class ParticipantMappingOperationTests
    {
        [Fact]
        public void GetParticipantMappingByParticipantGuidFromDbGetSingleValidResponse()
        {
            // Setup
            var participant = new ParticipantMapping { ParticipantGuid = Guid.NewGuid(),
                PlayHqProfileId = Guid.NewGuid(),
                LegacyPlayerId = null

            };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantMapping, ParticipantMappingRequestModel>(It.IsAny<string>(), It.IsAny<ParticipantMappingRequestModel>()))
                .ReturnsAsync(new List<ParticipantMapping> { participant });
            var db = new ParticipantMappingOperations(dbFactory.Object);

            var response = db.GetParticipantMappingByParticipanyGuid(Guid.NewGuid().ToString());

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public void GetParticipantMappingByParticipantGuidFromDbWithEmptyIdRequestShouldFail()
        {
            // Setup
            var participant = new ParticipantMapping();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantMapping, ParticipantMappingRequestModel>(It.IsAny<string>(), It.IsAny<ParticipantMappingRequestModel>()))
                .ReturnsAsync(new List<ParticipantMapping> { participant });
            var db = new ParticipantMappingOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.GetParticipantMappingByParticipanyGuid(string.Empty));
        }


        [Fact]
        public async void SaveParticipantMappingWithCorrectInputsReturnsFeedId()
        {
            // Setup
            var participantGuid = Guid.NewGuid();
            var dbResponseCompetition = new ParticipantMapping { ParticipantGuid = participantGuid };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultAsync<ParticipantMapping, ParticipantMappingSaveModel>(It.IsAny<string>(), It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(() => dbResponseCompetition);

            var db = new ParticipantMappingOperations(dbFactory.Object);

            // Act
            var contact = new ParticipantMappingSaveModel { ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1") };
            await db.SaveParticipantMapping(contact);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteResultAsync<ParticipantMapping, ParticipantMappingSaveModel>(It.IsAny<string>(),
                        It.IsAny<ParticipantMappingSaveModel>()),
                Times.Once);
        }

        [Fact]
        public void SaveParticipantMappingWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);

            dbConnection.Setup(connection =>
                    connection.ExecuteResultAsync<Participant, ParticipantMappingSaveModel>(It.IsAny<string>(), It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new ParticipantMappingOperations(dbFactory.Object);

            // Act
            var contact = new ParticipantMappingSaveModel
            {
                ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1")
            };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveParticipantMapping(contact));
        }

        [Fact]
        public void SaveParticipantMapping_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup        
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new ParticipantMappingOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveParticipantMapping(null));
        }

        [Fact]
        public async void DeleteParticipantWithCorrectInputs()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(1);

            var db = new ParticipantMappingOperations(dbFactory.Object);
            var contact = new ParticipantMappingSaveModel
            {
                ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1"),
                FeedId = 1
            };

            // Act
            await db.DeleteParticipantMapping(contact);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteNonQueryAsync(It.IsAny<string>(),
                        It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async void DeleteParticipantMappingByPlayHQProfileIdWithCorrectInputs()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(1);

            var db = new ParticipantMappingOperations(dbFactory.Object);
            var obj = new ParticipantMappingSaveModel
            {
                ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1"),
                FeedId = 1
            };

            // Act
            await db.DeleteParticipantMappingByPlayHQProfileId(obj);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteNonQueryAsync(It.IsAny<string>(),
                        It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteParticipantMappingByPlayHQProfileId_ThrowsException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(1);

            var db = new ParticipantMappingOperations(dbFactory.Object);

            // Assert
            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteParticipantMappingByPlayHQProfileId(null));
        }

        [Fact]
        public async void UnlinkMyCricketIdWithCorrectInputs()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(1);

            var db = new ParticipantMappingOperations(dbFactory.Object);
            var contact = new ParticipantMappingSaveModel
            {
                ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1"),
                FeedId = 1
            };

            // Act
            await db.UnlinkMyCricketId(contact);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteNonQueryAsync(It.IsAny<string>(),
                        It.IsAny<object>()), Times.Once);
        }
    }
}
