using UnityEngine;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Sockets;

class LudoGameManager : MonoBehaviour
{

    private SocketManager socketManager;
    private void Awake()
    {
        socketManager = FindObjectOfType<SocketManager>();
        LocalPlayer.LoadGame();
        CheckInternet();

    }

    private void CheckInternet()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            print("internet connection not available");
            ServerRequest.instance.serverConnection = false;
        }
        else
        {
            ServerRequest.instance.serverConnection = true;

        }
    }

    private void Start()
    {
        if (socketManager == null)
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }

        UiManager.instance.UpdateUi();

        if (LocalPlayer.playerId == "null")
        {
            var id = new { LocalPlayer.playerId };
            socketManager.EmitEvent(ServerRequestApi.PLAYER_REGISTRATION.ToString(), new (JsonConvert.SerializeObject(id)));
        }
        else
        {
            var id = new { LocalPlayer.playerId };
            socketManager.EmitEvent(ServerRequestApi.PLAYER_REGISTRATION.ToString(),(JsonConvert.SerializeObject(id)));
        }
        LocalPlayer.LoadGame();


    }

    private void OnApplicationQuit()
    {
        LocalPlayer.SaveGame();
        ServerRequest.instance.QuitGame();
    }
}
