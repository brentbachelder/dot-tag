using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GIPController : MonoBehaviour
{
    public Text gameText;
    public Text playerText;
    public int gameType = 0;
    public Sprite[] gameTypeGraphic = new Sprite[3];

	[Space]

	[SerializeField] Image gtGraphicParent;

	void Start()
	{
		gtGraphicParent.sprite = gameTypeGraphic[gameType];

	}
}
