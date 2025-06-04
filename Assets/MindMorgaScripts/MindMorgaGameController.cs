using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SocketManager;

public class MindMorgaGameController : MonoBehaviour
{
    public static MindMorgaGameController Mindgame { get; private set; }

    [SerializeField]
    private Sprite bgImage;

    public Sprite[] images;  
    public List<Button> btns = new List<Button>();



    public Text player1Score;
    public Text player2Score;

    public Text player1FinalScore;
    public Text player2FinalScore;

    public Text Player1UserName;
    public Text Player2UserName;
    public Text prizePool;

    public GameObject WinnerPopUp;
    public GameObject LoosePopUp;

    [SerializeField] private RectTransform arrowPlayer1;
    [SerializeField] private RectTransform arrowPlayer2;
    private Vector3 originalScale = Vector3.one;
    private Vector3 enlargedScale = new Vector3(1.5f, 1.5f, 1.5f);

    public User[] players;
    public static string currentTurnSocketId;
    public string turnSocketId;
    [SerializeField]
    private SocketManager socketManager;
    private bool isActive = false;
    public float fadeDuration = 2.0f; 
    public float waitBeforeRedirect = 5.0f;
    private Coroutine turnTimerCoroutine;
    private const float turnTimeLimit = 15f;
    public Text turnTimerText;
    private int cardsPickedThisTurn = 0;



    public GameObject PopUp;


    void Awake()
    {
        images = Resources.LoadAll<Sprite>("Sprites/Mind");
        socketManager = FindObjectOfType<SocketManager>();
        Mindgame = this;
    }

    private void Start()
    {
        if (socketManager == null)
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }

        player1Score.text = "0";
        player2Score.text = "0";
        player1FinalScore.text = "0";
        player2FinalScore.text = "0";
    }


    public void InitializePlayers(User[] users)
    {
        if (users.Length < 2 || users.Length > 4)
        {
            Logger.LogError("Invalid number of players. The game supports 2 to 4 players.");
            return;
        }

        players = users;
        UpdatePlayerUI();
    }

    private void UpdatePlayerUI()
    {
        if (players == null || players.Length < 2)
        {
            Logger.LogError("Invalid number of players. The game requires at least 2 players.");
            return;
        }

        Player1UserName.text = players[0].username;
        Player2UserName.text = players[1].username;
        prizePool.text = socketManager.getPrizePool().ToString();
    }

    public void HandlePlayerTurn(string socketId)
    {
        StartCoroutine(HandleTurnCoroutine(socketId));
    }

    //private IEnumerator HandleTurnCoroutine(string socketId)
    //{
    //    for (int i = 0; i < players.Length; i++)
    //    {
    //        if (players[i].socketId == socketId)
    //        {
    //            UpdateTurnArrow(i);
    //            yield return new WaitForSeconds(0.5f);
    //            break;
    //        }
    //    }
    //    turnSocketId = socketId;
    //}

    private IEnumerator HandleTurnCoroutine(string socketId)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].socketId == socketId)
            {
                UpdateTurnArrow(i);
                yield return new WaitForSeconds(0.5f);
                break;
            }
        }

        turnSocketId = socketId;
        cardsPickedThisTurn = 0;

        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }

        turnTimerCoroutine = StartCoroutine(StartTurnTimer());
    }



    private IEnumerator StartTurnTimer()
    {
        float timer = turnTimeLimit;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // Display remaining seconds (rounded)
            if (turnTimerText != null)
            {
                turnTimerText.text = Mathf.Ceil(timer).ToString() + "s";
            }

            yield return null;
        }

        // Clear the text on timeout
        if (turnTimerText != null)
        {
            turnTimerText.text = "0s";
        }

        // Emit timeout if it's still this player's turn
        if (socketManager != null && socketManager.isConnected && socketManager.getMySocketId() == turnSocketId)
        {
            socketManager.EmitEvent("UPDATE_TURN", "");
            Logger.Log("TURN_TIMEOUT emitted due to 10s inactivity");
        }
    }



    private void UpdateTurnArrow(int playerIndex)
    {
        // Always keep arrows active
        SetArrowAlphaAndScale(arrowPlayer1, playerIndex == 0);
        SetArrowAlphaAndScale(arrowPlayer2, playerIndex == 1);
    }

    private void SetArrowAlphaAndScale(Transform arrow, bool isActive)
    {
        // Make sure child is active
        if (arrow.childCount > 0)
        {
            GameObject child = arrow.GetChild(0).gameObject;
            if (!child.activeSelf)
                child.SetActive(true);
        }

        // Adjust parent alpha
        Image image = arrow.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = isActive ? 1f : 0f;
            image.color = color;
        }

        // Adjust scale
        arrow.localScale = isActive ? enlargedScale : originalScale;
    }


    public void GetStarted()
    {
        GetButtons();
        AddListeners();
        Logger.Log($"GetStarted called. Buttons count: {btns.Count}");

    }

    void GetButtons()
    {
        btns.Clear();

        GameObject[] objects = GameObject.FindGameObjectsWithTag("GameButton");
        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].image.sprite = bgImage;
        }
    }


    void AddListeners()
    {
        foreach (Button btn in btns)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickButton());
        }
    }

    //public void ClickButton()  
    //{
    //    if (isActive)
    //        return;
    //    if (socketManager != null && socketManager.isConnected)
    //    {
    //        if (socketManager.getMySocketId() == turnSocketId)
    //        {
    //            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    //            string buttonName = selectedObject.name;
    //            Logger.LogError(selectedObject.name);

    //            isActive = true;

    //            socketManager.EmitEvent("PICK_CARD", buttonName);
    //            Logger.Log($"PICK_CARD emitted for {buttonName}");


    //        }
    //    }
    //    else
    //    {
    //        Logger.LogWarning("SocketManager is not connected. Cannot emit PICK_CARD.");
    //    }
    //}
    public void ClickButton()
    {
        if (isActive)
            return;

        if (socketManager != null && socketManager.isConnected)
        {
            if (socketManager.getMySocketId() == turnSocketId)
            {
                GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                string buttonName = selectedObject.name;
                Logger.LogWarning(selectedObject.name);

                isActive = true;

                socketManager.EmitEvent("PICK_CARD", buttonName);
                Logger.Log($"PICK_CARD emitted for {buttonName}");

                // Track the number of picks
                cardsPickedThisTurn++;

                if (cardsPickedThisTurn >= 2)
                {
                    // Cancel turn timer after second pick
                    if (turnTimerCoroutine != null)
                    {
                        StopCoroutine(turnTimerCoroutine);
                        turnTimerCoroutine = null;
                    }

                    if (turnTimerText != null)
                    {
                        turnTimerText.text = "";
                    }

                    cardsPickedThisTurn = 0; // Reset for next turn
                }
            }
        }
        else
        {
            Logger.LogWarning("SocketManager is not connected. Cannot emit PICK_CARD.");
        }
    }


    public void LoadCardSprite()
    {
        isActive = false;

        CardData cardData = socketManager.GetCardData();
        if (cardData == null)
        {
            Logger.LogWarning("No stored card data available.");
            return;
        }

        int index = cardData.index;
        string cardName = cardData.card;

        if (index < 0 || index >= btns.Count)
        {
            Logger.LogWarning($"Invalid card index: {index}");
            return;
        }

        Button button = btns[index];
        GameObject selectedObject = button.gameObject;

        selectedObject.transform.GetChild(0).gameObject.SetActive(true);

        Sprite sprite = System.Array.Find(images, img => img.name == cardName);
        if (sprite != null)
        {
            button.image.sprite = sprite;
        }
        else
        {
            Logger.LogWarning($"Sprite {cardName} not found in the loaded images!");
        }

        Logger.Log($"Card displayed: Index = {index}, Card = {cardName}");

        StartCoroutine(RevertCardAppearance(selectedObject));
    }

    private IEnumerator RevertCardAppearance(GameObject selectedObject)
    {
        yield return new WaitForSeconds(0.5f);

        selectedObject.transform.GetChild(0).gameObject.SetActive(false);
    }




    public void CloseCardSprite(int index1, int index2)
    {
        if (index1 < 0 || index1 >= btns.Count || index2 < 0 || index2 >= btns.Count)
        {
            Logger.LogWarning($"Invalid indices: {index1}, {index2}");
            return;
        }

        Button firstButton = btns[index1];
        Button secondButton = btns[index2];

        GameObject firstObject = firstButton.gameObject;
        GameObject secondObject = secondButton.gameObject;

        firstObject.transform.GetChild(0).gameObject.SetActive(true);

        secondObject.transform.GetChild(0).gameObject.SetActive(true);

        Logger.Log($"Cards closing: {index1}, {index2}");

        StartCoroutine(RevertClosedCards(firstObject, firstButton, secondObject, secondButton));

        if (socketManager != null && socketManager.isConnected)
        {
            if (socketManager.getMySocketId() == turnSocketId)
            {
                socketManager.EmitEvent("UPDATE_TURN", "");
                Logger.Log($"Update turn emitted");
            }
        }
    }

    private IEnumerator RevertClosedCards(GameObject firstObject, Button firstButton, GameObject secondObject, Button secondButton)
    {
        yield return new WaitForSeconds(0.5f);

        firstButton.image.sprite = bgImage;
        firstObject.transform.GetChild(0).gameObject.SetActive(false);

        secondButton.image.sprite = bgImage;
        secondObject.transform.GetChild(0).gameObject.SetActive(false);

        Logger.Log("Both cards reset at the same time.");
    }




    public void DisableMatchedCards(int index1, int index2, int score1, int score2)
    {
        Button firstButton = btns[index1];
        if (firstButton != null)
        {
            firstButton.gameObject.SetActive(false);
        }

        Button secondButton = btns[index2];
        if (secondButton != null)
        {
            secondButton.gameObject.SetActive(false);
        }

        Logger.Log("Matched cards disabled: " + index1 + ", " + index2);

        player1Score.text = score1.ToString();
        player2Score.text = score2.ToString();
    }

    //private int player1CurrentScore = 0;
    //private int player2CurrentScore = 0;

    //public void DisableMatchedCards(int index1, int index2)
    //{
    //    Button firstButton = btns[index1];
    //    if (firstButton != null)
    //    {
    //        firstButton.gameObject.SetActive(false);
    //    }

    //    Button secondButton = btns[index2];
    //    if (secondButton != null)
    //    {
    //        secondButton.gameObject.SetActive(false);
    //    }

    //    Logger.Log("Matched cards disabled: " + index1 + ", " + index2);

    //    // Determine whose turn it is
    //    if (turnSocketId == players[0].socketId)
    //    {
    //        player1CurrentScore++;
    //        player1Score.text = player1CurrentScore.ToString();
    //    }
    //    else if (turnSocketId == players[1].socketId)
    //    {
    //        player2CurrentScore++;
    //        player2Score.text = player2CurrentScore.ToString();
    //    }
    //}


    public void EndGame(string winnerId, int score1, int score2)
    {
        player1FinalScore.text = score1.ToString();
        player2FinalScore.text = score2.ToString();

        if (socketManager.socket.Id == winnerId)
        {
            StartCoroutine(ShowPopup(WinnerPopUp, "You Win!"));
        }
        else
        {
            StartCoroutine(ShowPopup(LoosePopUp, "You Lose!"));
        }
    }

    private IEnumerator ShowPopup(GameObject popup, string resultMessage)
    {
        popup.SetActive(true);

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popup.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;

        Text popupText = popup.GetComponentInChildren<Text>();
        if (popupText != null)
        {
            popupText.text = resultMessage;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f; 

        yield return new WaitForSeconds(waitBeforeRedirect);

        SceneManager.LoadScene("Home"); 
    }

    public void EnablePopUp()
    {
        PopUp.SetActive(true);
    }

    public void QuitToHome()
    {
        SceneManager.LoadScene("Home");
        socketManager.EmitEvent("EXIT_ROOM", "");
        Logger.Log($"Exit room Emitted");
    }
    public void ClosePopup()
    {
        if (PopUp != null)
        {
            PopUp.SetActive(false);
        }
    }
}
