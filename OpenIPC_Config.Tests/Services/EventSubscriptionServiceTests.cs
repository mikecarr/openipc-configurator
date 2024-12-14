using Moq;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.Tests.Services;

public class TestEvent : PubSubEvent<string>
{
}

[TestFixture]
public class EventSubscriptionServiceTests
{
    private Mock<IEventAggregator> _mockEventAggregator;
    private Mock<ILogger> _mockLogger;
    private EventSubscriptionService _eventSubscriptionService;
    private TestEvent _testEvent;

    [SetUp]
    public void SetUp()
    {
        _mockEventAggregator = new Mock<IEventAggregator>();
        _mockLogger = new Mock<ILogger>();
        _testEvent = new TestEvent();

        _mockEventAggregator
            .Setup(ea => ea.GetEvent<TestEvent>())
            .Returns(_testEvent);

        _eventSubscriptionService = new EventSubscriptionService(_mockEventAggregator.Object, _mockLogger.Object);
    }

    [Test]
    public void Subscribe_InvokesActionWhenEventIsPublished()
    {
        // Arrange
        string receivedPayload = null;
        _eventSubscriptionService.Subscribe<TestEvent, string>(payload => receivedPayload = payload);

        // Act
        _testEvent.Publish("Test Payload");

        // Assert
        Assert.AreEqual("Test Payload", receivedPayload);
        _mockLogger.Verify(logger => logger.Verbose(It.Is<string>(msg => msg.Contains("Subscribed to event TestEvent"))),
            Times.Once);
    }

    [Test]
    public void Publish_TriggersSubscribers()
    {
        // Arrange
        string receivedPayload = null;
        _testEvent.Subscribe(payload => receivedPayload = payload);

        // Act
        _eventSubscriptionService.Publish<TestEvent, string>("Another Test Payload");

        // Assert
        Assert.AreEqual("Another Test Payload", receivedPayload);
        _mockLogger.Verify(
            logger => logger.Verbose(It.Is<string>(msg =>
                msg.Contains("Published event TestEvent with payload Another Test Payload"))), Times.Once);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_IfEventAggregatorIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EventSubscriptionService(null, _mockLogger.Object));
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_IfLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EventSubscriptionService(_mockEventAggregator.Object, null));
    }
}