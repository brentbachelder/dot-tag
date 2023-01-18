using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomSelectController : MonoBehaviour
{
    GameManagerController GameManager;
    [SerializeField] Transform DotParent;
    [SerializeField] GameObject DotPrefab;
    [SerializeField] Text Message;

    Dictionary<string, Transform> Dots = new Dictionary<string, Transform>();
    List<int> DotIDs = new List<int>();

    float startTime;
    int officiallyIt;

	void Start()
	{
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManagerController>();

        startTime = GameManager.GameStartTime;
        int itNum = GameManager.PlayerManager.GameMessage.IndexOf("|");
        string itNumCondensed = GameManager.PlayerManager.GameMessage.Substring(itNum + 1);
        officiallyIt = int.Parse(itNumCondensed);
        
        int counter = 0;
        DotParent.GetComponent<RectTransform>().sizeDelta = new Vector2((140 * GameManager.PlayerStatic.Count) - 140, 100);
        foreach(var key in GameManager.PlayerStatic)
		{
            DotIDs.Add(int.Parse(key.Key));

            var dot = Instantiate(DotPrefab, DotParent);
            Dots.Add(key.Key, dot.transform);
            dot.GetComponent<RectTransform>().anchoredPosition = new Vector3(140 * counter, 0, 0);
            dot.GetComponent<RandomDot>().color = GameManager.color[GameManager.PlayerStatic[key.Key].Color];
            dot.GetComponent<RandomDot>().playerName = GameManager.PlayerStatic[key.Key].ID;
            counter++;
		}

        StartCoroutine("TheWholeProcess");
	}

    IEnumerator TheWholeProcess()
	{
        while(Time.realtimeSinceStartup < startTime + 3f)
		{
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float moveAmount = elapsedTime / .01f;
            if(DotParent.GetComponent<RectTransform>().anchoredPosition.y < -60) DotParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(DotParent.GetComponent<RectTransform>().anchoredPosition.x, -660 + (moveAmount * 2));
            yield return new WaitForSeconds(.01f);
        }
        DotParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(DotParent.GetComponent<RectTransform>().anchoredPosition.x, -60);

        //float timer = 4f;
        int it = 0;
        while(Time.realtimeSinceStartup < startTime + 7f)
		{
            float timer = (startTime + 7f) - Time.realtimeSinceStartup;
            float timeScale = Mathf.Max(.1f, (4f - timer) / 10);

            string itString = DotIDs[it].ToString();
            foreach(var dot in Dots) Dots[dot.Key].GetComponent<RandomDot>().isIt = false;
            Dots[itString].GetComponent<RandomDot>().isIt = true;
            if(it + 1 == DotIDs.Count) it = 0;
            else it++;
            yield return new WaitForSeconds(timeScale);
		}

        while(Time.realtimeSinceStartup < startTime + 9f)
        {
            while(DotIDs[it] != officiallyIt)
            {
                if(Time.realtimeSinceStartup > startTime + 9f) break;
                string itString = DotIDs[it].ToString();
                foreach(var dot in Dots) Dots[dot.Key].GetComponent<RandomDot>().isIt = false;
                Dots[itString].GetComponent<RandomDot>().isIt = true;
                if(it + 1 == DotIDs.Count) it = 0;
                else it++;
                yield return new WaitForSeconds(.6f);
            }
        }

        foreach(var dot in Dots) Dots[dot.Key].GetComponent<RandomDot>().isIt = false;
        Dots[officiallyIt.ToString()].GetComponent<RandomDot>().isIt = true;
        Message.text = $"{Dots[officiallyIt.ToString()].GetComponent<RandomDot>().playerName} is it!";

        //yield return new WaitForSeconds(3f);
        while(Time.realtimeSinceStartup < startTime + 12f)
		{
            yield return 0;
		}
        if(GameManager.PlayerManager.Controller == int.Parse(PlayerPrefs.GetString("UserNumber"))) GameManager.StartPreGame();
	}
}
