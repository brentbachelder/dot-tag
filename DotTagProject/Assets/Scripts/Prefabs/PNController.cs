using UnityEngine;
using UnityEngine.UI;

public class PNController : MonoBehaviour
{
    public string PlayerID;
    public Color PlayerColor;
    public int Pause = 0;
    public bool isController = false;
    [SerializeField] Image PlayerImage;
    public Text PlayerName;
    [SerializeField] Text c;
    [Space]
    [SerializeField] Color defaultColor;
    [SerializeField] Color disabledColor;

    void Start()
    {
        PlayerName.text = PlayerID;
        PlayerImage.color = PlayerColor;
        if(!isController) c.gameObject.SetActive(false);
    }

    public void SetColor(Color color)
	{
        PlayerImage.color = color;
        PlayerColor = color;
	}

    public void SetPause(int pause)
	{
        if(pause == 1) PlayerName.color = disabledColor;
        else PlayerName.color = defaultColor;
        Pause = pause;
	}
}
