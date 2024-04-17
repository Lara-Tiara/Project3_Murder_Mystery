using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Photon.Pun;

public class CharacterManagerTests
{
    private GameObject gameObject;
    private CharacterManager characterManager;
    private Button[] buttons;

    [SetUp]
    public void Setup()
    {
        CharacterManager.ResetClickedButtonCount();
        gameObject = new GameObject();
        characterManager = gameObject.AddComponent<CharacterManager>();
        gameObject.AddComponent<PhotonView>();

        // Create a mock of buttons array
        gameObject.AddComponent<Canvas>();
        buttons = new Button[3];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject buttonObject = new GameObject($"Button{i}");
            buttonObject.transform.SetParent(gameObject.transform);
            buttons[i] = buttonObject.AddComponent<Button>();
            buttonObject.AddComponent<Text>(); // Assume each button has a Text component for simplicity
        }
        characterManager.buttons = buttons;

        // Reset static variables
        typeof(CharacterManager).GetField("clickedButtonCount", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, 0);
    }

    [Test]
    public void OnButtonClick_ButtonBecomesNonInteractable()
    {
        // Arrange
        int buttonIndex = 1; // Simulate clicking the second button

        // Act
        characterManager.OnButtonClick(buttonIndex);

        // Assert
        Assert.IsFalse(buttons[buttonIndex].interactable, "Button should be non-interactable after being clicked.");
    }

    [Test]
    public void OnButtonClick_IncrementsClickedButtonCount()
    {
        // Arrange
        int initialCount = CharacterManager.GetClickedButtonCount();

        // Act
        characterManager.OnButtonClick(1);

        // Assert
        Assert.AreEqual(initialCount + 1, CharacterManager.GetClickedButtonCount(), "Clicked button count should be incremented.");
    }

    [TearDown]
    public void Teardown()
    {
        GameObject.DestroyImmediate(gameObject);
    }
}

