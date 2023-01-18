using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

public class GameControllerOld : MonoBehaviour
{
    /*public bool debugPlayerPosition = false;

    [Space]
    
    private DatabaseReference _ref;
    [SerializeField] GameObject DotsParent;
    [SerializeField] GameObject DotPrefab;
    Dictionary<string, GameObject> PlayerList = new Dictionary<string, GameObject>();
    GameInfo settings = new GameInfo();

    [Space]

    [SerializeField] PlayerController Player;
    [SerializeField] CameraController cam;
    [SerializeField] Text timerText;
    float timer = -999;
    int prevSec;

    [Space]

    public int latency = 8;
    public bool gameOver = false;
    int UpdatedPositionsCount;
    int errorCount = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        UpdatedPositionsCount = latency;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if(task.Exception != null) { Debug.Log("Error Loading Firebase"); return; }
            GetGameSettings();
        });
    }

    void GetGameSettings()
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("MyGame")).GetValueAsync().ContinueWithOnMainThread(task => {
            if(task.IsFaulted) Debug.Log("GAME CONTROLLER :: Can't get Settings from Firebase");
            else if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                GameInfo ps = JsonUtility.FromJson<GameInfo>(snapshot.GetRawJsonValue());
                settings = ps;

                FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("MyGame")).Child("Players").GetValueAsync().ContinueWithOnMainThread(task2 => {
                    if(task2.IsFaulted) Debug.Log("GAME CONTROLLER :: Can't get Players from Firebase");
                    else if(task2.IsCompleted)
                    {
                        DataSnapshot snapshot2 = task2.Result;
                        CreateDots(snapshot2);
					}
                });
            }
        });
    }

    void CreateDots(DataSnapshot snapshot)
    {
        foreach(var child in snapshot.Children)
        {
            var dot = Instantiate(DotPrefab, DotsParent.transform);
            if(child.Key == PlayerPrefs.GetString("UserName")) { Player.myDot = dot; cam.target = dot.transform; }
            PlayerList[child.Key] = dot;
        }
        timer = settings.Timer;
    }

    void SetPosition()
	{
        PlayerInfo playerPosition = new PlayerInfo();
        if(debugPlayerPosition)
		{
            Vector2 tempPos = Player.myDot.GetComponent<Rigidbody2D>().position;
            playerPosition.xPos = Mathf.FloorToInt(Player.theoreticalPosition.x * 100f);
            playerPosition.yPos = Mathf.FloorToInt(Player.theoreticalPosition.y * 100f);
            //playerPosition.xPos = tempPos.x + Player.movementSum.x;
            //playerPosition.yPos = tempPos.y + Player.movementSum.y;
            playerPosition.isIt = Player.isIt;
            //Player.myDot.GetComponent<Rigidbody2D>().position = new Vector2(playerPosition.xPos / 100, playerPosition.yPos / 100);
		}
        else
		{
            //playerPosition.xPos = Mathf.Round(Player.myDot.GetComponent<Rigidbody2D>().position.x * 100f) / 100f;
            //playerPosition.yPos = Mathf.Round(Player.myDot.GetComponent<Rigidbody2D>().position.x * 100f) / 100f;
            playerPosition.xPos = Mathf.FloorToInt(Player.myDot.GetComponent<Rigidbody2D>().position.x * 100f);
            playerPosition.yPos = Mathf.FloorToInt(Player.myDot.GetComponent<Rigidbody2D>().position.y * 100f);
            playerPosition.isIt = Player.isIt;
        }

        string json = JsonUtility.ToJson(playerPosition);
        _ref = FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("MyGame")).Child("Players").Child(PlayerPrefs.GetString("UserName"));
        _ref.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if(task.IsFaulted) errorCount++;
            else if(task.IsCompleted) GetPositions();
        });
    }


    void GetPositions()
	{
        FirebaseDatabase.DefaultInstance.GetReference("Games").Child(PlayerPrefs.GetString("MyGame")).Child("Players").GetValueAsync().ContinueWithOnMainThread(task => {
            if(task.IsFaulted) errorCount++;
            else
            {
                DataSnapshot snapshot = task.Result;
                ApplyPositions(snapshot);
            }
        });        
    }

    void ApplyPositions(DataSnapshot snapshot)
	{
        foreach(var child in snapshot.Children)
        {
            PlayerInfo dp = JsonUtility.FromJson<PlayerInfo>(snapshot.Child(child.Key).GetRawJsonValue());
            PlayerList[child.Key].GetComponent<DotController>().GoalPosition = new Vector3(dp.xPos / 100, dp.yPos / 100);
        }
    }

    void SetTimer()
	{
        /*timer -= Time.fixedDeltaTime;
        if(timer <= 0 && timer > -999) gameOver = true;

        int min = Mathf.FloorToInt(timer / 60);
        int sec = (int)timer - (min * 60);
        string secDisplay = sec.ToString();
        if(sec < 10) secDisplay = "0" + sec;

        if(sec != prevSec) prevSec = sec;

        timerText.text = min.ToString() + ":" + secDisplay;*/
        /*timerText.text = Time.realtimeSinceStartup.ToString();
    }

	private void FixedUpdate()
	{
        if(Player.myDot != null)
        {
            if(UpdatedPositionsCount <= 0) { SetPosition(); UpdatedPositionsCount = latency; }
            else UpdatedPositionsCount--;
        }

        SetTimer();
    }

	void OnDisable()
    {
        Debug.Log("# of Errors : " + errorCount);
    }*/
}
