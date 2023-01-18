using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] InputField username;
    [SerializeField] Text usernameError;
    bool readyToStart = false;
    
    // Start is called before the first frame update
    void Start()
    {
        username.characterLimit = 12;
        usernameError.gameObject.SetActive(false);
        if(PlayerPrefs.HasKey("UserName") && PlayerPrefs.GetString("UserName") != null) username.text = PlayerPrefs.GetString("UserName");
        else
		{
            int random = Random.Range(1000, 10000);
            username.text = "user" + random;
        }
        setUsername();
    }

    public void checkUserNameRegex()
	{
        username.text = Regex.Replace(username.text, @"[^a-zA-Z0-9]", "");
	}

    public void setUsername()
	{
        if(username.text != "" && username.text.Length >= 5) 
        { 
            PlayerPrefs.SetString("UserName", username.text); 
            readyToStart = true;
            usernameError.gameObject.SetActive(false);
        }
        else 
        { 
            readyToStart = false;
            usernameError.gameObject.SetActive(true);
        }
	}

    public void goToLobby()
	{
        if(readyToStart) SceneManager.LoadScene("Lobby");
    }
}
