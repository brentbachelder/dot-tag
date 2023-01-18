using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetupController : MonoBehaviour
{
    GameManagerController GameManager;
    [SerializeField] SetupNetwork NetworkController;
    public GameSettings settingsLocal = new GameSettings();

    [Space]

    [Header("Settings Components")]
    [SerializeField] Text GameName;
    [SerializeField] Text GameType;
    [SerializeField] Text MaxPlayers;
    [SerializeField] Text TimeLimit;
    [SerializeField] Text PlayerSpeed;
    [SerializeField] Text CameraZoom;

    [Space]

    [SerializeField] Transform PlayerListParent;
    [SerializeField] GameObject PlayerNamePrefab;
    [SerializeField] GameObject StartGameButton;

    [Space]

    [SerializeField] GameObject CharacterSettingsButton;
    [SerializeField] GameObject CharacterSettingsMenu;
    [SerializeField] GameObject GameSettingsButton;
    [SerializeField] GameObject GameSettingsMenu;
    [SerializeField] GameSettingsController GameSettingsController;

    [Space]

    [SerializeField] Image SoundButtonParent;
    [SerializeField] RectTransform ColorButtonParent;
    [SerializeField] GameObject ColorButtonPrefab;
    [SerializeField] Image BigDot;
    string[] gameTypes = new string[] { "Three Lives", "Instant Death", "Manhunt" };
    string[] playerSpeed = new string[] { "Normal", "Fast" };
    string[] cameraZoom = new string[] { "Zoomed Out", "Normal", "Zoomed In" };

    Dictionary<string, GameObject> NamePrefabs = new Dictionary<string, GameObject>();

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        StartGameButton.SetActive(false);
        GameName.text = PlayerPrefs.GetString("GameID");
        GameSettingsMenu.SetActive(false);
        CharacterSettingsButton.SetActive(false);
        BigDot.color = GameManager.color[PlayerPrefs.GetInt("UserColor")];
        CreateColorButtons();
    }

	void Update()
	{
        if(GameManager.settingsInitialized && GameManager.managerInitialized && settingsLocal != GameManager.CurrentSettings) UpdateSettings();
        if(GameManager.settingsStaticChange) DrawPlayerPrefabs();
    }

    void DrawPlayerPrefabs()
    {
        GameManager.settingsStaticChange = false;
        NamePrefabs.Clear();
        foreach(Transform child in PlayerListParent) Destroy(child.gameObject);

        int counter = 0;
        foreach(var key in GameManager.PlayerStatic)
        {
            var player = Instantiate(PlayerNamePrefab, PlayerListParent);
            NamePrefabs[key.Key] = player;
            player.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20 - (55 * counter), 0);
            counter++;

            if(key.Key == GameManager.owner.ToString()) NamePrefabs[key.Key].GetComponent<PNController>().isController = true;
            else NamePrefabs[key.Key].GetComponent<PNController>().isController = false;

            string pid = GameManager.PlayerStatic[key.Key].ID;
            if(pid == PlayerPrefs.GetString("UserName")) pid += " (you)";
            NamePrefabs[key.Key].GetComponent<PNController>().PlayerID = pid;

            NamePrefabs[key.Key].GetComponent<PNController>().SetColor(GameManager.color[GameManager.PlayerStatic[key.Key].Color]);
            NamePrefabs[key.Key].GetComponent<PNController>().SetPause(GameManager.PlayerStatic[key.Key].Pause);
        }
        GameSettingsController.controllerNumber = GameManager.owner;
    }


    public void UpdateSettings()
    {
        if(GameManager.CurrentSettings.CurrentPlayers >= 2 && GameManager.owner == int.Parse(PlayerPrefs.GetString("UserNumber"))) StartGameButton.SetActive(true);
        else StartGameButton.SetActive(false);

        if(GameManager.PlayerManager.GameMessage.Contains("randomizing")) SceneManager.LoadScene("RandomSelect");
        else
        {
            settingsLocal = GameManager.CurrentSettings;
            if(GameType.text != gameTypes[settingsLocal.GameType]) GameType.text = gameTypes[settingsLocal.GameType];
            if(MaxPlayers.text != settingsLocal.MaxPlayers.ToString()) MaxPlayers.text = settingsLocal.MaxPlayers.ToString();
            if(TimeLimit.text != settingsLocal.TimeLimit.ToString() + " Seconds") TimeLimit.text = settingsLocal.TimeLimit.ToString() + " Seconds";
            if(PlayerSpeed.text != playerSpeed[settingsLocal.PlayerSpeed]) PlayerSpeed.text = playerSpeed[settingsLocal.PlayerSpeed];
            if(CameraZoom.text != cameraZoom[settingsLocal.CameraZoom]) CameraZoom.text = cameraZoom[settingsLocal.CameraZoom];
            GameSettingsController.controllerNumber = GameManager.owner;
        }
    }    

    public void ChangeColor(int buttonColor)
	{
        BigDot.color = GameManager.color[buttonColor];
        NamePrefabs[PlayerPrefs.GetString("UserNumber")].GetComponent<PNController>().SetColor(GameManager.color[buttonColor]);
        PlayerPrefs.SetInt("UserColor", buttonColor);
        NetworkController.ColorUpdate();
	}

    public void ChangeGameType(int value)
    {
        int newGameType;
        if(settingsLocal.GameType + value < gameTypes.Length && settingsLocal.GameType + value > 0) newGameType = settingsLocal.GameType + value;
        else
        {
            if(settingsLocal.GameType + value < 0) newGameType = gameTypes.Length - 1;
            else newGameType = 0;

        }
        GameType.text = gameTypes[newGameType];
        NetworkController.ChangeGameType(newGameType);
    }

    public void ChangeMaxPlayers(int value)
    {
        int min = 3;
        int max = 9;
        int newMaxPlayers;

        if(settingsLocal.MaxPlayers + value <= max && settingsLocal.MaxPlayers + value > min) newMaxPlayers = settingsLocal.MaxPlayers + value;
        else
        {
            if(settingsLocal.MaxPlayers + value < min) newMaxPlayers = 9;
            else newMaxPlayers = min;

        }
        MaxPlayers.text = newMaxPlayers.ToString();
        NetworkController.ChangeMaxPlayers(newMaxPlayers);
    }

    public void ChangeTimeLimit(int value)
    {
        int newTimeLimit;
        if(settingsLocal.TimeLimit + value < 180 && settingsLocal.TimeLimit + value >= 60) newTimeLimit = settingsLocal.TimeLimit + value;
        else
        {
            if(settingsLocal.TimeLimit + value < 0) newTimeLimit = 180;
            else newTimeLimit = 60;

        }
        TimeLimit.text = newTimeLimit.ToString() + " Seconds";
        NetworkController.ChangeTimeLimit(newTimeLimit);
    }

    public void ChangePlayerSpeed(int value)
    {
        int newPlayerSpeed;
        if(settingsLocal.PlayerSpeed + value < playerSpeed.Length && settingsLocal.PlayerSpeed + value > 0) newPlayerSpeed = settingsLocal.PlayerSpeed + value;
        else
        {
            if(settingsLocal.PlayerSpeed + value < 0) newPlayerSpeed = playerSpeed.Length - 1;
            else newPlayerSpeed = 0;

        }
        PlayerSpeed.text = playerSpeed[newPlayerSpeed];
        NetworkController.ChangePlayerSpeed(newPlayerSpeed);
    }

    public void ChangeCameraZoom(int value)
    {
        int newCameraZoom;
        if(settingsLocal.CameraZoom + value < cameraZoom.Length && settingsLocal.CameraZoom + value > 0) newCameraZoom = settingsLocal.CameraZoom + value;
        else
        {
            if(settingsLocal.CameraZoom + value < 0) newCameraZoom = cameraZoom.Length - 1;
            else newCameraZoom = 0;

        }
        CameraZoom.text = cameraZoom[newCameraZoom];
        NetworkController.ChangeCameraZoom(newCameraZoom);
    }

    public void ShowCharacterSettings()
    {
        GameSettingsMenu.SetActive(false);
        GameSettingsButton.SetActive(true);
        CharacterSettingsMenu.SetActive(true);
        CharacterSettingsButton.SetActive(false);
    }

    public void ShowGameSettings()
    {
        GameSettingsMenu.SetActive(true);
        GameSettingsButton.SetActive(false);
        CharacterSettingsMenu.SetActive(false);
        CharacterSettingsButton.SetActive(true);
    }

    void CreateColorButtons()
	{
        int[] yLoc = new int[] { 225, 75, -75, -225 };
        for(int i = 0; i < 8; i++)
		{
            int xi = i;
            var colBut = Instantiate(ColorButtonPrefab, ColorButtonParent);
            if(i < 4) colBut.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, yLoc[i], 0);
            else colBut.GetComponent<RectTransform>().anchoredPosition = new Vector3(50, yLoc[i - 4], 0);
            colBut.GetComponent<Image>().color = GameManager.color[i];
            colBut.GetComponent<Button>().onClick.AddListener(delegate { ChangeColor(xi); });
        }
	}

    public void LeaveGame()
	{
        if(settingsLocal.CurrentPlayers > 1) NetworkController.RemoveMyself();
        else GameManager.RemoveGame(true);
	}
}