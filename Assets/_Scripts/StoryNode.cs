using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryNode", menuName = "Story/StoryNode")]
public class StoryNode : ScriptableObject
{
    public string characterName;
    public int nodeId;
    public TextAsset storyText;
    //public List<StoryChoice> choices;
    public bool isActive = false;
    public bool hasBeenRead = false; // Flag to track if the node has been read
}

/*
[System.Serializable]
public class StoryChoice
{
    public string choiceText; // Text displayed for the choice
    public bool choiceSelected;
    public StoryNode nextNode; // The next StoryNode if this choice is selected
}

[System.Serializable]
public class StoryCondition
{
    public StoryNode requiredNode; // The node that must be read before this condition is met
    // You might want to add a flag to indicate whether the condition has been met (if it's more complex than just checking a single node)
}
*/
