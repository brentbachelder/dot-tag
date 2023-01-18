using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsController : MonoBehaviour
{
    [SerializeField] Button[] buttons = new Button[10];
    [SerializeField] Image[] arrows = new Image[10];
    [SerializeField] Text[] descriptions = new Text[5];
    [SerializeField] Text[] options = new Text[5];
    [SerializeField] Color disabledColor;
    [SerializeField] Color enabledColor;

    public int controllerNumber = -1;
    int prevControllerNumber;

    void Update()
    {
        if(controllerNumber != prevControllerNumber)
		{
            prevControllerNumber = controllerNumber;
            if(controllerNumber == int.Parse(PlayerPrefs.GetString("UserNumber"))) EnableButtons();
            else DisableButtons();
		}
    }

    void EnableButtons()
	{
        for(var i = 0; i < buttons.Length; i++) buttons[i].interactable = true;
        for(var i = 0; i < arrows.Length; i++) arrows[i].color = enabledColor;
        for(var i = 0; i < descriptions.Length; i++) descriptions[i].color = enabledColor;
        for(var i = 0; i < options.Length; i++) options[i].color = enabledColor;
    }

    void DisableButtons()
	{
        for(var i = 0; i < buttons.Length; i++) buttons[i].interactable = false;
        for(var i = 0; i < arrows.Length; i++) arrows[i].color = disabledColor;
        for(var i = 0; i < descriptions.Length; i++) descriptions[i].color = disabledColor;
        for(var i = 0; i < options.Length; i++) options[i].color = disabledColor;
    }
}
