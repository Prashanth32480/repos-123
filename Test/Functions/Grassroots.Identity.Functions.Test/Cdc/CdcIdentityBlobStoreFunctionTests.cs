using System;
using System.Threading.Tasks;
using Grassroots.Common.BlobStorage.ServiceInterface;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.Cdc
{
    public class CdcIdentityBlobStoreFunctionTests
    {
        private readonly Mock<IStoreFeed> _storeFeedMock = new Mock<IStoreFeed>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly EventGridEvent _eventGridEvent;

        public CdcIdentityBlobStoreFunctionTests()
        {
            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection))
                .Returns(() => "Sample Connection String");

            _configurationMock.Setup(x => x.GetValue(Grassroots.Identity.Functions.Cdc.AppSettingsKey.IdentityCdcFeedsContainer))
                .Returns(() => "Sample Container Name");

            _eventGridEvent = new EventGridEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "Identity.Account.BatchedEvents",
                Data = "{\"events\":[{\"type\":\"accountLoggedIn\",\"id\":\"af54e8a3-XXX-XXX-XXX-XXX\",\"timestamp\":1587917550,\"callId\":\"0fc9XXX\",\"version\":\"2.0\",\"apiKey\":\"4_PBxxxZ-Q\",\"data\":{\"accountType\":\"full\",\"uid\":\"33cxxx797\"}},{\"type\":\"accountRegistered\",\"id\":\"5c7464e2-XXX-XXX-XXX-XXX\",\"timestamp\":1587917550,\"callId\":\"0fc9XXX\",\"version\":\"2.0\",\"apiKey\":\"4_PBxxxZ-Q\",\"data\":{\"accountType\":\"full\",\"uid\":\"33cxxx797\"}}],\"nonce\":\"c77919b1-XXX-XXX-XXX-XXX\",\"timestamp\":1587917553}"
            };
        }

        [Fact]
        public async Task BlobStorageFunctionLogsProperExceptionsIfThereIsAnErrorSavingTheFeed()
        {
            // Arrange
            const string errorMessage = "Some Random error that occurred while trying to save the feed";
            _storeFeedMock.Setup(x => x.SaveFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException(errorMessage));

            // Act
            var blobStore = new CdcIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object);

            await blobStore.Run(_eventGridEvent);

            // Assert
            _storeFeedMock.Verify(x => x.SaveFeed(_configurationMock.Object.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection), It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Once);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<InvalidOperationException>(), null, null), Times.Once);
        }

        [Fact]
        public async Task BlobStorageFunctionCallsSaveFeedWithProperValuesWhenThereAreNoErrors()
        {
            // Arrange

            // Act
            var blobStore = new CdcIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object);

            await blobStore.Run(_eventGridEvent);

            // Assert
            _storeFeedMock.Verify(x => x.SaveFeed(_configurationMock.Object.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection), It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Once);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Never);
        }
        [Fact]
        public async Task CdcIdentityBlobStoreFunction_WithInvalidIdentityFeedContainer_ThrowsError()
        {
            //Arrange
            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection))
                .Returns(() => null);
            var blobStore = new CdcIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object);

            // Act
            await blobStore.Run(_eventGridEvent);

            //Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection),
                    It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }

        [Fact]
        public async Task CdcIdentityBlobStoreFunction_WithInvalidIdentityRawFeedStorageConnection_ThrowsError()
        {
            //Arrange
            _configurationMock.Setup(x => x.GetValue(Grassroots.Identity.Functions.Cdc.AppSettingsKey.IdentityCdcFeedsContainer))
                .Returns(() => null);
            var blobStore = new CdcIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object);

            // Act
            await blobStore.Run(_eventGridEvent);

            //Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(Grassroots.Identity.Functions.Cdc.AppSettingsKey.IdentityCdcFeedsContainer),
                    It.IsAny<string>(), _eventGridEvent.Data.ToString(), _eventGridEvent.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }

        [Fact]
        public async Task CdcIdentityBlobStoreFunction_WithInvalidGridEventData_ThrowsError()
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
            var blobStore = new CdcIdentityBlobStoreFunction(_storeFeedMock.Object,
                _telemetryHandlerMock.Object, _configurationMock.Object);

            // Act
            await blobStore.Run(eventGrid);

            // Assert
            _storeFeedMock.Verify(
                x => x.SaveFeed(
                    _configurationMock.Object.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection),
                    It.IsAny<string>(), eventGrid.Data.ToString(), eventGrid.Id), Times.Never);
            _telemetryHandlerMock.Verify(x => x.TrackException(It.IsAny<Exception>(), null, null), Times.Once);
        }
    }
}