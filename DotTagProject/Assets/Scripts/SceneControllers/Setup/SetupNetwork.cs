using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

public class SetupNetwork : MonoBehaviour
{
    [SerializeField] SetupController SetupController;
    GameManagerController GameManager;

    float myPauseTime;

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if(task.Exception != null)
            {
                GameManager.Error.SetErrorMessage("Couldn't connect to database");
                return;
            }
        });
    }

    public void ColorUpdate()
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Static").Child(PlayerPrefs.GetString("UserNumber")).Child("Color").SetValueAsync(PlayerPrefs.GetInt("UserColor"));
    }

    public void ChangeGameType(int type)
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("GameType").SetValueAsync(type);
    }

    public void ChangeMaxPlayers(int max)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("MaxPlayers").SetValueAsync(max);
    }

    public void ChangeTimeLimit(int time)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("TimeLimit").SetValueAsync(time);
    }

    public void ChangePlayerSpeed(int speed)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("PlayerSpeed").SetValueAsync(speed);
    }

    public void ChangeCameraZoom(int zoom)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("CameraZoom").SetValueAsync(zoom);
    }

    public async void RemoveMyself()
	{
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Static").Child(PlayerPrefs.GetString("UserNumber")).RemoveValueAsync().ContinueWith(task => {
            if(task.Exception != null) GameManager.Error.SetErrorMessage("Error Removing Myself from Game");
        });

        GameManager.managerEnabled = false;
        SceneManager.LoadScene("Lobby");
    }

    public async void StartGame()
	{
        string map = GenerateMap();
        List<string> removeKeys = new List<string>();
        foreach(var key in GameManager.PlayerStatic)
        {
            if(GameManager.PlayerStatic[key.Key].Pause == 1) removeKeys.Add(key.Key);
            else
            {
                GamePlayerVariables variables = new GamePlayerVariables();
                if(GameManager.CurrentSettings.GameType == 0) variables.Lives = 2;

                string jsonVariables = JsonUtility.ToJson(variables);

                await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Variables").Child(key.Key).SetRawJsonValueAsync(jsonVariables);
            }
        }
        foreach(var key in removeKeys)
		{
            GameManager.CurrentPlayers.Remove(key);
            await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Static").Child(key).RemoveValueAsync().ContinueWith(task => {
                if(task.Exception != null) GameManager.Error.SetErrorMessage("Error Removing Player from Game");
            });
        }

        int randomNumber = Random.Range(0, GameManager.CurrentPlayers.Count);
        string randomPlayer = GameManager.CurrentPlayers[randomNumber];
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Variables").Child(randomPlayer).Child("It").SetValueAsync(1);
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Map").SetValueAsync(map);

        await FirebaseDatabase.DefaultInstance.GetReference("Lobby").Child(PlayerPrefs.GetString("LobbyNumber")).Child("Status").SetValueAsync("In Progress");
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings").Child("Status").SetValueAsync("In Progress");
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("GameManager").Child("GameMessage").SetValueAsync("Randomize|" + randomPlayer);
        PlayerPrefs.SetString("RandomSelect", randomPlayer);
    }

    string GenerateMap()
    {
        int columns = 14;
        int rows = 10;

        string[,] map = new string[columns, rows];
        int openTiles = 0;
        while(openTiles < 9)
        {
            openTiles = 0;
            for(var y = 0; y < rows; y++)
            {
                for(var x = 0; x < columns; x++)
                {
                    if(y == 0)
                    {
                        if(x == 0) map[x, y] = "00";
                        else if(x == columns - 1) map[x, y] = "01";
                        else map[x, y] = "10";
                    }
                    else if(y == rows - 1)
                    {
                        if(x == 0) map[x, y] = "03";
                        else if(x == columns - 1) map[x, y] = "02";
                        else map[x, y] = "12";
                    }
                    else
                    {
                        if(x == 0) map[x, y] = "13";
                        else if(x == columns - 1) map[x, y] = "11";
                        else
                        {
                            int randomShape = Random.Range(0, 30);
                            int randomDirection = Random.Range(0, 4);
                            if(randomShape > 5) randomShape = 6;
                            map[x, y] = randomShape.ToString() + randomDirection.ToString();
                            if(randomShape == 6 || randomShape == 5) openTiles++;
                        }
                    }
                }
            }
        }
        map[Mathf.FloorToInt(columns / 2) - 1, Mathf.FloorToInt(rows / 2) - 1] = "60";
        map[Mathf.FloorToInt(columns / 2), Mathf.FloorToInt(rows / 2) - 1] = "60";
        map[Mathf.FloorToInt(columns / 2) + 1, Mathf.FloorToInt(rows / 2) - 1] = "60";
        map[Mathf.FloorToInt(columns / 2) - 1, Mathf.FloorToInt(rows / 2)] = "60";
        map[Mathf.FloorToInt(columns / 2), Mathf.FloorToInt(rows / 2)] = "60";
        map[Mathf.FloorToInt(columns / 2) + 1, Mathf.FloorToInt(rows / 2)] = "60";
        map[Mathf.FloorToInt(columns / 2) - 1, Mathf.FloorToInt(rows / 2) + 1] = "60";
        map[Mathf.FloorToInt(columns / 2), Mathf.FloorToInt(rows / 2) + 1] = "60";
        map[Mathf.FloorToInt(columns / 2) + 1, Mathf.FloorToInt(rows / 2) + 1] = "60";

        string finalMap = "";
        for(var yy = 0; yy < rows; yy++)
        {
            for(var xx = 0; xx < columns; xx++)
            {
                finalMap += map[xx, yy];
            }
        }

        return finalMap;
    }
}
