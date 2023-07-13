using System.Collections.Generic;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Moq;
using Xunit;
using System;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;

namespace Grassroots.Identity.Functions.Test.AccessLayer.Sql
{
    public class ParticipantOperationTests
    {
        [Fact]
        public void GetParticipantByMyCricketIdFromDb_GetSingleValidResponse()
        {
            // Setup
            var participant = new ParticipantDetails();// { PlayHQProfileId = Guid.NewGuid() };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            var response = db.GetParticipantByMyCricketId(1);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetParticipantByMyCricketIdFromDb_WithEmptyIdRequest_ShouldFail()
        {
            // Setup
            var participant = new ParticipantDetails();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.GetParticipantByMyCricketId(0));
        }

        [Fact]
        public void GetParticipantByPlayHQProfileIdFromDbGetSingleValidResponse()
        {
            // Setup
            var participant = new Participant();// { PlayHQProfileId = Guid.NewGuid() };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<Participant, ParticipantRequestModel>(It.IsAny<string>(), It.IsAny<ParticipantRequestModel>()))
                .ReturnsAsync(new List<Participant> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            var response = db.GetParticipantByPlayHQProfileId(Guid.NewGuid().ToString());

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetParticipantByPlayHQProfileIdFromDbWithEmptyIdRequestShouldFail()
        {
            // Setup
            var participant = new Participant();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<Participant, ParticipantRequestModel>(It.IsAny<string>(), It.IsAny<ParticipantRequestModel>()))
                .ReturnsAsync(new List<Participant> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.GetParticipantByPlayHQProfileId(string.Empty));
        }


        [Fact]
        public async void SaveParticipantWithCorrectInputsReturnsFeedId()
        {
            // Setup
            var participantGuid = Guid.NewGuid();
            var dbResponseCompetition = new Participant();// { PlayHQProfileId = participantGuid };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultAsync<Participant, ParticipantSaveModel>(It.IsAny<string>(), It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(() => dbResponseCompetition);

            var db = new ParticipantOperations(dbFactory.Object);

            // Act
            var contact = new ParticipantSaveModel();// { FeedId = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1") };
            await db.SaveParticipant(contact);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteResultAsync<Participant, ParticipantSaveModel>(It.IsAny<string>(),
                        It.IsAny<ParticipantSaveModel>()),
                Times.Once);
        }

        [Fact]
        public void SaveParticipantWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);

            dbConnection.Setup(connection =>
                    connection.ExecuteResultAsync<Participant, ParticipantSaveModel>(It.IsAny<string>(), It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new ParticipantOperations(dbFactory.Object);

            // Act
            var contact = new ParticipantSaveModel
            {
                CricketId = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1")
            };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveParticipant(contact));
        }

        [Fact]
        public void SaveParticipant_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup        
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveParticipant(null));
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

            var db = new ParticipantOperations(dbFactory.Object);

            // Act
            var contact = new ParticipantSaveModel
            {
                FeedId = 1,
                ParticipantGuid = new Guid("09016E9F-FC3A-4C0E-8CEC-D032FA2ABEA1")
            };
            await db.DeleteParticipant(contact);


            // Assert
            dbConnection.Verify(
                connection =>
                    connection.ExecuteNonQueryAsync(It.IsAny<string>(),
                        It.IsAny<object>()), Times.Once);
        }


        [Fact]
        public void DeleteParticipant_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup        
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteParticipant(null));
        }

        [Fact]
        public void GetParticipantByCricketIdFromDb_GetSingleValidResponse()
        {
            // Setup
            var participant = new ParticipantDetails();// { PlayHQProfileId = Guid.NewGuid() };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            var response = db.GetParticipantByParticipantId("1");

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetParticipantByCricketIdFromDb_WithEmptyIdRequest_ShouldFail()
        {
            // Setup
            var participant = new ParticipantDetails();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.GetParticipantByParticipantId(""));
        }

        [Fact]
        public void GetParticipantByCricketIdForUnlinkFromDb_GetSingleValidResponse()
        {
            // Setup
            var participant = new ParticipantDetails();// { PlayHQProfileId = Guid.NewGuid() };
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            var response = db.GetParticipantByParticipantIdForUnlink("1");

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetParticipantByCricketIdForUnlinkFromDb_WithEmptyIdRequest_ShouldFail()
        {
            // Setup
            var participant = new ParticipantDetails();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(It.IsAny<string>(), It.IsAny<GetParticipantByMyCricketIdRequestModel>()))
                .ReturnsAsync(new List<ParticipantDetails> { participant });
            var db = new ParticipantOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.GetParticipantByParticipantIdForUnlink(""));
        }
    }
}
