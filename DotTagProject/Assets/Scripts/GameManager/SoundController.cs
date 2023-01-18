using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundController : MonoBehaviour
{
    bool soundOn;
    [SerializeField] Image SoundButtonParent;
    [SerializeField] Sprite[] soundSprites = new Sprite[2];

    public AudioSource backgroundMusic;

    bool pastLoading = false;


    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("SoundOn") == 1) soundOn = false;
        else soundOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!pastLoading && SceneManager.GetActiveScene().name != "Loading")
        {
            pastLoading = true;
            SoundChange();
        }

        if(pastLoading)
        {
            if(SceneManager.GetActiveScene().name != "Lobby" && SceneManager.GetActiveScene().name != "Setup" && SceneManager.GetActiveScene().name != "TitleScreen") SoundButtonParent.gameObject.SetActive(false);
            else SoundButtonParent.gameObject.SetActive(true);
        }
    }

    public void SoundChange()
    {
        if(soundOn)
        {
            SoundButtonParent.sprite = soundSprites[1];
            soundOn = false;
            PlayerPrefs.SetInt("SoundOn", 0);
            backgroundMusic.Pause();
        }
        else
        {
            SoundButtonParent.sprite = soundSprites[0];
            soundOn = true;
            PlayerPrefs.SetInt("SoundOn", 1);
            backgroundMusic.Play();
        }
    }
}
