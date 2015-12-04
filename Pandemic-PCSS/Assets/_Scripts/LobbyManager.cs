using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LobbyManager : NetworkManager
{

    public void StartGame()
    {
        NetworkManager.singleton.StartHost();
    }

    public void JoinGame()
    {
        NetworkManager.singleton.StartClient();
    }
}
