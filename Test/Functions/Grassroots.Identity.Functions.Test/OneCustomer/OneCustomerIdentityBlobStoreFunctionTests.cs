using System;
using System.Threading.Tasks;
using Grassroots.Common.BlobStorage.ServiceInterface;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.External.OneCustomer;
using Grassroots.Identity.Functions.PlayHQ;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.OneCustomer
{
    public class OneCustomerIdentityBlobStoreFunctionTests
    {
        private readonly Mock<IStoreFeed> _storeFeedMock = new Mock<IStoreFeed>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly EventGridEvent _eventGridEvent;
        private readonly Mock<IFeatureFlag> _featureFlagMock = new Mock<IFeatureFlag>();

        public OneCustomerIdentityBlobStoreFunctionTests()
        {
            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection))
                .Returns(() => "Sample Connection String");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.IdentityOneCustomerFeedsContainer))
                .Returns(() => "Sample Container Name");

            _eventGridEvent = new EventGridEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "ONECUSTOMER.ACCOUNT.UPDATED",
                Data =
                    "{\"messageId\":\"Test Message ID\",\"entityId\":\"Test Entity ID\",\"eventRaisedDateTime\":\"Test Date Time\",\"eventType\":\"INSIDER.ACCOUNT.UPDATED\",\"data\":{\"id\":\"Test Function\",\"cricketId\":\"310bee0c2b1041e981738066fc3eeea9\"}}"
            };
        }

        [Fact]
        public async Task BlobStorageFunctionLogsProperExceptionsIfThereIsAnErrorSavingTheFeed()
        {
            // Arrange
            const string errorMessage = "Some Random error that occurred while trying to save the feed";
            _storeFeedMock.Setup(x => x.SaveFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException(errorMessage));

            _featureFlagMock.Setup(x => x.FlagEnabled(It.IsAny<string>())).Returns(true);

            // Act
            var blobStore = new OneCustomerIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object, _featureFlagMock.Object);
            
            await blobStore.Run(_eventGridEvent);

            // Assert
            _storeFeedMock.Verify(x => x.SaveFeed(_configurationMock.Object.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection), It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Once);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<InvalidOperationException>(), null, null), Times.Once);
        }

        [Fact]
        public async Task BlobStorageFunctionCallsSaveFeedWithProperValuesWhenThereAreNoErrors()
        {
            // Arrange
            _featureFlagMock.Setup(x => x.FlagEnabled(It.IsAny<string>())).Returns(true);
            // Act
            var blobStore = new OneCustomerIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object, _featureFlagMock.Object);

            await blobStore.Run(_eventGridEvent);

            // Assert
            _storeFeedMock.Verify(x => x.SaveFeed(_configurationMock.Object.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection), It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Once);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Never);
        }
        [Fact]
        public async Task OneCustomerIdentityBlobStoreFunction_WithInvalidIdentityFeedContainer_ThrowsError()
        {
            //Arrange
            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection))
                .Returns(() => null);

            _featureFlagMock.Setup(x => x.FlagEnabled(It.IsAny<string>())).Returns(true);

            var blobStore = new OneCustomerIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object, _featureFlagMock.Object);


            // Act
            await blobStore.Run(_eventGridEvent);

            //Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection),
                    It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }

        [Fact]
        public async Task OneCustomerIdentityBlobStoreFunction_WithInvalidIdentityRawFeedStorageConnection_ThrowsError()
        {
            //Arrange
            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.IdentityOneCustomerFeedsContainer))
                .Returns(() => null);

            _featureFlagMock.Setup(x => x.FlagEnabled(It.IsAny<string>())).Returns(true);

            var blobStore = new OneCustomerIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object, _featureFlagMock.Object);

            // Act
            await blobStore.Run(_eventGridEvent);

            //Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(External.AppSettingsKey.IdentityOneCustomerFeedsContainer),
                    It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }

        [Fact]
        public async Task OneCustomerIdentityBlobStoreFunction_WithInvalidGridEventData_ThrowsError()
        {
            //Arrange
            var eventGrid = new EventGridEvent
            {
                Data = "",
                Id = Guid.NewGuid().ToString(),
                DataVersion = "DataVersion",
                EventTime = DateTime.UtcNow,
                EventType = "EventType",
                Subject = "Subject",
                Topic = "Topic"
            };

            _featureFlagMock.Setup(x => x.FlagEnabled(It.IsAny<string>())).Returns(true);

            var blobStore = new OneCustomerIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object, _featureFlagMock.Object);

            // Act
            await blobStore.Run(eventGrid);

            // Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(External.AppSettingsKey.IdentityRawFeedStorageConnection),
                    It.IsAny<string>(), eventGrid.Data.ToString(), eventGrid.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }
    }
}