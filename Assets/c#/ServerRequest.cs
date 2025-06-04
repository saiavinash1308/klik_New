using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Sockets;

public class ServerRequest : MonoBehaviour
{
    public static ServerRequest instance;
    public bool serverConnection = false;
    private SocketManager socket;

    private void Awake()
    {
        instance = this;
        socket = FindObjectOfType<SocketManager>();
    }
    private void Start()
    {
        if (socket == null)
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }
    }

    //public void OnGameStarted()
    //{
    //    if (serverConnection) return;
    //    var playerId = new { LocalPlayer.playerId };
    //    socket.EmitEvent(ServerRequestApi.ON_GAME_STARTED.ToString(), JsonConvert.SerializeObject(playerId));
    //}

    public void RollDice(int diceValue, PawnType pawn)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            print("RollDice Emit");
        if (serverConnection) return;
        if (diceValue == 6)
        {
                print("Emitted 6");
                var data = new { diceValue };
            socket.EmitEvent(ServerRequestApi.ROLL_DICE.ToString(), JsonConvert.SerializeObject(data));
        }
        else if (AvoidSwitchingPlayers(diceValue, pawn))
        {
                print("Emit !6");
            var diceWithPlayers = new { diceValue, pawn, PlayerInfo.instance.players };
            socket.EmitEvent(ServerRequestApi.ROLL_DICE.ToString(), JsonConvert.SerializeObject(diceWithPlayers));
        }
        else
        {
                print("Switched Called");
            SwitchPlayers(diceValue);
        }
        });
    }

    private void SwitchPlayers(int diceValue)
    {
        if (serverConnection) return;
        var dice = new { diceValue, LocalPlayer.playerId, PlayerInfo.instance.players };
        socket.EmitEvent(ServerRequestApi.SWITCH_PLAYER.ToString(), JsonConvert.SerializeObject(dice));
    }

    private bool AvoidSwitchingPlayers(int diceValue, PawnType currentPawn)
    {
        bool switchPawns = false;

        foreach (var Player in PlayerInfo.instance.pawnInstances)
        {
            if (Player.pawnType != currentPawn) continue;

            if (Player.isLeftTheHouse)
            {
                bool canMoveAhead = Player.spotIndexOnLudoBoard + diceValue <= Player.Lastspot;
                if (canMoveAhead)
                {
                    switchPawns = false;
                    break;
                }
            }
            else
            {
                switchPawns = true;
            }
        }
        return !switchPawns;
    }

    public void PlayerFinishedMoving(bool richedTheDestination)
    {
        if (serverConnection) return;
        int diceValue = DiceController.instance.currentDiceValue;
        var pawndetails = new { diceValue, LocalPlayer.playerId, richedTheDestination };
        socket.EmitEvent(ServerRequestApi.PLAYER_FINISHED_MOVING.ToString(), JsonConvert.SerializeObject(pawndetails));
    }

    //public void MatchMaking(int players, PawnType pawnType)
    //{
    //    if (serverConnection) return;
    //    var generalInfo = new { players, LocalPlayer.playerId, LocalPlayer.profilePic };
    //    socket.EmitEvent(ServerRequestApi.MATCH_MAKING.ToString(), JsonConvert.SerializeObject(generalInfo));
    //}

    public void MatchMaking()
    {
        if (serverConnection) return;
        socket.EmitEvent(ServerRequestApi.MATCH_MAKING.ToString(), " ");
    }

    public void ExitRoom()
    {
        if (serverConnection) return;
        var generalInfo = new { LocalPlayer.playerId };
        socket.EmitEvent(ServerRequestApi.EXIT_ROOM.ToString(), JsonConvert.SerializeObject(generalInfo));
    }

    public void QuitGame()
    {
        if (serverConnection) return;
        var quit = new { quit = true, PlayerInfo.instance.players, LocalPlayer.playerId };
        socket.EmitEvent(ServerRequestApi.ON_QUIT.ToString(), JsonConvert.SerializeObject(quit));
    }

    public void ThisPlayerWins()
    {
        if (serverConnection) return;
        var winner = new { LocalPlayer.playerId };
        socket.EmitEvent(ServerRequestApi.ON_PLAYER_WIN.ToString(), JsonConvert.SerializeObject(winner));
    }

    public void MovePlayer(int diceValue, int pawnNo, int pawnType)
    {
        print("Move Player Emitted");
        if (serverConnection) return;
        var generalInfo = new { diceValue, pawnNo, LocalPlayer.playerId };
        socket.EmitEvent(ServerRequestApi.MOVE_PLAYER.ToString(), JsonConvert.SerializeObject(generalInfo));
    }

    public void OnlinePlayers()
    {
        if (serverConnection) return;
        socket.EmitEvent(ServerReponseApi.ONLINE_PLAYERS.ToString(), "");
    }

    public void OnPlayerWins(PawnType winnerPawn)
    {
        if (serverConnection) return;
        var winner = new { winnerPawn };
        socket.EmitEvent(ServerReponseApi.ON_PLAYER_WIN.ToString(), JsonConvert.SerializeObject(winner));
    }
}
