using UnityEngine;

public class SaveLoadManger : MonoBehaviour
{
    public static SaveLoadManger Instance;
    static string _gameFrameKey = "game_frame";

    void Awake()
    {
        Instance = this;
    }
    public void Save(_GameFrame gameFrame)
    {
        string jsonDataToSave = JsonUtility.ToJson(gameFrame);
        PlayerPrefs.SetString(_gameFrameKey, jsonDataToSave);
        PlayerPrefs.Save();
    }

    public bool CanLoad()
    {
        return PlayerPrefs.HasKey(_gameFrameKey);
    }

    public _GameFrame Load()
    {
        _GameFrame loadedGameFrame = null;
        if (CanLoad())
        {
            string jsonGameFrame = PlayerPrefs.GetString(_gameFrameKey);
            loadedGameFrame = JsonUtility.FromJson<_GameFrame>(jsonGameFrame);
        }
        return loadedGameFrame;
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteKey(_gameFrameKey);
        PlayerPrefs.Save();
    }
}
