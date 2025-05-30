using System;
using UnityEngine;
using UnityEngine.UI;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance { get; private set; }
    public SocketIOUnity socket;
    public Text statusText;
    internal bool isConnected = false;
    private bool hasEmittedAddUser = false;
    [SerializeField]
    private MindMorgaGameController MindMorgaGameController;
    private string roomId;
    private string socketId;
    private int steps;
    private User[] users;

    internal bool stopSearch = true;

    private float prizePool;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSocket();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator EmitAddUserEventWhenConnected()
    {
        while (!isConnected)
        {
            yield return null; 
        }

        string authToken = PlayerPrefs.GetString("AuthToken", null);
        if (!string.IsNullOrEmpty(authToken) && !hasEmittedAddUser)
        {
            Debug.Log("AuthToken: " + authToken);
            EmitEvent("ADD_USER", authToken);
            hasEmittedAddUser = true; 
        }
    }

    private void Start()
    {
        StartCoroutine(EmitAddUserEventWhenConnected());
    }

    internal void InitializeSocket()
    {
        //var url = "http://localhost:3000/";
        var url = "https://sockets-klik.fivlog.space/";
        var uri = new Uri(url);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.OnConnected += OnConnected;
        // socket.OnDisconnected += OnDisconnected; // Register for disconnection

        Debug.Log("Connecting to server...");
        socket.Connect();
    }

    internal void OnConnected(object sender, EventArgs e)
    {
        isConnected = true;
        Debug.Log("Connected to server.");
        AddListeners();
        EmitAddUserIfNecessary();
    }

    private void EmitAddUserIfNecessary()
    {
        string authToken = PlayerPrefs.GetString("AuthToken", null);
        if (!string.IsNullOrEmpty(authToken) && !hasEmittedAddUser)
        {
            Debug.Log("AuthToken: " + authToken);
            EmitEvent("ADD_USER", authToken);
            hasEmittedAddUser = true; // Ensure it only emits once
        }
    }

    private void OnDisconnected(object sender, EventArgs e)
    {
        isConnected = false;
        Debug.Log("Disconnected from server.");
        hasEmittedAddUser = false; // Reset flag on disconnection
    }

    internal void EmitEvent(string eventName, string data)
    {
        if (isConnected)
        {
            socket.Emit(eventName, data);
        }
        else
        {
            Debug.LogWarning("Attempted to emit event while disconnected.");
        }
    }

    internal void AddListeners()
    {

        socket.On("STOP_SEARCH", OnStopSearch);
        
        //MIND_MORGA

        socket.On("START_MEMORY_GAME", MindMorgaGameStarted);
        socket.On("MEMORY_GAME_CURRENT_TURN", MindMorgaOnPlayerTurn);
        socket.On("OPEN_CARD", OpenCard);
        socket.On("CLOSE_CARDS", CloseCard);
        socket.On("CARDS_MATCHED", CardsMatched);
        socket.On("END_GAME", EndGame);
    }

    public void OnStopSearch(SocketIOResponse res)
    {
        string stopData = res.GetValue<string>();
        Debug.Log("Stop Searching" + stopData);
        stopSearch = false;
    }

    internal string getMySocketId()
    {
        return this.socket.Id;
    }

    // MIND_MORGA

    public void MindMorgaGameStarted(SocketIOResponse res)
    {
        string responseData = res.GetValue<string>();
        Debug.Log("Game Started Response Data: " + responseData);
        Debug.Log("My Socket Id " + socket.Id);

        GameStartData gameStartData;
        try
        {
            gameStartData = JsonConvert.DeserializeObject<GameStartData>(responseData);
            if (gameStartData == null)
            {
                Debug.LogError("GameStartData is null after deserialization.");
                return;
            }

            if (gameStartData.roomId == null)
            {
                Debug.LogError("No roomId found");
                return;
            }

            if (gameStartData.users == null)
            {
                Debug.LogError("Users array is null.");
                return;
            }
            roomId = gameStartData.roomId;
            prizePool = gameStartData.prizePool;
            Debug.Log($"Number of users: {gameStartData.users.Length}");
            users = new User[gameStartData.users.Length];
            for (int i = 0; i < gameStartData.users.Length; i++)
            {
                Debug.Log($"User {i}: Socket ID = {gameStartData.users[i]}");
                User user;
                string socketId = gameStartData.users[i].socketId;
                string username = gameStartData.users[i].username;
                if (socket.Id != socketId)
                {
                    user = new User(socketId, username);
                }
                else
                {
                    user = new User(gameStartData.users[i].socketId, username, true);
                }
                Debug.Log("socketId " + user.socketId);
                users[i] = user;
                Debug.Log("User pushed to array");
            }

            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("MindMorgaGameManager " + MindMorgaGameController.Mindgame);
                Debug.Log("Starting player initialization...");
                MindMorgaGameController.Mindgame.InitializePlayers(users);
            });
            Debug.Log("Player initialization completed.");


        }
        catch (JsonException jsonEx)
        {
            Debug.LogError("JSON Parsing Error: " + jsonEx.Message);
            return;
        }
    }

    public void MindMorgaOnPlayerTurn(SocketIOResponse res)
    {
        string socketId = res.GetValue<string>();

        Debug.Log("Received Player Turn for socketId: " + socketId);


        MainThreadDispatcher.Enqueue(() =>
        {
            MindMorgaGameController.Mindgame.HandlePlayerTurn(socketId);
        });
    }

    public void OpenCard(SocketIOResponse res)
    {
        string cardDataJson = res.GetValue<string>();
        Debug.Log("Card data received: " + cardDataJson);

        var jsonData = JsonConvert.DeserializeObject<CardData>(cardDataJson);

        if (jsonData != null)
        {
            SetCardData(jsonData.index, jsonData.card);
            Debug.Log($"Stored card data: Index = {jsonData.index}, Card = {jsonData.card}");

            MainThreadDispatcher.Enqueue(() =>
            {
                MindMorgaGameController.Mindgame.LoadCardSprite();
                Debug.Log("Card data stored and displayed.");
            });
        }
        else
        {
            Debug.LogError("Failed to deserialize card data.");
        }
    }


    public void CloseCard(SocketIOResponse res)
    {
        string cardData = res.GetValue<string>();
        Debug.Log("Close card data received: " + cardData);
        var jsonData = JsonUtility.FromJson<CloseCardsData>(cardData.ToString());
        MainThreadDispatcher.Enqueue(() =>
        {
            MindMorgaGameController.Mindgame.CloseCardSprite(jsonData.index1, jsonData.index2);
            Debug.Log("Closing card data Received");
        });
    }

    public void CardsMatched(SocketIOResponse res)
    {
        string cardData = res.GetValue<string>();
        Debug.Log("Card matched data received: " + cardData);
        var jsonData = JsonUtility.FromJson<MatchCardsData>(cardData.ToString());
        MainThreadDispatcher.Enqueue(() =>
        {
            MindMorgaGameController.Mindgame.DisableMatchedCards(jsonData.index1, jsonData.index2, jsonData.score1, jsonData.score2);
            Debug.Log("Match card data Received");
        });
    }

    public void EndGame(SocketIOResponse res)
    {
        string cardData = res.GetValue<string>();
        Debug.Log("Winner data received: " + cardData);
        var jsonData = JsonUtility.FromJson<EndCardsData>(cardData.ToString());
        MainThreadDispatcher.Enqueue(() =>
        {
            MindMorgaGameController.Mindgame.EndGame(jsonData.winnerId, jsonData.score1, jsonData.score2);
            Debug.Log("Match card data Received");
        });
    }

    [System.Serializable]
    public class CardData
    {
        public int index;
        public string card;

        public CardData(int index, string cardName)
        {
            this.index = index;
            this.card = cardName;
        }
    }


    [System.Serializable]
    public class CloseCardsData
    {
        public int index1;  
        public int index2; 
    }

    [System.Serializable]
    public class MatchCardsData
    {
        public int index1;  
        public int index2;  
        public int score1;  
        public int score2;  
    }

    [System.Serializable]
    public class EndCardsData
    {
        public string winnerId;
        public int score1;  
        public int score2; 
    }


    public string GetRoomId()
    {
        return roomId;
    }

    internal int diceValue;

    public void SetDiceValue(int value)
    {
        diceValue = value;
    }

    public int GetDiceValue()
    {
        return diceValue;
    }

    private CardData storedCardData;

    public void SetCardData(int index, string name)
    {
        storedCardData = new CardData(index, name);
    }

    public CardData GetCardData()
    {
        return storedCardData;
    }




    public int GetSteps()
    {
        return steps;
    }



    public string GetSocketId()
    {
        return socketId;
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
        }
    }
    public User[] getUsers()
    {
        return this.users;
    }

    public float getPrizePool()
    {
        return this.prizePool;
    }


    


    [Serializable]
    public class GameStartData
    {
        public string roomId;
        public User[] users;
        public float prizePool;
    }

    [Serializable]
    public class User
    {
        public string socketId;
        public string username;
        public bool isCurrent;

        public User(string socketId,string username, bool isCurrent = false)
        {
            this.socketId = socketId;
            this.username = username;
            this.isCurrent = isCurrent;
        }

        public string getSocketId()
        {
            return this.socketId;
        }

    }

}