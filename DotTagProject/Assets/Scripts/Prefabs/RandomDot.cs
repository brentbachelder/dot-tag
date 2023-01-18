using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomDot : MonoBehaviour
{
    [SerializeField] Text SpriteText;
    [SerializeField] Image SpriteColor;
    
    public Color color;
    public string playerName = "Player";
    public bool isIt = false;
    
    // Start is called before the first frame update
    void Start()
    {
        SpriteColor.color = color;
        SpriteText.text = playerName;
    }

    // Update is called once per frame
    void Update()
    {
        if(isIt) SpriteColor.color = Color.red;
        else SpriteColor.color = color;
    }
}
