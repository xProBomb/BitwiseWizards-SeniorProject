using Microsoft.Extensions.Logging;
using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services;
using TrustTrade.Services.Web.Implementations;

namespace TestTrustTrade
{
    [TestFixture]
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _chatRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<ILogger<ChatService>> _loggerMock;
        private ChatService _chatService;

        [SetUp]
        public void Setup()
        {
            _chatRepositoryMock = new Mock<IChatRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<ChatService>>();

            _chatService = new ChatService(
                _chatRepositoryMock.Object,
                _userRepositoryMock.Object,
                _notificationServiceMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task GetConversationAsync_ValidConversation_ReturnsConversation()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1))
                .ReturnsAsync(conversation);

            // Act
            var result = await _chatService.GetConversationAsync(1, 1);

            // Assert
            Assert.That(result, Is.EqualTo(conversation));
        }

        [Test]
        public async Task GetConversationAsync_UserNotInConversation_ReturnsNull()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1))
                .ReturnsAsync(conversation);

            // Act
            var result = await _chatService.GetConversationAsync(1, 3); // User 3 is not in the conversation

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetOrCreateConversationAsync_ExistingConversation_ReturnsExistingConversation()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1, 2))
                .ReturnsAsync(conversation);

            // Act
            var result = await _chatService.GetOrCreateConversationAsync(1, 2);

            // Assert
            Assert.That(result, Is.EqualTo(conversation));
            _chatRepositoryMock.Verify(x => x.CreateConversationAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetOrCreateConversationAsync_NewConversation_CreatesAndReturnsNewConversation()
        {
            // Arrange
            var newConversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1, 2))
                .ReturnsAsync((Conversation)null);
            _chatRepositoryMock.Setup(x => x.CreateConversationAsync(1, 2))
                .ReturnsAsync(newConversation);

            // Act
            var result = await _chatService.GetOrCreateConversationAsync(1, 2);

            // Assert
            Assert.That(result, Is.EqualTo(newConversation));
            _chatRepositoryMock.Verify(x => x.CreateConversationAsync(1, 2), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_ValidConversation_SendsMessageAndCreatesNotification()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            var sender = new User { Id = 1, Username = "sender" };

            var message = new Message
            {
                Id = 1,
                ConversationId = 1,
                SenderId = 1,
                RecipientId = 2,
                Content = "Hello",
                CreatedAt = DateTime.UtcNow
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1))
                .ReturnsAsync(conversation);
            _chatRepositoryMock.Setup(x => x.SendMessageAsync(1, 1, 2, "Hello"))
                .ReturnsAsync(message);
            _userRepositoryMock.Setup(x => x.FindByIdAsync(1))
                .ReturnsAsync(sender);

            // Act
            var result = await _chatService.SendMessageAsync(1, 1, 2, "Hello");

            // Assert
            Assert.That(result.ConversationId, Is.EqualTo(1));
            Assert.That(result.SenderId, Is.EqualTo(1));
            Assert.That(result.Content, Is.EqualTo("Hello"));
            _notificationServiceMock.Verify(x => x.CreateMessageNotificationAsync(1, 2, 1), Times.Once);
        }

        [Test]
        public async Task MarkConversationAsReadAsync_ValidConversation_MarksMessagesAsRead()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1))
                .ReturnsAsync(conversation);
            _chatRepositoryMock.Setup(x => x.MarkMessagesAsReadAsync(1, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _chatService.MarkConversationAsReadAsync(1, 1);

            // Assert
            Assert.That(result, Is.True);
            _chatRepositoryMock.Verify(x => x.MarkMessagesAsReadAsync(1, 1), Times.Once);
        }

        [Test]
        public async Task MarkConversationAsReadAsync_UserNotInConversation_ReturnsFalse()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = 1,
                User1Id = 1,
                User2Id = 2
            };

            _chatRepositoryMock.Setup(x => x.GetConversationAsync(1))
                .ReturnsAsync(conversation);

            // Act
            var result = await _chatService.MarkConversationAsReadAsync(1, 3); // User 3 is not in the conversation

            // Assert
            Assert.That(result, Is.False);
            _chatRepositoryMock.Verify(x => x.MarkMessagesAsReadAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}