using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System;

public class CluesManager : PersistentSingleton<CluesManager>, ICluesManager
{
    private const string SHARED_CLUES_KEY = "SharedClues";

    public void AddSharedClue(StoryClue clue)
    {
        List<StoryClue> clues = GetSharedClues();
        if (!clues.Any(c => c.clueText == clue.clueText))
        {
            clues.Add(clue);
            string json = JsonUtility.ToJson(new SerializableCluesList() { Clues = clues });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { SHARED_CLUES_KEY, json } });
        }
    }

    public List<StoryClue> GetSharedClues()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SHARED_CLUES_KEY, out object value))
        {
            string json = value as string;
            if (!string.IsNullOrEmpty(json))
            {
                SerializableCluesList cluesList = JsonUtility.FromJson<SerializableCluesList>(json);
                return cluesList.Clues;
            }
        }
        return new List<StoryClue>();
    }

    [Serializable]
    private class SerializableCluesList
    {
        public List<StoryClue> Clues;
    }
}
