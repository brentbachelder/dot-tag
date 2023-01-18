using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

public class GameNetwork : MonoBehaviour
{
    GameManagerController GameManager;

    DatabaseReference _positions;
    DatabaseReference _variables;

    [SerializeField] GameController Controller;
    [SerializeField] TilemapController Tilemap;
    [SerializeField] Error Error;

    public Dictionary<string, GamePlayerPositions> PlayerPositions = new Dictionary<string, GamePlayerPositions>();
    public Dictionary<string, GamePlayerVariables> PlayerVariables = new Dictionary<string, GamePlayerVariables>();
    public bool initialSettingsDone = false;
    public bool initialPositionsDone = false;
    public bool initialVariablesDone = false;


    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();
        _positions = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Positions");
        _variables = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Variables");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if(task.Exception != null)
            {
                Error.SetErrorMessage("Couldn't connect to database");
                return;
            }

            if(!initialSettingsDone) GetInitialSettings();
        });
    }

    async void GetInitialSettings()
    {
        // Create the Map
        await FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("Map").GetValueAsync().ContinueWithOnMainThread(task => {
            string map = task.Result.Value.ToString();
            Tilemap.CreateRoom(map);
        });

        await _variables.GetValueAsync().ContinueWithOnMainThread(task => {
            DataSnapshot snapshot = task.Result;
            PlayerVariables.Clear();

            foreach(var child in snapshot.Children)
            {
                GamePlayerVariables info = JsonUtility.FromJson<GamePlayerVariables>(snapshot.Child(child.Key).GetRawJsonValue());
                PlayerVariables.Add(child.Key, info);
            }
            initialVariablesDone = true;
        });
        if(GameManager.PlayerManager.Controller == int.Parse(PlayerPrefs.GetString("UserNumber"))) FindPositions();
        _positions.ValueChanged += PositionsChanged;
        _variables.ValueChanged += VariablesChanged;
    }

    async void FindPositions()
	{
        PlayerPositions.Clear();
        foreach(var player in GameManager.PlayerStatic)
		{
            Vector2 pos = Tilemap.FindSpaces();
            if(player.Key == PlayerPrefs.GetString("RandomSelect")) pos = Vector2.zero;
            Vector2 submitPos = new Vector2(Mathf.FloorToInt(pos.x * 100f), Mathf.FloorToInt(pos.y * 100f));
            await _positions.Child(player.Key).Child("xPos").SetValueAsync(submitPos.x);
            await _positions.Child(player.Key).Child("yPos").SetValueAsync(submitPos.y);
        }
    }

    void OnDisable()
    {
        if(_variables != null) { _variables.ValueChanged -= VariablesChanged; _variables = null; }
    }

    void VariablesChanged(object sender, ValueChangedEventArgs args)
	{
        DataSnapshot snapshot = args.Snapshot;
        PlayerVariables.Clear();

        foreach(var child in snapshot.Children)
		{
            GamePlayerVariables info = JsonUtility.FromJson<GamePlayerVariables>(snapshot.Child(child.Key).GetRawJsonValue());
            PlayerVariables.Add(child.Key, info);
        }

        if(initialSettingsDone) Controller.UpdateVariables(PlayerVariables);
	}

    void PositionsChanged(object sender, ValueChangedEventArgs args)
	{
        DataSnapshot snapshot = args.Snapshot;
        PlayerPositions.Clear();

		foreach(var child in snapshot.Children)
        {
            GamePlayerPositions info = JsonUtility.FromJson<GamePlayerPositions>(snapshot.Child(child.Key).GetRawJsonValue());
            PlayerPositions.Add(child.Key, info);
        }

        if(snapshot.ChildrenCount == GameManager.PlayerStatic.Count) initialPositionsDone = true;
        if(initialSettingsDone) Controller.UpdatePositions(PlayerPositions);
    }

    public void SendPosition(Vector2 myPosition)
	{
        string myNum = PlayerPrefs.GetString("UserNumber");
        _positions.Child(myNum).Child("xPos").SetValueAsync(myPosition.x);
        _positions.Child(myNum).Child("yPos").SetValueAsync(myPosition.y);
    }

    public void StartGame()
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("GameManager").Child("GameMessage").SetValueAsync("GameStarted");
    }

    public void EndGame()
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("GameID")).Child("GameManager").Child("GameMessage").SetValueAsync("GameEnded");
    }
}
