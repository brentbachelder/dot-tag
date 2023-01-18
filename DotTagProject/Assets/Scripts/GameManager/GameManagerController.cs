using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;


public class GameManagerController : MonoBehaviour
{
    DatabaseReference _manager;
    DatabaseReference _static;
    DatabaseReference _settings;
    DatabaseReference _network;
    public Error Error;

    [HideInInspector] public Dictionary<string, GamePlayerStatic> PlayerStatic = new Dictionary<string, GamePlayerStatic>();
    [HideInInspector] public List<string> CurrentPlayers = new List<string>();
    [HideInInspector] public GamePlayerManager PlayerManager = new GamePlayerManager();
    [HideInInspector] public GameSettings CurrentSettings = new GameSettings();

    bool gameLoading = true;
    public int timeBeforeKickout = 15;
    public string Partner = "";
    [HideInInspector] public float GameStartTime;
    bool GameStartTimeInitiated = false;
    float pauseTime;
    float partnerPauseTime;
    bool partnerIsPaused = false;
    //string currentGameMessage = "";
    [HideInInspector] public bool networkDisconnected = true;
    [HideInInspector] public float networkDisconnectTime;
    [HideInInspector] public bool firebaseInitialized = false;
    [HideInInspector] public bool settingsInitialized = false;
    [HideInInspector] public bool staticInitialized = false;
    [HideInInspector] public bool managerInitialized = false;
    [HideInInspector] public bool managerEnabled = false;
    [HideInInspector] public bool settingsStaticChange = false;

    public int activePlayers = 0;
    public int totalPlayers = 0;
    public int nextInLine = 0;
    public int owner = 0;
    public int controller = 0;

    public Color[] color = new Color[8];

    void Awake()
	{
        DontDestroyOnLoad(this.gameObject);
    }

	void Start()
    {
        _network = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if(task.Exception != null)
            {
                Error.SetErrorMessage("Couldn't connect to database");
                return;
            }

            _network.ValueChanged += ConnectionChanged;
        });
    }

	void Update()
	{
        if(gameLoading)
        {
            if(!networkDisconnected)
            {
                gameLoading = false;
                SceneManager.LoadScene("TitleScreen");
            }
        }
        else
        {
            if(!firebaseInitialized && managerEnabled) InitializeFirebase();
            if(firebaseInitialized && !managerEnabled) RemoveFirebase();

            if(firebaseInitialized)
            {
                if(networkDisconnected && Time.realtimeSinceStartup > networkDisconnectTime + 8 && SceneManager.GetActiveScene().name != "Lobby" && SceneManager.GetActiveScene().name != "TitleScreen")
                {
                    _static.Child(PlayerPrefs.GetString("UserNumber")).RemoveValueAsync();
                    managerEnabled = false;
                    SceneManager.LoadScene("Lobby");
                }
                if(partnerIsPaused && Time.realtimeSinceStartup - partnerPauseTime > timeBeforeKickout) RemovePartner();
            }
        }
	}

	void InitializeFirebase()
	{
        _manager = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("GameManager");
        _static = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Static");
        _settings = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Settings");

        _manager.ValueChanged += ManagerChanged;
        _static.ValueChanged += StaticChanged;
        _settings.ValueChanged += SettingsChanged;

        firebaseInitialized = true;
        Debug.Log("Firebase Initialized");
    }

    void RemoveFirebase()
    {
        if(_settings != null) { _settings.ValueChanged -= SettingsChanged; _settings = null; }
        if(_manager != null) { _manager.ValueChanged -= ManagerChanged; _manager = null; }
        if(_static != null) { _static.ValueChanged -= StaticChanged; _static = null; }
        firebaseInitialized = false;
        Debug.Log("Firebase uninitializer");
    }

    void OnDisable()
    {
        RemoveFirebase();
        if(_network != null) { _network.ValueChanged -= ConnectionChanged; _network = null; }
    }

    void ConnectionChanged(object sender, ValueChangedEventArgs args)
    {
        if((bool)args.Snapshot.Value)
        {
            if(Error) Error.ClearErrorMessage();
            networkDisconnected = false;
        }
        else
        {
            if(Error && SceneManager.GetActiveScene().name != "Loading") Error.SetErrorMessage("No Network Connection");
            networkDisconnected = true;
            networkDisconnectTime = Time.realtimeSinceStartup;
        }
    }

    void SettingsChanged(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;
        if(snapshot.ChildrenCount > 0)
        {
            CurrentSettings = JsonUtility.FromJson<GameSettings>(snapshot.GetRawJsonValue());
            settingsInitialized = true;
        }
    }

    void ManagerChanged(object sender, ValueChangedEventArgs args)
	{
        DataSnapshot snapshot = args.Snapshot;
        if(snapshot.ChildrenCount > 0)
		{
            PlayerManager = JsonUtility.FromJson<GamePlayerManager>(snapshot.GetRawJsonValue());
            owner = PlayerManager.Owner;
            controller = PlayerManager.Controller;
            managerInitialized = true;
            CheckGameMessage(PlayerManager.GameMessage);
        }
    }

    void StaticChanged(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;
        if(snapshot.ChildrenCount > 0)
        {
            PlayerStatic.Clear();
            CurrentPlayers.Clear();
            Partner = "";
            bool nextIsPartner = false;
            bool ownerThere = false;
            int firstPlayer = -1;
            int prevTotalPlayers = totalPlayers;
            int prevActivePlayers = activePlayers;
            totalPlayers = 0;
            activePlayers = 0;
            foreach(var child in snapshot.Children)
            {
                GamePlayerStatic info = JsonUtility.FromJson<GamePlayerStatic>(snapshot.Child(child.Key).GetRawJsonValue());
                if(info.ID != null)
                {
                    if(firstPlayer == -1 && info.Pause == 0) firstPlayer = int.Parse(child.Key);
                    if(Partner == "") Partner = child.Key;
                    if(nextIsPartner) { Partner = child.Key; nextIsPartner = false; }
                    if(child.Key == PlayerPrefs.GetString("UserNumber")) nextIsPartner = true;
                    if(int.Parse(child.Key) == owner) ownerThere = true;

                    if(Partner != "" && partnerIsPaused && child.Key == Partner && info.Pause == 0) partnerIsPaused = false;
                    if(Partner != "" && child.Key == Partner && info.Pause == 1 && !partnerIsPaused)
                    {
                        partnerIsPaused = true;
                        partnerPauseTime = Time.realtimeSinceStartup;
                    }

                    if(info.Pause != 1) activePlayers++;
                    totalPlayers++;

                    PlayerStatic.Add(child.Key, info);
                    CurrentPlayers.Add(child.Key);
                }
            }
            if(PlayerStatic[PlayerManager.Controller.ToString()].Pause == 1) _manager.Child("Controller").SetValueAsync(firstPlayer);
            nextInLine = int.Parse(CurrentPlayers[CurrentPlayers.Count - 1]) + 1;

            if(!ownerThere && settingsInitialized) ReplaceOwner();
            if(CurrentSettings.Status == "open" && CurrentSettings.NextInLine != nextInLine) FirebaseDatabase.DefaultInstance.GetReference("Lobby").Child(PlayerPrefs.GetString("LobbyNumber")).Child("NextInLine").SetValueAsync(nextInLine);
            if(prevTotalPlayers != totalPlayers) FirebaseDatabase.DefaultInstance.GetReference("Lobby").Child(PlayerPrefs.GetString("LobbyNumber")).Child("CurrentPlayers").SetValueAsync(totalPlayers);
            if(prevActivePlayers != activePlayers) _settings.Child("CurrentPlayers").SetValueAsync(activePlayers);
            settingsStaticChange = true;
            staticInitialized = true;
        }
    }

    void ReplaceOwner()
	{
        foreach(var key in PlayerStatic)
		{
            if(PlayerStatic[key.Key].Pause == 0)
			{
                owner = int.Parse(key.Key);
                break;
			}
		}
        Debug.Log("Changed Owner");
        _manager.Child("Owner").SetValueAsync(owner);
    }

    void OnApplicationFocus(bool focused)
    {
        if(managerEnabled)
        {
            if(!focused)
            {
                pauseTime = Time.realtimeSinceStartup;
                if(activePlayers > 1) _static.Child(PlayerPrefs.GetString("UserNumber")).Child("Pause").SetValueAsync(1);
                else RemoveGame();
            }
            else
            {
                if(Time.realtimeSinceStartup - pauseTime < timeBeforeKickout) SetResume();
                else
                {
                    managerEnabled = false;
                    SceneManager.LoadScene("Lobby");
                }
            }
        }
    }

    public void RemovePartner()
	{
        _static.Child(Partner).RemoveValueAsync();
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Positions").Child(Partner).RemoveValueAsync();
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Variables").Child(Partner).RemoveValueAsync();
        partnerIsPaused = false;
    }

    void CheckGameMessage(string message)
	{
        if(message != "empty")
        {
            Debug.Log(message);
            if(message.Contains("Randomize"))
            {
                if(!GameStartTimeInitiated) GameStartTime = Time.realtimeSinceStartup;
                if(SceneManager.GetActiveScene().name != "RandomSelect") SceneManager.LoadScene("RandomSelect");
            }
            else
            {
                if(SceneManager.GetActiveScene().name != "Game") SceneManager.LoadScene("Game");
            }
        }
	}

    async void SetResume()
    {
        bool restoreIt = false;
        string un = PlayerPrefs.GetString("UserNumber");
        GamePlayerStatic tempStat = new GamePlayerStatic();
        tempStat.ID = PlayerPrefs.GetString("UserName");
        tempStat.Color = PlayerPrefs.GetInt("UserColor");
        string jsonTemp = JsonUtility.ToJson(tempStat);

        await _static.GetValueAsync().ContinueWithOnMainThread(task => {
            if(!task.Result.Exists)
            {
                if(SceneManager.GetActiveScene().name == "Setup") restoreIt = true;
                else SceneManager.LoadScene("Lobby");
            }
            else
            {
                _static.Child(un).SetRawJsonValueAsync(jsonTemp);
                if(SceneManager.GetActiveScene().name != "RandomSelect" && PlayerManager.GameMessage.Contains("Randomize")) SceneManager.LoadScene("RandomSelect");
                else if(SceneManager.GetActiveScene().name != "Game" && PlayerManager.GameMessage.Contains("Game")) SceneManager.LoadScene("Game");
            }
        });

        if(restoreIt) RestoreGame();
    }

    public void RemoveGame(bool goToLobby = false)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).RemoveValueAsync().ContinueWith(task => {
            if(task.Exception != null) Error.SetErrorMessage("Error Removing Game from Games");
        });

        FirebaseDatabase.DefaultInstance.GetReference("Lobby").Child(PlayerPrefs.GetString("LobbyNumber")).RemoveValueAsync().ContinueWith(task => {
            if(task.Exception != null) Error.SetErrorMessage("Error Removing Game from Lobby");
        });

        if(goToLobby)
        {
            managerEnabled = false;
            SceneManager.LoadScene("Lobby");
        }
    }

    public async void RestoreGame()
    {
        CurrentSettings.CurrentPlayers = 1;
        CurrentSettings.NextInLine = nextInLine;
        string jsonSettings = JsonUtility.ToJson(CurrentSettings);
        await _settings.SetRawJsonValueAsync(jsonSettings).ContinueWith(task => {
            if(task.Exception != null) Error.SetErrorMessage("Error Updating Settings");
        });
        await FirebaseDatabase.DefaultInstance.GetReference("Lobby").Child(PlayerPrefs.GetString("LobbyNumber")).SetRawJsonValueAsync(jsonSettings).ContinueWith(task => {
            if(task.Exception != null) Error.SetErrorMessage("Error Updating Lobby");
        });

        string jsonManager = JsonUtility.ToJson(PlayerManager);
        await _manager.SetRawJsonValueAsync(jsonManager).ContinueWith(task => {
            if(task.Exception != null) Error.SetErrorMessage("Error Updating Manager");
        });

        string un = PlayerPrefs.GetString("UserNumber");
        GamePlayerStatic tempStat = new GamePlayerStatic();
        tempStat.ID = PlayerPrefs.GetString("UserName");
        tempStat.Color = PlayerPrefs.GetInt("UserColor");
        string jsonTemp = JsonUtility.ToJson(tempStat);
        await _static.Child(un).SetRawJsonValueAsync(jsonTemp);
    }

    public void StartPreGame()
	{
        _manager.Child("GameMessage").SetValueAsync("Pregame");
    }

    public void StartRealGame()
    {
        _manager.Child("GameMessage").SetValueAsync("Game");
    }

    public void StartEndGame()
	{
        GameStartTimeInitiated = false;
	}
}
