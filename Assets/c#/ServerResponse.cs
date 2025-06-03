using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.UIElements;

public class ServerResponse : MonoBehaviour
{
    public SocketManager socket;
    [SerializeField] private YouWinPanel _youWinPanel;

    private void Awake()
    {
        socket = FindObjectOfType<SocketManager>();
        socket.socket.On(ServerReponseApi.START_GAME.ToString(), StartGame);    
        socket.socket.On(ServerReponseApi.ROLL_DICE.ToString(), RollDice);
        socket.socket.On(ServerReponseApi.ON_PLAYER_WIN.ToString(), OnPlayerWin);
        socket.socket.On(ServerReponseApi.YOU_WIN.ToString(), YouWin);
        socket.socket.On(ServerReponseApi.PLAYER_REGISTRATION.ToString(), RegisterPlayerId);
        socket.socket.On(ServerReponseApi.SWITCH_PLAYER.ToString(), SwitchPlayers);
        socket.socket.On(ServerReponseApi.PLAYER_FINISHED_MOVING.ToString(), PlayerFinishedMoving);
        socket.socket.On(ServerReponseApi.AVOID_SWITCH_PLAYER.ToString(), AvoidSwitchingPlayer);
        socket.socket.On(ServerReponseApi.PLAYER_MOVED.ToString(), PlayerMoved);
        socket.socket.On(ServerReponseApi.ONLINE_PLAYERS.ToString(), OnlinePlayers);
        socket.socket.On(ServerReponseApi.EXIT_ROOM.ToString(), ExitRoom);
        socket.socket.On(ServerReponseApi.GET_PLAYERS_INFO.ToString(), GetPlayerInfo);
    }

    private void Start()
    {
        if (socket == null)
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }
    }

    private JObject ParseData(SocketIOResponse e) => JObject.Parse(e.GetValue<string>());

    private void GetPlayerInfo(SocketIOResponse e)
    {
        try
        {
            JObject data = ParseData(e);
            List<string> videoGames = JsonConvert.DeserializeObject<List<string>>(data["array"].ToString());
            Logger.Log("Games: " + data["array"]);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("GetPlayerInfo Error: " + ex.Message);
        }
    }

    private void StartGame(SocketIOResponse e)
    {
        string responseData = e.GetValue<string>();
        Logger.Log("Game Started Response Data: " + responseData);
        JObject data = ParseData(e);
        try
        {
            JArray usersArray = (JArray) data["users"];
            List<string> allPlayersId = new List<string>();
            Dictionary<string, string> profiles = new Dictionary<string, string>();

            foreach (var user in usersArray)
            {
                string socketId = user["socketId"].ToString();
                string username = user["username"].ToString();

                allPlayersId.Add(socketId);
                profiles[socketId] = username;

                print($"User: {username}, Socket ID: {socketId}");
            }
            MainThreadDispatcher.Enqueue(() =>
            {
                AnalyseAndRegisterOnlinePlayers.instance.AnaliysisAndRegistration(allPlayersId, profiles);
                print("Game started");
            });
            
        }
        catch (Exception error)
        {
            print("Exception in StartGame: " + error);
        }
    }


    //private void SwitchPlayers(SocketIOResponse e)
    //{
    //    string responseData = e.GetValue<string>();
    //    print("Switched Player"+ e);
    //    JObject data = ParseData(e);

    //    string nextPlayerId = JsonConvert.DeserializeObject<string>(data["nextPlayerId"].ToString());
    //    print("NextPlayerId:"+ nextPlayerId);
    //    int diceValue = int.Parse(data["diceValue"].ToString());

    //    PawnType currentPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(nextPlayerId);
    //    Logger.Log("#SwitchPawns: " + currentPawn);

    //    if (PlayerInfo.instance.selectedPawn == currentPawn)
    //        DiceController.instance.playerMovementIsFinished = true;

    //    DiceController.instance.currentPawn = currentPawn;
    //    DiceController.instance.UpdateValue();
    //    StartCoroutine(DiceController.instance.RollDice(diceValue));
    //    StartCoroutine(PawnTimer.instance.Timer(currentPawn));
    //}
    private void SwitchPlayers(SocketIOResponse e)
    {
        // Get the array of arguments passed to the event
        string jsonString = e.GetValue<string>(); // This works if it's a single string in the array
        print("Switched Player: " + jsonString);

        // Parse the JSON string
        JObject data = JObject.Parse(jsonString);

        string nextPlayerId = data["nextPlayerId"].ToString();
        print("NextPlayerId: " + nextPlayerId);

        int diceValue = int.Parse(data["diceValue"].ToString());

        PawnType currentPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(nextPlayerId);
        Logger.Log("#SwitchPawns: " + currentPawn);



        MainThreadDispatcher.Enqueue(() =>
        {
            if (PlayerInfo.instance.selectedPawn == currentPawn)
                DiceController.instance.playerMovementIsFinished = true;
                DiceController.instance.currentPawn = currentPawn;
                DiceController.instance.UpdateValue();
                StartCoroutine(DiceController.instance.RollDice(diceValue));
                StartCoroutine(PawnTimer.instance.Timer(currentPawn));
        });
        
    }


    private void RollDice(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        print("Roll Dice Listner Called");
        JObject data = JObject.Parse(jsonString);
        int diceValue = int.Parse(data["diceValue"].ToString());

        Logger.Log("#RollDice: " + diceValue);
        MainThreadDispatcher.Enqueue(() =>
        {
            StartCoroutine(DiceController.instance.RollDice(diceValue));
        });
       
    }

    private void OnPlayerWin(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        JObject data = JObject.Parse(jsonString);
        string winnerId = data["PlayerId"].ToString();
        MainThreadDispatcher.Enqueue(() =>
        {
            PawnType winnerPawnType = TempOnlinePlayersData.instance.GetPlayerPawnType(winnerId);
            SetWinner.instance.OnPlayerWin(winnerPawnType);
            TempOnlinePlayersData.instance.RemovePlayer(winnerId);
        });
        
    }

    private void AvoidSwitchingPlayer(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        JObject data = JObject.Parse(jsonString);
        int diceValue = int.Parse(data["diceValue"].ToString());
        MainThreadDispatcher.Enqueue(() =>
        {
            StartCoroutine(DiceController.instance.RollDice(diceValue));
        });
    }

    private void PlayerFinishedMoving(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        JObject data = JObject.Parse(jsonString);

        string nextPlayerId = data["nextPlayerId"].ToString();
        int diceValue = int.Parse(data["diceValue"].ToString());

        PawnType currentPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(nextPlayerId);
        Logger.Log("#PawnFinishedMoving: " + currentPawn);

        MainThreadDispatcher.Enqueue(() =>
        {
            if (PlayerInfo.instance.selectedPawn == currentPawn)
                DiceController.instance.playerMovementIsFinished = true;

            DiceController.instance.currentPawn = currentPawn;
            DiceController.instance.UpdateValue();

            StartCoroutine(PawnTimer.instance.Timer(currentPawn));
            
        });
    }

    private void RegisterPlayerId(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        JObject data = JObject.Parse(jsonString);
        LocalPlayer.playerId = data["id"].ToString();

        UiManager.instance.UpdateUi();
        LocalPlayer.SaveGame();
        Logger.Log("Registered PlayerId: " + LocalPlayer.playerId);
    }

    private void ExitRoom(SocketIOResponse e)
    {
        try
        {
            JObject data = ParseData(e);
            string playerId = JsonConvert.DeserializeObject<string>(data["playerId"].ToString());

            PawnType exitPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(playerId);
            Logger.Log($"Exit triggered by {playerId} | Pawn: {exitPawn}");

            PlayerInfo.instance.RemovePawn(exitPawn);
            UiManager.instance.ExitPanel(exitPawn);
            TempOnlinePlayersData.instance.RemovePlayer(playerId);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("ExitRoom Error: " + ex.Message);
        }
    }

    private void YouWin(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        print("Player Moved" + jsonString);
        try
        {
            JObject data = JObject.Parse(jsonString);
            string playerId = data["playerId"].ToString();
            MainThreadDispatcher.Enqueue(() =>
            {
                    PawnType exitPawn = TempOnlinePlayersData.instance.GetPlayerPawnType(playerId);
                _youWinPanel.OnPlayerWin(exitPawn);

                PlayerInfo.instance.RemovePawn(exitPawn);
                UiManager.instance.ExitPanel(exitPawn);
                TempOnlinePlayersData.instance.RemovePlayer(playerId);
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning("YouWin Error: " + ex.Message); 
        }
    }

    private void PlayerMoved(SocketIOResponse e)
    {
        string jsonString = e.GetValue<string>();
        print("Player Moved" + jsonString);
        JObject data = JObject.Parse(jsonString);

        int diceValue = int.Parse(data["diceValue"].ToString());
        int pawnNo = int.Parse(data["pawnNo"].ToString());
        string playerId = data["playerId"].ToString();

        MainThreadDispatcher.Enqueue(() =>
        {
                PawnType pawnType = TempOnlinePlayersData.instance.GetPlayerPawnType(playerId);
            Logger.Log($"Pawn Movement | Dice: {diceValue}, PawnNo: {pawnNo}");

            FindAndMoveThePawn(diceValue, pawnNo, pawnType);
        });
    }

    private void FindAndMoveThePawn(int diceValue, int pawnNo, PawnType pawnType)
    {
        print("Piece moved");
        foreach (var pawn in PlayerInfo.instance.pawnInstances)
        {
            if (pawn.pawnType == pawnType && pawn.pawnNumber == pawnNo)
            {
                print("Pawn Coroutine called");
                MainThreadDispatcher.Enqueue(() =>
                {
                    StartCoroutine(pawn.MoveTo(diceValue, true));
                });
            }
        }
    }

    private void OnlinePlayers(SocketIOResponse e)
    {
        JObject data = ParseData(e);
        int players = int.Parse(data["onlinePlayers"].ToString());

        UiManager.instance.onlinePlayers.text = players.ToString();
    }
}
