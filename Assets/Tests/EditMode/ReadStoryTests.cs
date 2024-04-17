using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ReadStoryTests
{
    [Test]
    public void CombineStoryText_CombinesTextCorrectly()
    {
        // Arrange
        var readStory = new GameObject().AddComponent<ReadStory>();
        var text1 = "Hello, ";
        var text2 = "world!";
        var storyNodes = new List<StoryNode>
        {
            new StoryNode { storyText = CreateTextAsset(text1) },
            new StoryNode { storyText = CreateTextAsset(text2) }
        };

        // Act
        string result = readStory.CombineStoryText(storyNodes);

        // Assert
        Assert.AreEqual("Hello, \n\nworld!\n\n", result);
    }

    private TextAsset CreateTextAsset(string content)
    {
        return new TextAsset(content);
    }
}
