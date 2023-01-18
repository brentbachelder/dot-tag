using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Error : MonoBehaviour
{
    float countdown = 0;
    [SerializeField] GameObject ErrorParent;
    [SerializeField] Text ErrorText;


    void Update()
    {
        if(countdown != -1)
        {
            if(countdown > 0) countdown -= Time.deltaTime;
            else ClearErrorMessage();
        }
    }

    public void SetErrorMessage(string message, int seconds = -1)
    {
        ErrorText.text = message;
        countdown = seconds;
        ErrorParent.SetActive(true);
    }

    public void ClearErrorMessage()
    {
        ErrorText.text = "";
        countdown = 0;
        ErrorParent.SetActive(false);
    }
}
