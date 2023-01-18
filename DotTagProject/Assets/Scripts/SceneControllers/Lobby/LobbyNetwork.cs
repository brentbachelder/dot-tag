using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

public class LobbyNetwork : MonoBehaviour
{
    GameManagerController GameManager;
    
    DatabaseReference _lobby;
    public Dictionary<string, GameSettings> Games = new Dictionary<string, GameSettings>();
    [SerializeField] LobbyController LobbyController;

    [HideInInspector] public int lastKey;

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();
        _lobby = FirebaseDatabase.DefaultInstance.GetReference("Lobby");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if(task.Exception != null)
            {
                GameManager.Error.SetErrorMessage("Couldn't connect to database");
                return;
            }
            InitializeFirebase();
        });
    }

    void InitializeFirebase()
    {
        _lobby.ValueChanged += LobbyChanged;
    }

    void OnDisable()
    {
        if(_lobby != null) { _lobby.ValueChanged -= LobbyChanged; _lobby = null; }
    }

    void LobbyChanged(object sender, ValueChangedEventArgs args)
    {
        Games.Clear();
        DataSnapshot snapshot = args.Snapshot;

        foreach(var child in snapshot.Children)
        {
            GameSettings info = JsonUtility.FromJson<GameSettings>(snapshot.Child(child.Key).GetRawJsonValue());
            Games.Add(child.Key, info);
            lastKey = int.Parse(child.Key);
        }
        lastKey += 1;
        // This is the buttons for network disconnect. Do something.
        if(LobbyController != null && !GameManager.networkDisconnected) LobbyController.UpdateOpenGames(Games);
    }


    public async void CreateGameOut(string jsonGame, string jsonGPM, string gameID)
    {
        string gameKey = lastKey.ToString();

        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(gameID).Child("Settings").SetRawJsonValueAsync(jsonGame).ContinueWith(task => {
            if(!task.IsCompleted) { GameManager.Error.SetErrorMessage("Couldn't create game (settings)"); }
        });

        await _lobby.Child(lastKey.ToString()).SetRawJsonValueAsync(jsonGame).ContinueWith(task2 => {
            if(!task2.IsCompleted) { GameManager.Error.SetErrorMessage("Couldn't create game (lobby)"); }
        });

        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(gameID).Child("GameManager").SetRawJsonValueAsync(jsonGPM).ContinueWith(task => {
            if(!task.IsCompleted) { GameManager.Error.SetErrorMessage("Couldn't create game (game manager)"); }
        });

        JoinGame(gameKey);
    }

    public async void JoinGame(string lobbyNumber)
    {
        PlayerPrefs.SetString("UserNumber", Games[lobbyNumber].NextInLine.ToString());
        PlayerPrefs.SetString("LobbyNumber", lobbyNumber);
        PlayerPrefs.SetString("GameID", Games[lobbyNumber].GameID);

        GameSettings newSettings = new GameSettings();
        if(Games.ContainsKey(lobbyNumber)) newSettings = Games[lobbyNumber];
        else Debug.Log("Something is wrong with game count. Looking for " + lobbyNumber);

        newSettings.CurrentPlayers += 1;
        newSettings.NextInLine += 1;
        int myColor = Random.Range(0, 7);
        GamePlayerStatic stat = new GamePlayerStatic();
        stat.ID = PlayerPrefs.GetString("UserName");

        if(PlayerPrefs.HasKey("UserColor")) myColor = PlayerPrefs.GetInt("UserColor");
        else PlayerPrefs.SetInt("UserColor", myColor);
        stat.Color = myColor;

        string jsonStat = JsonUtility.ToJson(stat);
        string jsonSettings = JsonUtility.ToJson(newSettings);

        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(Games[lobbyNumber].GameID).Child("Static").Child(PlayerPrefs.GetString("UserNumber")).SetRawJsonValueAsync(jsonStat).ContinueWith(task => {
            if(!task.IsCompleted) GameManager.Error.SetErrorMessage("Couldn't add player static to game");
        });

        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(Games[lobbyNumber].GameID).Child("Settings").Child("CurrentPlayers").SetValueAsync(newSettings.CurrentPlayers);
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(Games[lobbyNumber].GameID).Child("Settings").Child("NextInLine").SetValueAsync(newSettings.NextInLine);
        await _lobby.Child(lobbyNumber).Child("CurrentPlayers").SetValueAsync(newSettings.CurrentPlayers);
        await _lobby.Child(lobbyNumber).Child("NextInLine").SetValueAsync(newSettings.NextInLine);

        GameManager.managerEnabled = true;
        SceneManager.LoadScene("Setup");
    }

    public void LobbyGoBack()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
