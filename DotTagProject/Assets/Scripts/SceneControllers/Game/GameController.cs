using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    GameManagerController GameManager;
    [SerializeField] GameNetwork NetworkController;
    [SerializeField] CameraController Camera;

    Dictionary<string, GameObject> dotPrefabs = new Dictionary<string, GameObject>();

    public float rotationSpeed = 180;
    Vector2 previousPosition;

    float startTime;
    int cameraZoom;
    public bool gameStarted = false;
    public bool gameOver = false;


    

    
    [SerializeField] PlayerController PlayerController;
    
    [SerializeField] GameObject DotPrefab;
    [SerializeField] Transform DotsParent;
    [SerializeField] Text TimerText;
    [SerializeField] GameObject CountdownParent;
    [SerializeField] Text CountdownText;
    [SerializeField] Text CountdownGameStarts;

    [Space]

    
    Transform myDot;
    int latency = 4;
    int latencyCounter = 0;

    [Space]

    int timer;
    [HideInInspector] public int gameType;
    [HideInInspector] public int playerSpeed;

    

    // Diag stuff
    float networkLatencyIn;
    [SerializeField] Text NetworkLatencyText;
    [SerializeField] Text DefaultFrameLatencyText;
    [SerializeField] Text SendPositionText;
    [SerializeField] Text RecievePositionText;
    [SerializeField] Text MyPartnerText;

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();

        if(GameManager.CurrentSettings.CameraZoom == 0) cameraZoom = 5;
        else if(GameManager.CurrentSettings.CameraZoom == 1) cameraZoom = 9;
        else cameraZoom = 15;
        
        DefaultFrameLatencyText.text = latency.ToString() + " frames";
        TimerText.text = "";

        startTime = GameManager.GameStartTime;
        MyPartnerText.text = GameManager.PlayerStatic[GameManager.Partner].ID;

        StartCoroutine("CheckForPositions");
    }

    // Update is called once per frame
    void Update()
    {
        if(gameStarted)
		{
            if(latencyCounter <= 0)
            {
                Vector2 myPosition = MyPosition();
                if(myPosition.x != previousPosition.x || myPosition.y != previousPosition.y)
			    {
                    previousPosition = myPosition;
                    NetworkController.SendPosition(myPosition);
                    SendPositionText.text = (myPosition.x / 100f) + ", " + (myPosition.y / 100f);
                }
                latencyCounter = latency;
            }
            if(GameManager.PlayerStatic.Count != dotPrefabs.Count) UpdatePlayers();
		}
    }

    public void InitializeSettings()
	{
        CreatePlayers();
        InstantPlayerPositions();

        Camera.zoom = cameraZoom;
        if(Time.realtimeSinceStartup < startTime + 14f) Camera.StartCoroutine("SetZoom");
        else Camera.AutoZoom();

        StartCoroutine("PreGame");
    }

    IEnumerator CheckForPositions()
	{
        while(!NetworkController.initialVariablesDone)
		{
            yield return 0;
		}
        while(!NetworkController.initialPositionsDone)
        {
            yield return 0;
        }
        InitializeSettings();
    }

    void CreatePlayers()
	{
        float moveSpeed = .2f;
        if(GameManager.CurrentSettings.PlayerSpeed == 1) moveSpeed = .35f;
        dotPrefabs.Clear();

        foreach(var player in GameManager.PlayerStatic)
        {
            var dot = Instantiate(DotPrefab, DotsParent);
            dotPrefabs[player.Key] = dot;
            dot.GetComponent<DotController>().color = GameManager.color[GameManager.PlayerStatic[player.Key].Color];
            dot.GetComponent<DotController>().ID = GameManager.PlayerStatic[player.Key].ID;
            dot.GetComponent<DotController>().moveSpeed = moveSpeed;
            dot.GetComponent<DotController>().rotationSpeed = rotationSpeed;
            dot.GetComponent<DotController>().GameType = GameManager.CurrentSettings.GameType;
            dot.GetComponent<DotController>().isIt = NetworkController.PlayerVariables[player.Key].It;
            if(player.Key == PlayerPrefs.GetString("UserNumber"))
            {
                PlayerController.myDot = dot;
                myDot = dot.transform;
                Camera.target = dot.transform;
                dot.GetComponent<DotController>().isMyDot = true;
            }
        }
    }

    void InstantPlayerPositions()
	{
        Debug.Log("Running Instant Player Positions");
        foreach(var player in NetworkController.PlayerPositions)
		{
            dotPrefabs[player.Key].GetComponent<Rigidbody2D>().position = new Vector2(NetworkController.PlayerPositions[player.Key].xPos / 100f, NetworkController.PlayerPositions[player.Key].yPos / 100f);
            dotPrefabs[player.Key].GetComponent<DotController>().GoalPosition = new Vector2(NetworkController.PlayerPositions[player.Key].xPos / 100f, NetworkController.PlayerPositions[player.Key].yPos / 100f);
            if(player.Key == PlayerPrefs.GetString("UserNumber"))
            {
                PlayerController.myPosition = new Vector2(NetworkController.PlayerPositions[player.Key].xPos / 100f, NetworkController.PlayerPositions[player.Key].yPos / 100f);
                previousPosition = new Vector2(NetworkController.PlayerPositions[player.Key].xPos / 100f, NetworkController.PlayerPositions[player.Key].yPos / 100f);
                RecievePositionText.text = NetworkController.PlayerPositions[player.Key].xPos / 100f + ", " + NetworkController.PlayerPositions[player.Key].yPos / 100f;
            }
            if(NetworkController.PlayerVariables[player.Key].It == 1) dotPrefabs[player.Key].GetComponent<DotController>().ImIt(true);
            else dotPrefabs[player.Key].GetComponent<DotController>().GetInitialDirection = true;
        }
	}

    void UpdatePlayers()
	{
        foreach(var player in dotPrefabs)
		{
            if(!GameManager.PlayerStatic.ContainsKey(player.Key))
			{
                Destroy(dotPrefabs[player.Key].gameObject);
                dotPrefabs.Remove(player.Key);
			}
		}
	}

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PreGame()
	{
        while(Time.realtimeSinceStartup < startTime + 18f)
		{
            float diff = Time.realtimeSinceStartup - startTime;
            if(diff < 14f && diff > 13.5f)
			{
                float moveAmt = 14f - diff;
                CountdownParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1920 * moveAmt * 2, 0);
			}
            else if(diff >= 14f)
			{
                CountdownParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                CountdownGameStarts.text = "Game Starts In";
                if(diff >= 15f)
                {
                    if(diff < 16f) CountdownText.text = "3";
                    else if(diff < 17f) CountdownText.text = "2";
                    else CountdownText.text = "1";
                }
            }
            yield return 0;
		}
        CountdownParent.SetActive(false);
        timer = GameManager.CurrentSettings.TimeLimit;
        UpdateTimer();
        if(GameManager.PlayerManager.Controller == int.Parse(PlayerPrefs.GetString("UserNumber"))) GameManager.StartRealGame();
        gameStarted = true;
        NetworkController.initialSettingsDone = true;
        StartCoroutine("TimerCountdown");
    }

    void EndGame()
	{

	}
    public void UpdatePositions(Dictionary<string, GamePlayerPositions> positions)
	{
        foreach(var pos in positions)
		{
            if(dotPrefabs.ContainsKey(pos.Key) && dotPrefabs[pos.Key])
			{
                Vector2 networkPosition = new Vector2(positions[pos.Key].xPos / 100f, positions[pos.Key].yPos / 100f);
                dotPrefabs[pos.Key].GetComponent<DotController>().GoalPosition = networkPosition;
                dotPrefabs[pos.Key].GetComponent<DotController>().GetDirection = true;
                if(gameStarted) dotPrefabs[pos.Key].GetComponent<DotController>().GetInitialDirection = false;
                if(pos.Key == PlayerPrefs.GetString("UserNumber")) RecievePositionText.text = networkPosition.x + ", " + networkPosition.y;
            }
		}
        if(NetworkLatencyText != null)
        {
            NetworkLatencyText.text = Mathf.Round((Time.realtimeSinceStartup - networkLatencyIn) * 1000) + " ms";
            networkLatencyIn = Time.realtimeSinceStartup;
            MyPartnerText.text = GameManager.PlayerStatic[GameManager.Partner].ID;
        }
	}

    public void UpdateVariables(Dictionary<string, GamePlayerVariables> variables)
	{
        foreach(var key in variables)
        {
            if(dotPrefabs.ContainsKey(key.Key)) dotPrefabs[key.Key].GetComponent<DotController>().isIt = variables[key.Key].It;
            if(dotPrefabs.ContainsKey(key.Key)) dotPrefabs[key.Key].GetComponent<DotController>().lives = variables[key.Key].Lives;
        }
    }

    Vector2 MyPosition()
    {
        Vector2 playerPosition;
        playerPosition.x = Mathf.FloorToInt(PlayerController.myPosition.x * 100f);
        playerPosition.y = Mathf.FloorToInt(PlayerController.myPosition.y * 100f);
        return playerPosition;
    }

    IEnumerator TimerCountdown()
	{
        TimerText.gameObject.SetActive(true);
        while(timer > 0)
		{
            yield return new WaitForSeconds(1);
            timer--;
            UpdateTimer();
		}
        GameOver();
        if(GameManager.PlayerManager.Controller == PlayerPrefs.GetInt("UserNumber")) NetworkController.EndGame();
	}

    void UpdateTimer()
	{
        int min = Mathf.FloorToInt(timer / 60);
        int sec = timer - min * 60;
        string secText = "";
        if((int)sec < 10) secText = "0" + sec.ToString();
        else secText = sec.ToString();
        
        TimerText.text = $"{min}:{secText}";
	}

    public void GameOver()
	{
        gameOver = true;
        gameStarted = false;
    }

	void FixedUpdate()
	{
        if(gameStarted && !gameOver) latencyCounter--;
	}
}
