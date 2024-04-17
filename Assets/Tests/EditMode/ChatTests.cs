using NUnit.Framework;
using UnityEngine;
using Moq;
using System.IO;
using System.Reflection;
using TMPro;

public class ChatTests
{
    private GameObject gameObject;
    private Chat chat;
    private Mock<ICustomFileSystem> mockFileSystem;

    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        chat = gameObject.AddComponent<Chat>();
        mockFileSystem = new Mock<ICustomFileSystem>();

        // Mocking file system interactions
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
        
        chat.FileSystem = mockFileSystem.Object;
        chat.chatText = new GameObject().AddComponent<TextMeshProUGUI>();
    }

    [Test]
    public void SaveChatLog_CreatesCorrectFilePath()
    {
        // Arrange
        string expectedDirectory = Application.dataPath + "/ChatLogs";
        string expectedFileName = "ChatLog_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        string expectedPath = Path.Combine(expectedDirectory, expectedFileName);

        // Set up mock responses and expectations
        mockFileSystem.Setup(x => x.DirectoryExists(expectedDirectory)).Returns(false);
        mockFileSystem.Setup(x => x.CreateDirectory(expectedDirectory)).Verifiable();
        mockFileSystem.Setup(x => x.WriteAllText(expectedPath, It.IsAny<string>())).Verifiable();

        // Act
        chat.SaveChatLog();

        // Assert
        mockFileSystem.Verify(fs => fs.CreateDirectory(expectedDirectory), Times.Once());
        mockFileSystem.Verify(fs => fs.WriteAllText(expectedPath, It.IsAny<string>()), Times.Once());
    }

    [TearDown]
    public void Teardown()
    {
        GameObject.DestroyImmediate(gameObject);
    }
}