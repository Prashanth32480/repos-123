using System;
using System.Collections.Generic;
using System.Linq;
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
    public class FeedOperationTests
    {
        [Fact]
        public async void SaveRawFeedWithCorrectInputsReturnsFeedId()
        {
            // Setup
            const long feedId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<RawFeedSaveRequestModel>()))
                .ReturnsAsync(feedId);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedSaveRequestModel { FeedTypeId = FeedType.Competition, MessageId = "123" };
            var response = await db.SaveRawFeed(rawFeed);

            // Assert
            Assert.Equal(response, feedId);
        }

        [Fact]
        public void SaveRawFeedWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<RawFeedsRequestModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedSaveRequestModel { FeedTypeId = FeedType.Competition, MessageId = "123" };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveRawFeed(rawFeed));
        }

        #region SetProcessStatusToSuccess
        [Fact]
        public async void SetProcessStatusToSuccessRawFeedOperations_WithCorrectInputRawFeedId_ReturnsFeedId()
        {
            // Arrange
            const long feedId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(feedId);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var response = await db.SetProcessStatusToSuccess(123);

            // Assert
            Assert.Equal(response, feedId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void SetProcessStatusToSuccessRawFeedOperations_WhenNoFeedIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SetProcessStatusToSuccess(123));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void SetProcessStatusToSuccessRawFeedOperations_WhenInvalidInputRawFeedId_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SetProcessStatusToSuccess(0));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        #endregion

        [Fact]
        public void SaveRawFeed_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.SaveRawFeed(null));
        }

        [Fact]
        public void GetRawFeed_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetRawFeedByMessageID(null));
        }

        [Fact]
        public async Task GetRawFeed_WithValidInput_ReturnRawFeeds()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<RawFeed, object>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<RawFeed>
                {new RawFeed {BlobId = "BlobId", Category = "Category", FeedId = 1, FeedTypeId = 1, MessageId = "MessageId", ProcessingDateTime = DateTime.UtcNow}});

            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedsRequestModel { MessageId = "MessageId" };
            var response = await db.GetRawFeedByMessageID(rawFeed.MessageId);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Count() == 1);
        }


        //External Feed
        [Fact]
        public async void SaveRawExternalFeedWithCorrectInputsReturnsFeedId()
        {
            // Setup
            const long feedId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<RawFeedSaveRequestModel>()))
                .ReturnsAsync(feedId);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedSaveRequestModel { FeedTypeId = FeedType.Competition, MessageId = "123" };
            var response = await db.SaveRawFeed(rawFeed);

            // Assert
            Assert.Equal(response, feedId);
        }

        [Fact]
        public void SaveRawExternalFeedWhenNoDataIsReceivedFromDatabaseThrowsException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<RawFeedsRequestModel>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedSaveRequestModel { FeedTypeId = FeedType.Competition, MessageId = "123" };

            // Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SaveRawFeed(rawFeed));
        }

        #region SetProcessStatusToSuccess
        [Fact]
        public async void SetProcessStatusToSuccessRawExternalFeedOperations_WithCorrectInputRawFeedId_ReturnsFeedId()
        {
            // Arrange
            const long feedId = 1;
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(feedId);
            var db = new FeedOperations(dbFactory.Object);

            // Act
            var response = await db.SetExternalFeedProcessStatusToSuccess(123);

            // Assert
            Assert.Equal(response, feedId);
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void SetProcessStatusToSuccessRawExternalFeedOperations_WhenNoFeedIdIsReceivedFromDatabase_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SetExternalFeedProcessStatusToSuccess(123));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void SetProcessStatusToSuccessRawExternalFeedOperations_WhenInvalidInputRawFeedId_ThrowsException()
        {
            // Arrange
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();

            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(() => null);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () => await db.SetExternalFeedProcessStatusToSuccess(0));
            dbConnection.Verify(x => x.ExecuteResultLongAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        #endregion

        [Fact]
        public void SaveExternalRawFeed_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.SaveExternalRawFeed(null));
        }

        [Fact]
        public void GetExternalRawFeed_WhenRequestNull_ThrowsArgumentException()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            var db = new FeedOperations(dbFactory.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await db.GetExternalRawFeedByMessageId(null));
        }

        [Fact]
        public async Task GetExternalRawFeed_WithValidInput_ReturnRawFeeds()
        {
            // Setup
            var dbFactory = new Mock<IDatabaseConnectionFactory>();
            var dbConnection = new Mock<IDatabaseConnection>();
            dbFactory.Setup(factory => factory.CreateConnection()).Returns(dbConnection.Object);
            dbConnection.Setup(connection => connection.ExecuteResultCollectionAsync<RawFeed, object>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<RawFeed>
                {new RawFeed {BlobId = "BlobId", Category = "Category", FeedId = 1, FeedTypeId = 1, MessageId = "MessageId", ProcessingDateTime = DateTime.UtcNow}});

            var db = new FeedOperations(dbFactory.Object);

            // Act
            var rawFeed = new RawFeedsRequestModel { MessageId = "MessageId" };
            var response = await db.GetExternalRawFeedByMessageId(rawFeed.MessageId);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Count() == 1);
        }
    }
}
