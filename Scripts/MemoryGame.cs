using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryGame : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform gridParent;
    public RectTransform backgroundCutout;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public Button startButton;
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip gameOverSound;

    private List<Card> cards = new List<Card>();
    private List<int> pattern = new List<int>();
    private int playerStep = 0;

    private int rows = 3;
    private int columns = 3;
    private int currentLevel = 1;
    private int currentSubLevel = 1;
    private int score = 0;
    private bool gameActive = false;
    private bool isShowingPattern = false;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        SetupGrid(rows, columns);
        UpdateUI();
        SetButtonText("Start");
    }

    void OnStartButtonClicked()
    {
        if (isShowingPattern) return;

        if (!gameActive)
        {
            ResetGame();
            StartLevel();
            gameActive = true;
            SetButtonText("Restart");
        }
        else
        {
            ResetGame();
            SetButtonText("Start");
            gameActive = false;
        }
    }

    void SetupGrid(int rowCount, int columnCount)
    {
        // Clear existing cards
        foreach (Card card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();

        // Setup grid layout
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnCount;

        ResizeBackground(rowCount, columnCount);

        // Create new cards
        int total = rowCount * columnCount;
        for (int i = 0; i < total; i++)
        {
            GameObject obj = Instantiate(cardPrefab, gridParent);
            Card card = obj.GetComponent<Card>();
            card.Init(i, this);
            card.SetColor(RandomCardColor());
            cards.Add(card);
        }
    }

    void ResizeBackground(int rowCount, int columnCount)
    {
        float size = Mathf.Max(rowCount, columnCount) * 120f;
        backgroundCutout.sizeDelta = new Vector2(size, size);
    }

    void StartLevel()
    {
        UpdateUI();
        playerStep = 0;
        pattern.Clear();

        // Generate random pattern
        int patternLength = GetPatternLengthForSubLevel();
        int GetPatternLengthForSubLevel()
        {
            int baseBlink = 2 + currentLevel; // Example: Level 1 = 3 blinks, Level 2 = 4 blinks, etc.
            return baseBlink + (currentSubLevel - 1);
        }


        HashSet<int> usedIndices = new HashSet<int>();

        while (pattern.Count < patternLength)
        {
            int randIndex = Random.Range(0, cards.Count);
            if (!usedIndices.Contains(randIndex))
            {
                pattern.Add(randIndex);
                usedIndices.Add(randIndex);
            }
        }

        StartCoroutine(ShowPattern());
    }

    IEnumerator ShowPattern()
    {
        isShowingPattern = true;
        yield return new WaitForSeconds(1.0f);

        foreach (int idx in pattern)
        {
            cards[idx].Highlight();
            yield return new WaitForSeconds(1.0f);
        }

        isShowingPattern = false;
    }

    public void OnCardSelected(int index)
    {
        if (!gameActive || isShowingPattern) return;

        PlaySound(clickSound);

        if (pattern[playerStep] == index)
        {
            playerStep++;
            if (playerStep == pattern.Count)
            {
                score++;
                UpdateUI();
                NextSubLevel();
            }
        }
        else
        {
            GameOver();
        }
    }

    void NextSubLevel()
    {
        currentSubLevel++;

        if (currentSubLevel > 3)
        {
            currentSubLevel = 1;
            currentLevel++;

            if (currentLevel > 3)
            {
                WinGame();
                return;
            }

            rows++;
            columns++;
            SetupGrid(rows, columns);
        }

        StartCoroutine(DelayBeforeNextLevel());
    }

    IEnumerator DelayBeforeNextLevel()
    {
        yield return new WaitForSeconds(1.5f);
        StartLevel();
    }

    void GameOver()
    {
        PlaySound(gameOverSound);
        EndGame("Game Over!\nFinal Score: " + score);
    }

    void WinGame()
    {
        EndGame("🎉 You Win!\nFinal Score: " + score);
    }

    void EndGame(string message)
    {
        gameActive = false;
        levelText.text = message;
        scoreText.gameObject.SetActive(false);
        SetButtonText("Restart");
        HideCards();
    }

    void ResetGame()
    {
        currentLevel = 1;
        currentSubLevel = 1;
        rows = 3;
        columns = 3;
        score = 0;

        SetupGrid(rows, columns);
        UpdateUI();
        scoreText.gameObject.SetActive(true);
        ShowCards();
    }

    void HideCards()
    {
        foreach (Card card in cards)
        {
            card.gameObject.SetActive(false);
        }
    }

    void ShowCards()
    {
        foreach (Card card in cards)
        {
            card.gameObject.SetActive(true);
            card.SetColor(RandomCardColor());
        }
    }

    void UpdateUI()
    {
        levelText.text = $"Level {currentLevel}-{currentSubLevel}";
        scoreText.text = $"Score: {score}";
    }

    void SetButtonText(string text)
    {
        startButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    Color RandomCardColor()
    {
        Color color;
        do
        {
            color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
        } while (color.grayscale > 0.8f);
        return color;
    }
}