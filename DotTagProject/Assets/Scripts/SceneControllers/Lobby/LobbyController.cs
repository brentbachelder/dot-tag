using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LobbyController : MonoBehaviour
{
    GameManagerController GameManager;
    [SerializeField] LobbyNetwork NetworkController;

    Dictionary<string, string> gameNames = new Dictionary<string, string>();

    [SerializeField] GameObject OpenGameObject;
    [SerializeField] GameObject OpenGameContent;
    [SerializeField] GameObject CreateGameError;
    [SerializeField] GameObject NotFoundError;
    [SerializeField] GameObject GameFullError;
    [SerializeField] Button CreateGameButton;
    [SerializeField] Button FindGameButton;
    [SerializeField] Button ActualCreateButton;

    [Space]

    [SerializeField] GameObject createGameParent;
    [SerializeField] InputField createGameInput;
    [SerializeField] GameObject findGameParent;
    [SerializeField] InputField findGameInput;
    [SerializeField] Text noOpenGamesMessage;
    [SerializeField] Image privateParent;
    [SerializeField] Sprite[] privateCheck = new Sprite[2];

    bool isPrivate = false;

	void Start()
	{
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        createGameInput.characterLimit = 10;
        CreateGameError.SetActive(false);
        findGameInput.characterLimit = 10;
        NotFoundError.SetActive(false);
        GameFullError.SetActive(false);
        createGameParent.SetActive(false);
        findGameParent.SetActive(false);
    }

	void Update()
	{
        if(GameManager.networkDisconnected)
        {
            CreateGameButton.interactable = false;
            FindGameButton.interactable = false;
        }
        else
		{
            CreateGameButton.interactable = true;
            FindGameButton.interactable = true;
        }
	}

	public void UpdateOpenGames(Dictionary<string, GameSettings> TempGames)
	{
        OpenGameContent.GetComponent<RectTransform>().sizeDelta = new Vector2(980, ((TempGames.Count + 1) * 160));
        gameNames.Clear();

        foreach(Transform child in OpenGameContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        int counter = 0;
        foreach(var game in TempGames)
        {
            if(game.Value.Status == "open" && game.Value.Type == "public" && game.Value.CurrentPlayers < game.Value.MaxPlayers)
            {
                float y = -80 - (180 * counter);
                GameObject ogo = Instantiate(OpenGameObject, OpenGameContent.GetComponent<RectTransform>());
                ogo.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, y, 0);
                ogo.GetComponent<Button>().onClick.AddListener(delegate { NetworkController.JoinGame(game.Key); });
                ogo.GetComponent<GIPController>().gameText.text = game.Value.GameID;
                ogo.GetComponent<GIPController>().playerText.text = game.Value.CurrentPlayers + " / " + game.Value.MaxPlayers;
                ogo.GetComponent<GIPController>().gameType = game.Value.GameType;
                if(!gameNames.ContainsKey(game.Key)) gameNames.Add(game.Key, game.Value.GameID.ToLower());
                counter++;
            }
        }
        if(counter > 0) noOpenGamesMessage.gameObject.SetActive(false);
        else noOpenGamesMessage.gameObject.SetActive(true);
    }

    public void CreateGameIn()
    {
        ActualCreateButton.interactable = false;
        GameSettings gameSettings = new GameSettings();
        gameSettings.GameID = createGameInput.text;
        GamePlayerManager GPM = new GamePlayerManager();

        if(isPrivate) { gameSettings.Type = "private"; }

        NetworkController.Games.Add(NetworkController.lastKey.ToString(), gameSettings);

        string jsonGame = JsonUtility.ToJson(gameSettings);
        string jsonGPM = JsonUtility.ToJson(GPM);

        NetworkController.CreateGameOut(jsonGame, jsonGPM, createGameInput.text);
    }

    public void ShowCreateGame()
    {
        createGameParent.SetActive(true);

        int random = Random.Range(1000, 10000);
        string gameName = "game" + random;

        createGameInput.text = gameName;
        createGameInput.Select();
        createGameInput.ActivateInputField();
    }

    public void HideCreateGame()
	{
        CreateGameError.SetActive(false);
        createGameParent.SetActive(false);
    }

    public void CheckCreateGame()
	{
        string gameName = createGameInput.text.ToLower();
        if(gameNames.ContainsValue(gameName))
		{
            CreateGameError.SetActive(true);
            createGameInput.Select();
            createGameInput.ActivateInputField();
        }
        else
		{
            CreateGameError.SetActive(false);
            CreateGameIn();
		}
	}

    public void CheckPrivate()
	{
        if(isPrivate)
		{
            privateParent.sprite = privateCheck[0];
            isPrivate = false;
		}
        else
		{
            privateParent.sprite = privateCheck[1];
            isPrivate = true;
		}
	}
    
    public void ShowFindGame()
	{
        findGameParent.SetActive(true);
        findGameInput.Select();
        findGameInput.ActivateInputField();
    }

    public void HideFindGame()
	{
        NotFoundError.SetActive(false);
        GameFullError.SetActive(false);
        findGameParent.SetActive(false);
    }

    public void CheckForFindGame()
	{
        string search = findGameInput.text.ToLower();
        if(!gameNames.ContainsValue(search))
		{
            GameFullError.SetActive(false);
            NotFoundError.SetActive(true);
            findGameInput.Select();
            findGameInput.ActivateInputField();
        }
        else
		{
            NotFoundError.SetActive(false);
            string gameID = "";
            foreach(var key in gameNames) {
                if(gameNames[key.Key].ToLower() == search) gameID = key.Key;
            }
            if(gameID != "")
            {
                if(NetworkController.Games[gameID].CurrentPlayers < NetworkController.Games[gameID].MaxPlayers)
                {
                    GameFullError.SetActive(false);
                    NetworkController.JoinGame(gameID);
                }
                else
                {
                    NotFoundError.SetActive(false);
                    GameFullError.SetActive(true);
                    findGameInput.Select();
                    findGameInput.ActivateInputField();
                }
            }
		}
	}

    public void CheckGameRegex(bool isFind = false)
    {
        if(isFind) findGameInput.text = Regex.Replace(findGameInput.text, @"[^a-zA-Z0-9]", "");
        else createGameInput.text = Regex.Replace(createGameInput.text, @"[^a-zA-Z0-9]", "");
    }
}
