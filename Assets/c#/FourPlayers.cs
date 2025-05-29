using System.Collections.Generic;

public class FourPlayers : OnlinePlayers
{
    public static FourPlayers instance;

    private void Awake()
    {
        instance = this;
    }


    //public override void PawnTypeAssignerToPlayerId(List<string> playersId,Dictionary<string,string> profiles)
    //{
    //    TempOnlinePlayersData.instance.AddPlayer(LocalPlayer.playerId, PlayerInfo.instance.selectedPawn);
    //    int thisPlayerIdIndex = playersId.FindIndex(x => x == LocalPlayer.playerId);
    //    int pawnNo = (int)PlayerInfo.instance.selectedPawn;


    //    //this will assign pawnColour to otherplayerIds
    //    for (int i = thisPlayerIdIndex+1; i < playersId.Count; i++)
    //    {
    //        pawnNo = GetOpponentPawnColour(pawnNo);
    //        TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);
    //        onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
    //    }

    //    for (int i = 0; i < thisPlayerIdIndex ; i++)
    //    {
    //        pawnNo = GetOpponentPawnColour(pawnNo);
    //        TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);
    //        onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
    //    }

    //}
    public override void PawnTypeAssignerToPlayerId(List<string> playersId, Dictionary<string, string> profiles)
    {
        if (!TempOnlinePlayersData.instance.HasPlayer(LocalPlayer.playerId))
        {
            TempOnlinePlayersData.instance.AddPlayer(LocalPlayer.playerId, PlayerInfo.instance.selectedPawn);
        }

        int thisPlayerIdIndex = playersId.FindIndex(x => x == LocalPlayer.playerId);
        int pawnNo = (int)PlayerInfo.instance.selectedPawn;

        // Assign pawnColour to other playerIds after thisPlayerIdIndex
        for (int i = thisPlayerIdIndex + 1; i < playersId.Count; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);

            if (!TempOnlinePlayersData.instance.HasPlayer(playersId[i]))
            {
                TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);
                //onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
            }
        }

        // Assign pawnColour to other playerIds before thisPlayerIdIndex
        for (int i = 0; i < thisPlayerIdIndex; i++)
        {
            pawnNo = GetOpponentPawnColour(pawnNo);

            if (!TempOnlinePlayersData.instance.HasPlayer(playersId[i]))
            {
                TempOnlinePlayersData.instance.AddPlayer(playersId[i], (PawnType)pawnNo);
                //onlinePlayersProfileManager.SetPlayersProfile((PawnType)pawnNo, profiles[playersId[i]], playersId[i]);
            }
        }
    }


    //private int GetOpponentPawnColour(int currentPawnNo)
    //{
    //    int[] pawns = { 1, 2, 3, 4 };
    //    int nextPawn = 0;

    //    for (int i = 0; i < pawns.Length; i++)
    //    {
    //        if (currentPawnNo == 4)
    //        {
    //            nextPawn = 1;
    //            break;
    //        }
    //        if (pawns[i] == currentPawnNo)
    //        {
    //            nextPawn = pawns[i + 1];
    //            break;
    //        }
    //    }

    //    return nextPawn;
    //}
    private int GetOpponentPawnColour(int currentPawnNo)
    {
        int nextPawn = currentPawnNo + 1;
        if (nextPawn > 4)
        {
            nextPawn = 1; // wrap around to 1 if exceeds 4
        }
        return nextPawn;
    }

}
