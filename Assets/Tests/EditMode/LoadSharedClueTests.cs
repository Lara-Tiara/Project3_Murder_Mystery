using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Moq;
using TMPro;
using System.Reflection;

public class LoadSharedClueTests
{
    private GameObject gameObject;
    private LoadSharedClue loadSharedClue;
    private GameObject gridLayoutGameObject;
    private Transform gridLayoutTransform;

    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        loadSharedClue = gameObject.AddComponent<LoadSharedClue>();

        gridLayoutGameObject = new GameObject("GridLayout");
        gridLayoutTransform = gridLayoutGameObject.transform;
        loadSharedClue.gridLayout = gridLayoutTransform;

        var mockCluesManager = new Mock<ICluesManager>();
        mockCluesManager.Setup(x => x.GetSharedClues()).Returns(new List<StoryClue>
        {
            new StoryClue { clueText = "Clue 1" },
            new StoryClue { clueText = "Clue 2" }
        });

        loadSharedClue.SetCluesManager(mockCluesManager.Object);

        GameObject clueButtonPrefab = new GameObject("ClueButtonPrefab");
        clueButtonPrefab.AddComponent<TextMeshProUGUI>();
        loadSharedClue.clueButtonPrefab = clueButtonPrefab;
    }

    [Test]
    public void SharedCluesUpdate_CreatesButtonsForEachClue()
    {
        // Act: Trigger the update function
        loadSharedClue.SharedCluesUpdate(CluesManager.Instance.GetSharedClues());

        // Assert: Check if two buttons are created for two clues
        int childCount = gridLayoutTransform.childCount;
        Assert.AreEqual(2, childCount);
        Assert.AreEqual("Clue 1", gridLayoutTransform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text);
        Assert.AreEqual("Clue 2", gridLayoutTransform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text);
    }

    [TearDown]
    public void Teardown()
    {
        // Cleanup GameObjects to avoid memory leak and cluttering the test environment
        GameObject.DestroyImmediate(gameObject);
        GameObject.DestroyImmediate(gridLayoutGameObject);
    }
}