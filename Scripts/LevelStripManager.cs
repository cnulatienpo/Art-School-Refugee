using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the vertical level strip UI anchored to the left side of the screen.
/// Buttons can switch between Level 1, the Mess Hall, and Level 2. Level 2 is
/// disabled until unlocked through the GameManager.
/// </summary>
public class LevelStripManager : MonoBehaviour
{
    [Header("Button References")]
    public Button level1Button;
    public Button messHallButton;
    public Button level2Button;
    public Button level3Button;

    [Header("Passage 2 Intro")]
    public Passage2IntroUI passage2Intro;

    [Header("Highlight Colors")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.cyan;

    GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (level1Button != null)
            level1Button.onClick.AddListener(OnLevel1Clicked);
        if (messHallButton != null)
            messHallButton.onClick.AddListener(OnMessHallClicked);
        if (level2Button != null)
            level2Button.onClick.AddListener(OnLevel2Clicked);
        if (level3Button != null)
            level3Button.onClick.AddListener(OnLevel3Clicked);
    }

    void OnEnable()
    {
        GameManager.Level2Unlocked += OnLevel2Unlocked;
        GameManager.Level3Unlocked += OnLevel3Unlocked;
    }

    void OnDisable()
    {
        GameManager.Level2Unlocked -= OnLevel2Unlocked;
        GameManager.Level3Unlocked -= OnLevel3Unlocked;
    }

    void Start()
    {
        UpdateButtonStates();
        HighlightActiveButton();
    }

    public void UpdateButtonStates()
    {
        bool level2Unlocked = gameManager != null && gameManager.IsLevel2Unlocked;
        bool level3Unlocked = gameManager != null && gameManager.IsLevel3Unlocked;
        if (level2Button != null)
        {
            level2Button.interactable = level2Unlocked;
            Image img = level2Button.GetComponent<Image>();
            if (img != null)
                img.color = level2Unlocked ? normalColor : Color.gray;
        }
        if (level3Button != null)
        {
            level3Button.interactable = level3Unlocked;
            Image img = level3Button.GetComponent<Image>();
            if (img != null)
                img.color = level3Unlocked ? normalColor : Color.gray;
        }
    }

    void OnLevel2Unlocked()
    {
        UpdateButtonStates();
    }

    void OnLevel3Unlocked()
    {
        UpdateButtonStates();
    }

    public void HighlightActiveButton()
    {
        if (gameManager == null)
            return;

        SetButtonColor(level1Button, gameManager.ActiveMode == GameManager.Mode.Level1);
        SetButtonColor(messHallButton, gameManager.ActiveMode == GameManager.Mode.MessHall);
        SetButtonColor(level2Button, gameManager.ActiveMode == GameManager.Mode.Level2);
        SetButtonColor(level3Button, gameManager.ActiveMode == GameManager.Mode.Level3);
    }

    void SetButtonColor(Button button, bool active)
    {
        if (button == null) return;
        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
            text.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;

        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = active ? activeColor : normalColor;
    }

    void OnLevel1Clicked()
    {
        if (gameManager != null)
            gameManager.EnterLevel1();
        HighlightActiveButton();
    }

    void OnMessHallClicked()
    {
        if (gameManager != null)
            gameManager.EnterMessHall();
        HighlightActiveButton();
    }

    void OnLevel2Clicked()
    {
        if (gameManager != null && gameManager.IsLevel2Unlocked)
        {
            if (passage2Intro != null)
            {
                passage2Intro.Show();
            }
            else
            {
                gameManager.EnterLevel2();
                HighlightActiveButton();
            }
        }
    }

    void OnLevel3Clicked()
    {
        if (gameManager != null && gameManager.IsLevel3Unlocked)
        {
            gameManager.EnterLevel3();
            HighlightActiveButton();
        }
    }
}
