using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System;

public class CluesManager : PersistentSingleton<CluesManager>, ICluesManager
{
    private const string SHARED_CLUES_KEY = "SharedClues";
    public List<Clue> cluesCurrentRound = new List<Clue>();

    public void AddSharedClue(Clue clue)
    {
        List<Clue> clues = GetSharedClues();
        if (!clues.Any(c => c.clueText == clue.clueText))
        {
            clues.Add(clue);
            cluesCurrentRound.Add(clue);
            string json = JsonUtility.ToJson(new SerializableCluesList() { Clues = clues });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { SHARED_CLUES_KEY, json } });
        }
    }

    public int GetSharedCluesCount()
    {
        return cluesCurrentRound.Count(clue => clue.hasDestroyed == false);
    }

    public void DestroyClue(Clue clue, int minimumClues)
    {
        if (GetSharedCluesCount() <= minimumClues)
        {
            return;
        }

        List<Clue> clues = GetSharedClues();
        Clue clueInList = clues.FirstOrDefault(c => c.clueKeyWord == clue.clueKeyWord);
        if (clueInList != null)
        {
            clueInList.hasDestroyed = true;
            string json = JsonUtility.ToJson(new SerializableCluesList() { Clues = clues });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { SHARED_CLUES_KEY, json } });
        }

        clueInList = cluesCurrentRound.FirstOrDefault(c => c.clueKeyWord == clue.clueKeyWord);
        if (clueInList != null)
        {
            clueInList.hasDestroyed = true;
        }
    }

    public List<Clue> GetSharedClues()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SHARED_CLUES_KEY, out object value))
        {
            string json = value as string;
            if (!string.IsNullOrEmpty(json))
            {
                SerializableCluesList cluesList = JsonUtility.FromJson<SerializableCluesList>(json);
                return cluesList.Clues.Where(clue => clue.hasDestroyed == false).ToList();
            }
        }
        return new List<Clue>();
    }

    [Serializable]
    private class SerializableCluesList
    {
        public List<Clue> Clues;
    }
}
