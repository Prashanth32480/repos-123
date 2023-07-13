using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.External.Common;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Grassroots.Identity.Functions.Insider.Test.Common
{
    public class FeedEventProcessorTests
    {
        private readonly Mock<IFeedEventOperations> _feedEventOperationsMock;
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock;
        private readonly IFeedEventProcessor _feedEventProcessor;

        public FeedEventProcessorTests()
        {
            _feedEventOperationsMock = new Mock<IFeedEventOperations>();
            _telemetryHandlerMock = new Mock<ITelemetryHandler>();
            _feedEventProcessor = new FeedEventProcessor(_feedEventOperationsMock.Object, _telemetryHandlerMock.Object);
        }

        [Fact]
        public async Task ForAValidDeleteEvent_ShouldFeedBeProcessed_ShouldReturnFalse()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var kondoGuid = Guid.NewGuid();
            var sourceEntityGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);

            // Assert
            
            _feedEventOperationsMock.Verify(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }

        [Fact]
        public async Task ForAValidCreateEvent_ShouldFeedBeProcessed_ShouldReturnFalse()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var kondoGuid = Guid.NewGuid();
            var sourceEntityGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);

            // Assert
            Assert.True(shouldFeedBeProcessed);

            _feedEventOperationsMock.Verify(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }

        [Fact]
        public async Task ForAValidUpdateEvent_ShouldFeedBeProcessed_ShouldReturnFalse()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var kondoGuid = Guid.NewGuid();
            var sourceEntityGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);

            // Assert
            Assert.True(shouldFeedBeProcessed);

            _feedEventOperationsMock.Verify(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }

        [Fact]
        public async Task ForAValidNonDeleteEvent_ShouldFeedBeProcessed_ShouldReturnTrue_And_UpdateEventRaisedDateTime_ShouldCallSaveFeedEvent()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var kondoGuid = Guid.NewGuid();
            var sourceEntityGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";
            var trueFalse = Convert.ToBoolean(new Random().Next(0, 1));
            if (trueFalse)
                _feedEventOperationsMock.Setup(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()))
                    .ReturnsAsync((ExternalFeedEvent)null);
            else
                _feedEventOperationsMock.Setup(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()))
                    .ReturnsAsync(new ExternalFeedEvent
                    {
                        EventType = "UPDATED",
                        FeedType = feedType.ToString(),
                        SourceSystem = sourceSystem.ToString(),
                        SourceEntityGuid = sourceEntityGuid,
                        DestinationEntityGuid = kondoGuid,
                        LastEventRaisedDateTime = eventRaisedDateTime.AddSeconds(-1)
                    });

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);
            await _feedEventProcessor.UpdateEventRaisedDateTime(kondoGuid);

            // Assert
            Assert.True(shouldFeedBeProcessed);

            _feedEventOperationsMock.Verify(x => x.DeleteExternalFeedEvent(It.IsAny<Guid>()), Times.Never);
            _feedEventOperationsMock.Verify(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Once);
        }

        [Fact]
        public async Task ForANonDeleteEvent_ShouldFeedBeProcessed_ShouldReturnFalse_IfRecordExistsInDBWithNewerDateThenInTheFeed()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var sourceEntityGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";
            _feedEventOperationsMock.Setup(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()))
                .ReturnsAsync(new ExternalFeedEvent { LastEventRaisedDateTime = eventRaisedDateTime.AddSeconds(1) });

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);

            // Assert
            Assert.False(shouldFeedBeProcessed);

            _feedEventOperationsMock.Verify(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task CallingUpdateEventRaisedDateTime_ShouldLogWarning_WhenShouldFeedBeProcessedMethodReturnsFalse()
        {
            //Arrange
            var feedType = (FeedType)new Random().Next(1, Enum.GetNames(typeof(FeedType)).Length - 1);
            var sourceSystem = (SourceSystem)new Random().Next(1, Enum.GetNames(typeof(SourceSystem)).Length - 1);
            var sourceEntityGuid = Guid.NewGuid();
            var kondoGuid = Guid.NewGuid();
            var eventRaisedDateTime = DateTime.UtcNow;
            const string eventType = "INSIDER.ACCOUNT.UPDATED";
            _feedEventOperationsMock.Setup(x => x.GetExternalFeedEventBySourceEntityGuid(It.IsAny<Guid>()))
                .ReturnsAsync(new ExternalFeedEvent { LastEventRaisedDateTime = eventRaisedDateTime.AddSeconds(1) });

            // Act
            var shouldFeedBeProcessed = await _feedEventProcessor.ShouldFeedBeProcessed(eventType, feedType, sourceSystem, sourceEntityGuid, eventRaisedDateTime);
            await _feedEventProcessor.UpdateEventRaisedDateTime(kondoGuid);
            // Assert
            Assert.False(shouldFeedBeProcessed);

            _telemetryHandlerMock.Verify(x => x.TrackTraceWarning(It.IsAny<string>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.DeleteExternalFeedEvent(It.IsAny<Guid>()), Times.Never);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }

        [Fact]
        public async Task CallingUpdateEventRaisedDateTime_ShouldLogWarning_WhenShouldFeedBeProcessedMethodIsNotCalledFirst()
        {
            //Arrange
            var kondoGuid = Guid.NewGuid();

            // Act
            await _feedEventProcessor.UpdateEventRaisedDateTime(kondoGuid);

            // Assert
            _telemetryHandlerMock.Verify(x => x.TrackTraceWarning(It.IsAny<string>()), Times.Once);
            _feedEventOperationsMock.Verify(x => x.DeleteExternalFeedEvent(It.IsAny<Guid>()), Times.Never);
            _feedEventOperationsMock.Verify(x => x.SaveExternalFeedEvent(It.IsAny<ExternalFeedEventSaveModel>()), Times.Never);
        }
    }
}
