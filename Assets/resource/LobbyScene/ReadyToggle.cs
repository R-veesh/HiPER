using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{
    public Image buttonImage;
    public Sprite readySprite;
    public Sprite notReadySprite;

    private bool isReady = false;

    public void ToggleReady()
    {
        isReady = !isReady;

        if (isReady)
        {
            buttonImage.sprite = readySprite;
            Debug.Log("Player READY");
        }
        else
        {
            buttonImage.sprite = notReadySprite;
            Debug.Log("Player NOT READY");
        }
    }
}