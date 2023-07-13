using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Grassroots.Identity.Database.Model.Static;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.AccessLayer.Sql
{
    public class FeedEventOperationTests
    {
        #region SaveFeedEvent
        [Fact]
        public async void SaveFeedEventWithCorrectInputsReturnsFeedId()
        {
            // Arrange
            const long id = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()))
                .ReturnsAsync(id);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var feedEvent = new FeedEventSaveModel
            {
                Id = id,
                EventType = "Competition.Created",
                FeedType = FeedType.Competition.ToString(),
                SourceSystem = "PlayHQ",
                SourceEntityGuid = Guid.NewGuid(),
                KondoEntityGuid = Guid.NewGuid(),
                LastEventRaisedDateTime = DateTime.UtcNow
            };
            var response = await db.SaveFeedEvent(feedEvent);

            // Assert
            Assert.Equal(response, id);
        }

        [Fact]
        public void SaveFeedEventWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var feedEvent = new FeedEventSaveModel
            {
                Id = new Random().Next(),
                EventType = "Competition.Created",
                FeedType = FeedType.Competition.ToString(),
                SourceSystem = "PlayHQ",
                SourceEntityGuid = Guid.NewGuid(),
                KondoEntityGuid = Guid.NewGuid(),
                LastEventRaisedDateTime = DateTime.UtcNow
            };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveFeedEvent(feedEvent));
        }

        [Fact]
        public void SaveFeedEvent_WhenRequestNull_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.SaveFeedEvent(null));
        }
        #endregion

        #region UpdateLastEventRaisedDateTime
        [Fact]
        public async void UpdateLastEventRaisedDateTimeFeedEventOperations_WithCorrectInput_ReturnsFeedId()
        {
            // Arrange
            const long feedEventId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()))
                .ReturnsAsync(feedEventId);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.UpdateLastEventRaisedDateTime(feedEventId, DateTime.UtcNow);

            // Assert
            Assert.Equal(response, feedEventId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()), Times.Once);
        }

        [Fact]
        public void UpdateLastEventRaisedDateTimeFeedEventOperations_WhenNoFeedIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.UpdateLastEventRaisedDateTime(123, DateTime.UtcNow));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()), Times.Once);
        }

        [Fact]
        public void UpdateLastEventRaisedDateTimeFeedEventOperations_WhenInvalidInputFeedEventId_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.UpdateLastEventRaisedDateTime(0, DateTime.UtcNow));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<FeedEventSaveModel>()), Times.Never);
        }
        #endregion

        #region GetFeedEvent
        [Fact]
        public void GetFeedEventByIdRequestWithNullInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetFeedEventById(0));
        }

        [Fact]
        public void GetFeedEventBySourceEntityGuidRequestWithEmptyGuidInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetFeedEventBySourceEntityGuid(Guid.Empty));
        }

        [Fact]
        public void GetFeedEventByKondoEntityGuidRequestWithEmptyGuidInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetFeedEventByKondoEntityGuid(Guid.Empty));
        }

        [Fact]
        public async Task GetFeedEventById_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var id = new Random().Next();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<FeedEvent, FeedEventRequestModel>(It.IsAny<string>(), It.IsAny<FeedEventRequestModel>()))
                .ReturnsAsync(new List<FeedEvent>
                {new FeedEvent {Id = id,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = Guid.NewGuid(),
                    KondoEntityGuid = Guid.NewGuid(),
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetFeedEventById(id);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Id == id);
        }

        [Fact]
        public async Task GetFeedEventByKondoEntityGuid_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var kondoEntityGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<FeedEvent, FeedEventRequestModel>(It.IsAny<string>(), It.IsAny<FeedEventRequestModel>()))
                .ReturnsAsync(new List<FeedEvent>
                {new FeedEvent {Id = 1,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = Guid.NewGuid(),
                    KondoEntityGuid = kondoEntityGuid,
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetFeedEventByKondoEntityGuid(kondoEntityGuid);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.KondoEntityGuid == kondoEntityGuid);
        }

        [Fact]
        public async Task GetFeedEventBySourceEntityGuid_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var sourceEntityGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<FeedEvent, FeedEventRequestModel>(It.IsAny<string>(), It.IsAny<FeedEventRequestModel>()))
                .ReturnsAsync(new List<FeedEvent>
                {new FeedEvent {Id = 1,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = sourceEntityGuid,
                    KondoEntityGuid = Guid.NewGuid(),
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetFeedEventByKondoEntityGuid(sourceEntityGuid);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.SourceEntityGuid == sourceEntityGuid);
        }
        #endregion

        #region DeleteFeedEvent
        [Fact]
        public async void DeleteFeedEventOperations_WithCorrectInputFeedEventId_ReturnsFeedId()
        {
            // Arrange
            const long feedId = 1;
            var kondoGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(feedId);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.DeleteFeedEvent(kondoGuid);

            // Assert
            Assert.Equal(response, feedId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteFeedEventOperations_WhenNoFeedEventIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteFeedEvent(Guid.NewGuid()));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteFeedEventOperations_WhenEmptyKondoGuidIsPassedAsInput_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteFeedEvent(Guid.Empty));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        #endregion

        #region SaveExternalFeedEvent
        [Fact]
        public async void SaveExternalFeedEventWithCorrectInputsReturnsFeedId()
        {
            // Arrange
            const long id = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()))
                .ReturnsAsync(id);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var feedEvent = new ExternalFeedEventSaveModel
            {
                Id = id,
                EventType = "Competition.Created",
                FeedType = FeedType.Competition.ToString(),
                SourceSystem = "PlayHQ",
                SourceEntityGuid = Guid.NewGuid(),
                DestinationEntityGuid = Guid.NewGuid(),
                LastEventRaisedDateTime = DateTime.UtcNow
            };
            var response = await db.SaveExternalFeedEvent(feedEvent);

            // Assert
            Assert.Equal(response, id);
        }

        [Fact]
        public void SaveExternalFeedEventWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var feedEvent = new ExternalFeedEventSaveModel
            {
                Id = new Random().Next(),
                EventType = "Competition.Created",
                FeedType = FeedType.Competition.ToString(),
                SourceSystem = "PlayHQ",
                SourceEntityGuid = Guid.NewGuid(),
                DestinationEntityGuid = Guid.NewGuid(),
                LastEventRaisedDateTime = DateTime.UtcNow
            };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveExternalFeedEvent(feedEvent));
        }

        [Fact]
        public void SaveExternalFeedEvent_WhenRequestNull_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.SaveExternalFeedEvent(null));
        }
        #endregion

        #region UpdateExternalFeedLastEventRaisedDateTime
        [Fact]
        public async void UpdateExternalFeedLastEventRaisedDateTimeFeedEventOperations_WithCorrectInput_ReturnsFeedId()
        {
            // Arrange
            const long feedEventId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()))
                .ReturnsAsync(feedEventId);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.UpdateExternalFeedLastEventRaisedDateTime(feedEventId, DateTime.UtcNow);

            // Assert
            Assert.Equal(response, feedEventId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()), Times.Once);
        }

        [Fact]
        public void UpdateExternalFeedLastEventRaisedDateTimeFeedEventOperations_WhenNoFeedIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.UpdateExternalFeedLastEventRaisedDateTime(123, DateTime.UtcNow));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()), Times.Once);
        }

        [Fact]
        public void UpdateExternalFeedLastEventRaisedDateTimeFeedEventOperations_WhenInvalidInputFeedEventId_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.UpdateExternalFeedLastEventRaisedDateTime(0, DateTime.UtcNow));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }
        #endregion

        #region GetExternalFeedEvent
        [Fact]
        public void GetExternalFeedEventByIdRequestWithNullInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetExternalFeedEventById(0));
        }

        [Fact]
        public void GetExternalFeedEventBySourceEntityGuidRequestWithEmptyGuidInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetExternalFeedEventBySourceEntityGuid(Guid.Empty));
        }

        [Fact]
        public void GetExternalFeedEventBydestinationEntityGuidRequestWithEmptyGuidInput_ThrowsArgumentException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetExternalFeedEventByDestinationEntityGuid(Guid.Empty));
        }

        [Fact]
        public async Task GetExternalFeedEventById_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var id = new Random().Next();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ExternalFeedEvent, ExternalFeedEventRequestModel>(It.IsAny<string>(), It.IsAny<ExternalFeedEventRequestModel>()))
                .ReturnsAsync(new List<ExternalFeedEvent>
                {new ExternalFeedEvent {Id = id,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = Guid.NewGuid(),
                    DestinationEntityGuid = Guid.NewGuid(),
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetExternalFeedEventById(id);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Id == id);
        }

        [Fact]
        public async Task GetExternalFeedEventByKondoEntityGuid_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var kondoEntityGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ExternalFeedEvent, ExternalFeedEventRequestModel>(It.IsAny<string>(), It.IsAny<ExternalFeedEventRequestModel>()))
                .ReturnsAsync(new List<ExternalFeedEvent>
                {new ExternalFeedEvent {Id = 1,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = Guid.NewGuid(),
                    DestinationEntityGuid = kondoEntityGuid,
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetExternalFeedEventByDestinationEntityGuid(kondoEntityGuid);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.DestinationEntityGuid == kondoEntityGuid);
        }

        [Fact]
        public async Task GetExternalFeedEventBySourceEntityGuid_WithValidInput_ReturnsFeedEvent()
        {
            // Arrange
            var sourceEntityGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<ExternalFeedEvent, ExternalFeedEventRequestModel>(It.IsAny<string>(), It.IsAny<ExternalFeedEventRequestModel>()))
                .ReturnsAsync(new List<ExternalFeedEvent>
                {new ExternalFeedEvent {Id = 1,  EventType = "Competition.Created",
                    FeedType = FeedType.Competition.ToString(),
                    SourceSystem = "PlayHQ",
                    SourceEntityGuid = sourceEntityGuid,
                    DestinationEntityGuid = Guid.NewGuid(),
                    LastEventRaisedDateTime = DateTime.UtcNow}});
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.GetExternalFeedEventByDestinationEntityGuid(sourceEntityGuid);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.SourceEntityGuid == sourceEntityGuid);
        }
        #endregion

        #region DeleteExternalFeedEvent
        [Fact]
        public async void DeleteExternalFeedEventOperations_WithCorrectInputFeedEventId_ReturnsFeedId()
        {
            // Arrange
            const long feedId = 1;
            var kondoGuid = Guid.NewGuid();
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(feedId);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act
            var response = await db.DeleteExternalFeedEvent(kondoGuid);

            // Assert
            Assert.Equal(response, feedId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteExternalFeedEventOperations_WhenNoFeedEventIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteExternalFeedEvent(Guid.NewGuid()));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteExternalFeedEventOperations_WhenEmptyKondoGuidIsPassedAsInput_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedEventOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.DeleteExternalFeedEvent(Guid.Empty));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        #endregion
    }
}
