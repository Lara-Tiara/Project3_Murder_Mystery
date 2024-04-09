using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StoryManager : PersistentSingleton<StoryManager>
{
    //public List<StoryNode> maxStoryNodes;
    //public List<StoryNode> rachelStoryNodes;
    //public List<StoryNode> chloeStoryNodes;

    private Dictionary<int, List<StoryNode>> characterStoryNodeMap;

    protected override void Awake()
    {

    }

    public void RemoveReadStoryNodes()
    {
        int selectedCharacterIndex = GameDataManager.selectCharacter;

        if (characterStoryNodeMap.TryGetValue(selectedCharacterIndex, out List<StoryNode> storyNodes))
        {
            // Remove all nodes that have been marked as read
            storyNodes.RemoveAll(node => node.hasBeenRead);

            // If you're using Photon and need to synchronize this change across clients,
            // consider using an RPC to call this method on all clients
        }
        else
        {
            Debug.LogError("Character story nodes not found for index: " + selectedCharacterIndex);
        }
    }
}
