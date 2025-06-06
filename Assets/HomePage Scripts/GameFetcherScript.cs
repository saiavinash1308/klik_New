using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GameFetcherScript : MonoBehaviour
{
    public GameObject gameDataPrefab;
    public Transform contentPanel1;
    public Transform contentPanel2;
    public Transform contentPanel3;

    public GameObject popupUI;
    public TMP_Text popupMessageText; 
    public Button confirmButton; 
    public Button cancelButton;
    public TextMeshProUGUI statusMessageText;
    private const string baseUrl = "https://backend-klik.fivlog.space/api/game/fetchGame/";
    //private const string baseUrl = "http://localhost:3001/api/game/fetchGame/";
    public HomePageManager HomePageManager;
    [SerializeField]
    private SocketManager socketManager;
    //private int currentWalletBalance;
    public class Game
    {
        public string gameId;
        public string gameType;
        public int maxPlayers;
        public int entryFee;
        public int prizePool;
        public string currency;
        public bool isActive;
    }

    [System.Serializable]
    public class GameResponse
    {
        public List<Game> games;
    }
    private void Awake()
    {
        socketManager = FindObjectOfType<SocketManager>();
    }

    void Start()
    {
        
        if (socketManager == null)
        {
            Logger.LogError("Network error. Please try again.");
            return; 
        }

        //currentWalletBalance = int.Parse(HomePageManager.totalAmount.text);

        confirmButton.onClick.AddListener(OnConfirmDeduction);
        cancelButton.onClick.AddListener(OnCancelDeduction);

        popupUI.SetActive(false);
    }

    internal IEnumerator GetGameData(string gameType)
    {
        string url = $"{baseUrl}{gameType}";

        Logger.Log($"Sending GET request to: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning($"Error fetching games: {request.error}");
            Logger.LogError("Network error. Please try again.");
            yield break;
        }

        string rawResponse = request.downloadHandler.text;
        Logger.Log($"Backend Response: {rawResponse}");

        if (string.IsNullOrEmpty(rawResponse))
        {
            Logger.LogWarning("Empty response from game fetcher API.");
            yield break;
        }

        GameResponse response = JsonConvert.DeserializeObject<GameResponse>(rawResponse);


        if (response.games == null || response.games.Count == 0)
        {
            Logger.Log("No games available.");
            yield break;
        }

        Logger.Log("Parsed Game Data:");
        foreach (var game in response.games)
        {
            Logger.Log($"Game Name: {game.gameType}, Game Type: {game.gameType}, Max Players: {game.maxPlayers}, Prize Pool: {game.prizePool} {game.currency}");
        }

        PopulatePanel(contentPanel1, response.games, "all");
        PopulatePanel(contentPanel2, response.games, "2");
        PopulatePanel(contentPanel3, response.games, "4");

        Canvas.ForceUpdateCanvases();
    }

    private void PopulatePanel(Transform contentPanel, List<Game> games, string playerType)
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        List<Game> filteredGames = new List<Game>();

        if (playerType == "all")
        {
            filteredGames = games;
        }
        else if (playerType == "2")
        {
            filteredGames = games.FindAll(game => game.maxPlayers == 2);
        }
        else if (playerType == "4")
        {
            filteredGames = games.FindAll(game => game.maxPlayers == 4);
        }

        foreach (var game in filteredGames)
        {
            GameObject gameDataUI = Instantiate(gameDataPrefab, contentPanel);

            TMP_Text[] textComponents = gameDataUI.GetComponentsInChildren<TMP_Text>();

            textComponents[0].text = game.gameType;
            textComponents[1].text = $"Max Players: {game.maxPlayers}";
            textComponents[2].text = $" {game.prizePool} {game.currency}";

            Button button = gameDataUI.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => OnGameButtonClicked(game));
        }
    }

    private void OnGameButtonClicked(Game game)
    {
        Logger.Log($"Button clicked for game: {game.gameType} (ID: {game.gameId})");

        popupMessageText.text = $"Are you sure you want to play {game.gameType}? Amount will be deducted: {game.entryFee} {game.currency}.";
        popupUI.SetActive(true);

        selectedGame = game;
    }

    private Game selectedGame;

    private void OnConfirmDeduction()
    {
        popupUI.SetActive(false);
        Logger.Log($"Emitting INIT_GAME event with gameId: {selectedGame.gameId}");
        socketManager.EmitEvent("INIT_GAME", selectedGame.gameId);
        //string sceneToLoad = "";
        if (selectedGame.gameType == "MEMORYGAME")
        {
            if (selectedGame.maxPlayers == 2)
            {
                SceneManager.LoadScene ("MindMorgaLoadingScene");         
            }

            HomePageManager.UpdateTotalAmount(-selectedGame.entryFee);
            Logger.Log("Deducted:" + selectedGame.entryFee);
        }
        else if (selectedGame.gameType == "LUDO")
        {
            if (selectedGame.maxPlayers == 2)
            {
                //sceneToLoad = "LudoGame";
                GameSettings.players = 2;
                SceneManager.LoadScene("LudoGame");
                //uiManager.OnClick("playerNo2");
                //StartCoroutine(LoadSceneAndCallUI("LudoGame", "playerNo2", "red"));

            }
            else if (selectedGame.maxPlayers == 4)
            {
                GameSettings.players = 4;
                SceneManager.LoadScene("LudoGame");
                //sceneToLoad = "LudoGame";
                //uiManager.OnClick("playerNo4");
                //StartCoroutine(LoadSceneAndCallUI("LudoGame", "playerNo4", "red"));

            }
            //else if (selectedGame.entryFee > currentWalletBalance)
            //{
            //    ShowStatusMessage("Insufficient balance");
            //    return;
            //}
            HomePageManager.UpdateTotalAmount(-selectedGame.entryFee);
            //uiManager.OnClick("online");
        }
        else
        {
            ShowStatusMessage("Game temporarily unavailable");
        }
    }

    public static class GameSettings
    {
        public static int players;
    }

    void ShowStatusMessage(string message)
    {
        statusMessageText.text = message;
        statusMessageText.gameObject.SetActive(true);
    }



    private void OnCancelDeduction()
    {
        popupUI.SetActive(false);
    }
}
