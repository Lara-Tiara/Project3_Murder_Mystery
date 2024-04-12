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
}
