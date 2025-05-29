
using System;
using System.Collections.Generic;
using UnityEngine;

public class TwoPlayers : OnlinePlayers
{
    public static TwoPlayers instance;
    private SocketManager socketManager;
    [SerializeField]
    private OnlinePlayersProfileManager onlinePlayersProfileManager;

    private static string socketId;
    private void Awake()
    {
        instance = this;
        socketManager = FindObjectOfType<SocketManager>();
    }

    private void Start()
    {
        if (socketManager == null)
        {
            //Debug.LogError("SocketManager not found!");
            return;
        }
        socketId = socketManager.getMySocketId();
    }
    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, string> profiles)
    {
        //foreach (var id in playersId)
        //{
        //    if (TempOnlinePlayersData.instance.HasPlayer(id))
        //    {
        //        // Already added, skip to avoid crash
        //        UnityEngine.Debug.LogWarning($"Player with socketId {id} already registered. Skipping.");
        //        continue;
        //    }

        //    if (id == socketId)
        //    {
        //        TempOnlinePlayersData.instance.AddPlayer(id, PlayerInfo.instance.selectedPawn);
        //    }
        //    else
        //    {
        //        PawnType opponentPawnType = (PawnType)GetOpponentPawnColour();
        //        TempOnlinePlayersData.instance.AddPlayer(id, opponentPawnType);
        //    }
        //}
        foreach (var id in playersId)
        {
            if (TempOnlinePlayersData.instance.HasPlayer(id))
            {
                UnityEngine.Debug.LogWarning($"Player with socketId {id} already registered. Skipping.");
                continue;
            }

            if (!profiles.ContainsKey(id) || profiles[id] == null)
            {
                UnityEngine.Debug.LogError($"Profile for playerId {id} is missing or null");
                continue;
            }

            if (PlayerInfo.instance == null)
            {
                UnityEngine.Debug.LogError("PlayerInfo.instance is null");
                continue;
            }

            if (onlinePlayersProfileManager == null)
            {
                UnityEngine.Debug.LogError("onlinePlayersProfileManager is null");
                continue;
            }

            if (id == socketId)
            {
                TempOnlinePlayersData.instance.AddPlayer(id, PlayerInfo.instance.selectedPawn);
                //onlinePlayersProfileManager.SetPlayersProfile(PlayerInfo.instance.selectedPawn, profiles[id], id);
            }
            else
            {
                PawnType opponentPawnType = (PawnType)GetOpponentPawnColour();
                TempOnlinePlayersData.instance.AddPlayer(id, opponentPawnType);
                //onlinePlayersProfileManager.SetPlayersProfile(PlayerInfo.instance.selectedPawn, profiles[id], id);
            }
        }

    }



    private int GetOpponentPawnColour()
    {
        int[] pawns = { 1, 2, 3, 4 };
        for (int i = 0; i < pawns.Length; i++)
        {
            bool isSameColour = pawns[i] == (int)PlayerInfo.instance.selectedPawn;
            if (isSameColour)
            {
                int opponentPawnColour = 2 + pawns[i] <= pawns.Length ? pawns[2 + i] : pawns[Math.Abs(pawns.Length - (2 + i))];
                return opponentPawnColour;
            }
        }
        return 0;
    }
}
