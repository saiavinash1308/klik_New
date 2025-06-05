using System.Collections.Generic;
using System;
using TMPro; // If using TextMeshPro
using UnityEngine;
using UnityEngine.UI; // If using Unity UI Text

public class TwoPlayers : OnlinePlayers
{
    public static TwoPlayers instance;
    private SocketManager socketManager;
    [SerializeField]
    private OnlinePlayersProfileManager onlinePlayersProfileManager;

    private static string socketId;

    // ✅ Add these UI references
    [SerializeField] private TextMeshProUGUI myUsernameText;
    [SerializeField] private TextMeshProUGUI opponentUsernameText;

    [SerializeField] private GameObject Oponent1;
    [SerializeField] private GameObject Oponent2;

    private void Awake()
    {
        instance = this;
        socketManager = FindObjectOfType<SocketManager>();
    }

    private void Start()
    {
        if (socketManager == null) return;
        socketId = socketManager.getMySocketId();
        DisableOthers();
    }

    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, string> profiles)
    {
        foreach (var id in playersId)
        {
            if (TempOnlinePlayersData.instance.HasPlayer(id))
            {
                Debug.LogWarning($"Player with socketId {id} already registered. Skipping.");
                continue;
            }

            if (!profiles.ContainsKey(id) || profiles[id] == null)
            {
                Debug.LogError($"Profile for playerId {id} is missing or null");
                continue;
            }

            if (PlayerInfo.instance == null)
            {
                Debug.LogError("PlayerInfo.instance is null");
                continue;
            }

            if (onlinePlayersProfileManager == null)
            {
                Debug.LogError("onlinePlayersProfileManager is null");
                continue;
            }

            if (id == socketId)
            {
                TempOnlinePlayersData.instance.AddPlayer(id, PlayerInfo.instance.selectedPawn);
                if (myUsernameText != null)
                    myUsernameText.text = profiles[id]; // ✅ Set your username
            }
            else
            {
                PawnType opponentPawnType = (PawnType)GetOpponentPawnColour();
                TempOnlinePlayersData.instance.AddPlayer(id, opponentPawnType);
                if (opponentUsernameText != null)
                    opponentUsernameText.text = profiles[id]; // ✅ Set opponent username
            }
        }
    }

    private int GetOpponentPawnColour()
    {
        int[] pawns = { 1, 2, 3, 4 };
        for (int i = 0; i < pawns.Length; i++)
        {
            if (pawns[i] == (int)PlayerInfo.instance.selectedPawn)
            {
                int opponentPawnColour = 2 + i < pawns.Length ? pawns[2 + i] : pawns[Math.Abs(pawns.Length - (2 + i))];
                return opponentPawnColour;
            }
        }
        return 0;
    }

    private void DisableOthers()
    {
        Oponent1.SetActive(false);
        Oponent2.SetActive(false);
    }
}
