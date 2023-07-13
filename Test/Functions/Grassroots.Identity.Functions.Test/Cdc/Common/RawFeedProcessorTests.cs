using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Cdc.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Common
{
    public class RawFeedProcessorTests
    {
        private readonly Mock<IFeedOperations> _mockFeedOperations;
        private readonly IRawFeedProcessor _rawFeedProcessor;

        public RawFeedProcessorTests()
        {
            _mockFeedOperations = new Mock<IFeedOperations>();
            _rawFeedProcessor = new RawFeedProcessor(_mockFeedOperations.Object);
        }

        [Fact]
        public async Task InsertRawFeed_WithValidInput_ReturnsId()
        {
            //Arrange
            const FeedType feedType = new FeedType();
            const string messageId = "messageId";
            const string blobId = "blobId";
            const string category = "category";
            const int expectedResult = 123;
            _mockFeedOperations.Setup(x => x.SaveRawFeed(It.IsAny<RawFeedSaveRequestModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var actualResult = await _rawFeedProcessor.InsertRawFeed(feedType, messageId, blobId, category, System.DateTime.UtcNow);

            // Assert
            Assert.NotNull(actualResult);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task SetRawFeedStatusToSuccess_WithValidInput_ReturnsId()
        {
            //Arrange
            const ProcessStatus processStatus = new ProcessStatus();
            const int rawFeedId = 1;
            const int expectedResult = 123;
            _mockFeedOperations.Setup(x => x.SetProcessStatusToSuccess(It.IsAny<long>()))
                .ReturnsAsync(expectedResult);

            // Act
            var actualResult = await _rawFeedProcessor.SetRawFeedStatusToSuccess(rawFeedId);

            // Assert
            Assert.NotNull(actualResult);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task CheckRawFeedExists_WithValidInput_ReturnsTrue()
        {
            //Arrange
            const string messageId = "1";
            _mockFeedOperations.Setup(x => x.GetRawFeedByMessageID(It.IsAny<string>()))
                .ReturnsAsync(new List<RawFeed> { new RawFeed { BlobId = "1" } });

            // Act
            var actualResult = await _rawFeedProcessor.CheckRawFeedExists(messageId);

            // Assert
            Assert.NotNull(actualResult);
            Assert.True(actualResult);
        }
    }
}
