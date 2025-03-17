using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro
using System;
using System.IO;
using System.Threading;

public class CardGameMain : MonoBehaviour
{
    public enum cardTypes
    {
        diamond = 1, spade = 2, club = 3, heart = 4,
        joker = 5, orange_mushroom = 6, slime = 7, yeti = 8
    }

    // Game references
    public static CardGameMain main;
    public CardUI prefabCardUI;
    public RectTransform canvas;
    public TextMeshProUGUI matchedPairsCounterText;
    public GameObject winPanel;

    // Sprites
    public Sprite diamondSprite, spadeSprite, clubSprite, heartSprite;
    public Sprite jokerSprite, orangeMushroomSprite, slimeSprite, yetiSprite;

    // Game state
    private CardUI card1, card2;
    private bool isCurrentlyChecking = false;
    private bool didPlayerWinGame = false;
    private List<GameObject> listOfCardsObject = new List<GameObject>();

    private int max_x = 4, max_y = 4, totalPairs, count = 0;

    // Metrics
    private bool isGameRunning = false;
    private float sessionStartTime = 0f;
    private int incorrectMatches = 0, totalAttempts = 0;
    private float attemptStartTime = 0f;
    private List<float> attemptTimes = new List<float>();
    private bool GameisPaused = false;

    // Save location
    private string saveDirectory;

    [Header("Audio Settings")]
    public AudioSource audioSource;   // Audio source to play sounds
    public AudioClip flipCardSound;   // Sound effect for flipping a card


    void Start()
    {
        main = this;

        // Debugging: Show where we're saving
        Debug.Log("Initializing CardGameMain...");

        // **Choose save location** (Manual or `persistentDataPath`)
        // Comment/uncomment based on your preference

        // 1. Use a fixed path (Make sure the folder exists!)
         saveDirectory = @"C:\Unity_Projects\GameSessions\CardGame";

        // 2. Use Unity's persistent data path (Recommended)
        //saveDirectory = Path.Combine(Application.persistentDataPath, "CardGameSessions");

        Debug.Log($"Save directory set to: {saveDirectory}");

        StartGame();
    }

    // -----------------------------
    //         GAME SETUP
    // -----------------------------
    void StartGame()
    {
        GameisPaused = false;
        Debug.Log("Starting game session...");

        // Ensure game resets properly
        didPlayerWinGame = false;
        count = 0;
        UpdateMatchedPairsCounter();
        DestroyAllCards();

        totalPairs = (max_x * max_y) / 2; // e.g., 4x4 => 8 pairs
        if (winPanel != null) winPanel.SetActive(false);

        // Shuffle & spawn cards
        List<cardTypes> cardsTypeList = new List<cardTypes>
        {
            cardTypes.club, cardTypes.spade, cardTypes.heart, cardTypes.diamond,
            cardTypes.joker, cardTypes.orange_mushroom, cardTypes.slime, cardTypes.yeti,
            cardTypes.club, cardTypes.spade, cardTypes.heart, cardTypes.diamond,
            cardTypes.joker, cardTypes.orange_mushroom, cardTypes.slime, cardTypes.yeti,
        };
        ShuffleCards(cardsTypeList);

        int x_pos = -350, y_pos = 400, i = 0;
        for (int x = 0; x < max_x; x++)
        {
            for (int y = 0; y < max_y; y++)
            {
                CardUI clone = Instantiate(prefabCardUI, canvas);
                clone.transform.localPosition = new Vector3(x_pos + 200 * x, y_pos - 250 * y);
                clone.cardType = cardsTypeList[i];
                AssignCardSprite(clone);
                listOfCardsObject.Add(clone.gameObject);
                i++;
            }
        }

        // Reset game metrics
        incorrectMatches = 0;
        totalAttempts = 0;
        attemptTimes.Clear();

        // Start session timer
        sessionStartTime = Time.time;
        isGameRunning = true;
        Debug.Log("Game started successfully.");
    }

    void ShuffleCards(List<cardTypes> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]); // Swap
        }
    }

    // -----------------------------
    //       PLAYER INPUT
    // -----------------------------
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Check if "-" is pressed to manually end session
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Debug.Log("Minus key pressed.");

            if (isGameRunning)
            {
                Debug.Log("Game is running. Saving session...");
                isGameRunning = false;
                SaveSession();
                DestroyAllCards();
                Debug.Log("[CardGame] Session ended manually. Data saved and cards destroyed.");
            }
            else
            {
                Debug.LogWarning("Game is not running. Save not triggered.");
            }
        }
    }

    // -----------------------------
    //       WIN CONDITION
    // -----------------------------
    IEnumerator HandleWinCondition()
    {
        isGameRunning = false;
        if (winPanel != null) winPanel.SetActive(true);
        SaveSession();
        yield return new WaitForSeconds(3f);
        DestroyAllCards();
        StartGame();
    }

    void UpdateMatchedPairsCounter()
    {
        if (matchedPairsCounterText != null)
        {
            matchedPairsCounterText.text = "Matched Pairs: " + count.ToString();
        }
    }

    // -----------------------------
    //       SAVE SESSION
    // -----------------------------
    public void SaveSession()
    {
        
        try
        {
            if (Time.time - sessionStartTime  <= 0f)
            {
                Debug.LogWarning("SaveSession called but sessionStartTime is invalid. Aborting save.");
                return;
            }

            float totalTimeInGame = Time.time - sessionStartTime;

            CardGameSessionData data = new CardGameSessionData
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                totalTime = totalTimeInGame,
                incorrectMatches = incorrectMatches,
                totalAttempts = totalAttempts,
                attemptTimes = new List<float>(attemptTimes)
            };

            string json = JsonUtility.ToJson(data, true);

            // Ensure directory exists
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                Debug.Log($"Created directory: {saveDirectory}");
            }

            string fileName = $"CardGameSession_{data.timestamp}.json";
            string fullPath = Path.Combine(saveDirectory, fileName);
            File.WriteAllText(fullPath, json);

            Debug.Log($"[CardGame] Session saved to: {fullPath}\n{json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CardGame] Error saving session: {ex}");
        }
        finally
        {
            sessionStartTime = 0f; // Prevent duplicate saves
        }
    }

    void DestroyAllCards()
    {
        foreach (var card in listOfCardsObject)
        {
            Destroy(card);
        }
        listOfCardsObject.Clear();
    }

    void AssignCardSprite(CardUI card)
    {
        switch (card.cardType)
        {
            case cardTypes.diamond: card.frontImage.sprite = diamondSprite; break;
            case cardTypes.spade: card.frontImage.sprite = spadeSprite; break;
            case cardTypes.club: card.frontImage.sprite = clubSprite; break;
            case cardTypes.heart: card.frontImage.sprite = heartSprite; break;
            case cardTypes.joker: card.frontImage.sprite = jokerSprite; break;
            case cardTypes.orange_mushroom: card.frontImage.sprite = orangeMushroomSprite; break;
            case cardTypes.slime: card.frontImage.sprite = slimeSprite; break;
            case cardTypes.yeti: card.frontImage.sprite = yetiSprite; break;
        }
    }
    public void TogglePause()
    {
        GameisPaused = !GameisPaused;
        if (GameisPaused)
        {
            Time.timeScale = 0;
            
        }
        else
        {
            Time.timeScale = 1;
        }

    }
    public void onCardSelection(CardUI cardUIelement)
    {
        if (GameisPaused)
        {
            return;
        }

        // If the game is not running or we're in the middle of checking, return
        if (!isGameRunning || isCurrentlyChecking) return;

        // Play the card flip sound
        if (audioSource != null && flipCardSound != null)
        {
            audioSource.PlayOneShot(flipCardSound);
        }

        // If first card not selected yet
        if (card1 == null)
        {
            card1 = cardUIelement;
            card1.PlayAnim("FlipOpen", () => { });

            // Mark the time we started this attempt (when first card is revealed)
            attemptStartTime = Time.time;
            return;
        }

        // If user clicked the same card, do nothing
        if (card1 == cardUIelement) return;

        // We have a second card now
        card2 = cardUIelement;
        isCurrentlyChecking = true;

        card2.PlayAnim("FlipOpen", () =>
        {
            // This is considered an attempt => increment totalAttempts
            totalAttempts++;

            // The time spent to pick the second card after the first
            float timeForThisAttempt = Time.time - attemptStartTime;
            attemptTimes.Add(timeForThisAttempt);

            if (card1.cardType == card2.cardType)
            {
                card1.isAlreadyMatchedWithAnotherCard = true;
                card2.isAlreadyMatchedWithAnotherCard = true;
                count += 1;
                UpdateMatchedPairsCounter();

                // Check Win
                if (count == totalPairs && !didPlayerWinGame)
                {
                    didPlayerWinGame = true;
                    StartCoroutine(HandleWinCondition());
                }
            }
            else
            {
                // It's an incorrect match
                incorrectMatches++;

                // Flip them back
                card1.PlayAnim("FlipClose", () => { });
                card2.PlayAnim("FlipClose", () => { });
            }

            // Reset for next attempt
            card1 = null;
            card2 = null;
            isCurrentlyChecking = false;
        });
    }

}
