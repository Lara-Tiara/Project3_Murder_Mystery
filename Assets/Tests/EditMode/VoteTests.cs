using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoteTests
{
    [Test]
    public void CombineStoryText_CombinesTextCorrectly()
    {
        // Arrange
        var vote = new GameObject().AddComponent<Vote>();
        var storyNodes = new List<StoryNode>
        {
            new StoryNode { storyText = new TextAsset("Hello, ") },
            new StoryNode { storyText = new TextAsset("world!") }
        };

        // Act
        string result = vote.CombineStoryText(storyNodes);

        // Assert
        Assert.AreEqual("Hello, \n\nworld!\n\n", result);
    }
}


