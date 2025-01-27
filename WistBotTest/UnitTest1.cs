namespace WistBotTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var botMock = new Mock<ITelegramBotClient>();
            var localizationMock = new Mock<LocalizationService>();
            var message = new Message
            {
                Chat = new Chat { Id = 1 },
                From = new User { Id = 1 }
            };
            var token = new CancellationToken();
            localizationMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("Help message");
            botMock.Setup(x => x.SendMessage(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message());
            var helpAction = new HelpAction(botMock.Object, localizationMock.Object);
            // Act
            helpAction.ExecuteMessage(message, token).Wait();
            // Assert
            botMock.Verify(x => x.SendMessage(1, "Help message", It.IsAny<ReplyKeyboardRemove>(), token), Times.Once);

        }
    }
}