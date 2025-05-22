
using System;
using System.Collections.Generic;
using UnityEngine;

class AnalyseAndRegisterOnlinePlayers : MonoBehaviour
{

    public static AnalyseAndRegisterOnlinePlayers instance;
    private SocketManager socketManager;
    private static string socketId;
    private void Awake()
    {
        socketManager = FindObjectOfType<SocketManager>();
        instance = this;
    }

    private void Start()
    {
        if (socketManager == null)
        {
            Debug.LogError("SocketManager not found!");
            return;
        }

        socketId = socketManager.getMySocketId();
        print(socketId);
    }

    public void AnaliysisAndRegistration(List<string> playersId, Dictionary<string, string> profiles)
    {
        foreach (var item in playersId)
        {
            print(item);
        }
        int thisPlayerIdInTheListIndex = 0;
        if (playersId.Contains(socketId))
        {
            thisPlayerIdInTheListIndex = playersId.FindIndex(x => x == socketId);
            print("Player Index:"+ thisPlayerIdInTheListIndex);
        }
        else
        {
            print("player id not in the list");
            print("something went wrong try again");
            UiManager.instance.ResetLevel();
            return;
        }

        Registertion(thisPlayerIdInTheListIndex, playersId,profiles);
    }

   
    private void Registertion(int thisPlayerListIndex,List<string> playersId, Dictionary<string, string> profiles)
    {
        print("Registration");
        switch (PlayerInfo.instance.players)
        {
            case 2:
                if (TwoPlayers.instance == null)
                {
                    Debug.LogError("TwoPlayers.instance is null");
                }
                else
                {
                    TwoPlayers.instance.PawnTypeAssignerToPlayerId(playersId, profiles);
                    print("Case 2 Called");
                }
                break;

            case 4:
                if (FourPlayers.instance == null)
                {
                    Debug.LogError("FourPlayers.instance is null");
                }
                else
                {
                    FourPlayers.instance.PawnTypeAssignerToPlayerId(playersId, profiles);
                }
                break;

        }
        StartGame(playersId);

    }

    private void StartGame(List<string> playersId)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            print("Start Game");
            UiManager.instance.uiPanel.SetActive(false);
            PawnType currentPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(playersId[0]);
            print("CurrentPawn:" +  currentPawn);   
            DiceController.instance.currentPawn = currentPawn;
            StartCoroutine(PawnTimer.instance.Timer(currentPawn));
            DiceController.instance.UpdateValue();
            UiManager.instance.uiPanel.SetActive(false);
        });
    }

}

