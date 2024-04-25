using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryNode", menuName = "Story/StoryNode")]
public class StoryNode : ScriptableObject
{
    public string characterName;
    public int nodeId;
    public TextAsset storyText;
    public List<StoryClue> storyClues;
    public bool isActive = false;
}


[System.Serializable]
public class StoryClue
{
    public string storyClueText;
}

