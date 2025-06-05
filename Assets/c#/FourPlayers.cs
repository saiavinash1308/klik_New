using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FourPlayers : OnlinePlayers
{
    public static FourPlayers instance;

    [SerializeField] private OnlinePlayersProfileManager onlinePlayersProfileManager;

    // ✅ Add 4 username text boxes
    [SerializeField] private TextMeshProUGUI player1UsernameText;
    [SerializeField] private TextMeshProUGUI player2UsernameText;
    [SerializeField] private TextMeshProUGUI player3UsernameText;
    [SerializeField] private TextMeshProUGUI player4UsernameText;

    private void Awake()
    {
        instance = this;
    }

    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, string> profiles)
    {
        if (!TempOnlinePlayersData.instance.HasPlayer(LocalPlayer.playerId))
        {
            TempOnlinePlayersData.instance.AddPlayer(LocalPlayer.playerId, PlayerInfo.instance.selectedPawn);
        }

        int thisPlayerIdIndex = playersId.FindIndex(x => x == LocalPlayer.playerId);
        int pawnNo = (int)PlayerInfo.instance.selectedPawn;

        // ✅ Track pawnType → text box mapping
        Dictionary<int, TextMeshProUGUI> pawnToTextBox = new Dictionary<int, TextMeshProUGUI>
        {
            { 1, player1UsernameText },
            { 2, player2UsernameText },
            { 3, player3UsernameText },
            { 4, player4UsernameText }
        };

        // ✅ Set your own username
        if (profiles.ContainsKey(LocalPlayer.playerId) && pawnToTextBox.ContainsKey(pawnNo))
        {
            pawnToTextBox[pawnNo].text = profiles[LocalPlayer.playerId];
        }

        // Assign pawnColours to other players after thisPlayerIdIndex
        for (int i = thisPlayerIdIndex + 1; i < playersId.Count; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);

            if (!TempOnlinePlayersData.instance.HasPlayer(playersId[i]))
            {
                TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);

                if (profiles.ContainsKey(playersId[i]) && pawnToTextBox.ContainsKey(pawnNo))
                {
                    pawnToTextBox[pawnNo].text = profiles[playersId[i]];
                }
            }
        }

        // Assign pawnColours to other players before thisPlayerIdIndex
        for (int i = 0; i < thisPlayerIdIndex; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);

            if (!TempOnlinePlayersData.instance.HasPlayer(playersId[i]))
            {
                TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);

                if (profiles.ContainsKey(playersId[i]) && pawnToTextBox.ContainsKey(pawnNo))
                {
                    pawnToTextBox[pawnNo].text = profiles[playersId[i]];
                }
            }
        }
    }

    private int GetOpponentPawnColour(int currentPawnNo)
    {
        int nextPawn = currentPawnNo + 1;
        if (nextPawn > 4)
        {
            nextPawn = 1;
        }
        return nextPawn;
    }
}
