using UnityEngine;
using Photon.Pun;

public class NoteDataManager : PersistentSingleton<NoteDataManager>
{
    private string GetPlayerPrefKey()
    {
        string playerId = PhotonNetwork.LocalPlayer.UserId;
        return $"Note_{playerId}";
    }

    public void SaveNote(string newNote)
    {
        string key = GetPlayerPrefKey();
        PlayerPrefs.SetString(key, newNote);
        PlayerPrefs.Save();
    }

    public string LoadNote()
    {
        string key = GetPlayerPrefKey();
        return PlayerPrefs.GetString(key, "");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnGameEnd()
    {
        PlayerPrefs.DeleteAll();
    }
}
