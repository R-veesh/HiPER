using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField ipInput;

    public void Host()
    {
        NetworkManager.singleton.StartHost();
    }

    public void Join()
    {
        NetworkManager.singleton.networkAddress = ipInput.text;
        NetworkManager.singleton.StartClient();
    }

    public void Quit()
    {
        Application.Quit();
    }
}