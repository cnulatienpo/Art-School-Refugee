using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a welcome panel the first time the player enters the Mess Hall.
/// Checks PlayerPrefs for the "messhall_seen" key and optionally saves it
/// when the user chooses not to show the screen again.
/// </summary>
public class MessHallIntroScreen : MonoBehaviour
{
    public CanvasGroup panel;           // Root panel with CanvasGroup for fading
    public Text messageText;            // Body text component
    public Toggle dontShowAgainToggle;  // Toggle to skip next time
    public Button startDrawingButton;   // Button to close the panel

    public float fadeDuration = 0.5f;

    const string PrefKey = "messhall_seen";

    void Awake()
    {
        if (startDrawingButton != null)
            startDrawingButton.onClick.AddListener(OnStartClicked);
    }

    void OnEnable()
    {
        if (PlayerPrefs.GetInt(PrefKey, 0) == 0)
            ShowIntro();
        else if (panel != null)
            panel.gameObject.SetActive(false);
    }

    void ShowIntro()
    {
        if (panel == null)
            return;

        if (messageText != null)
            messageText.text = IntroText;

        panel.alpha = 0f;
        panel.gameObject.SetActive(true);
        StartCoroutine(FadeCanvas(0f, 1f));
    }

    void OnStartClicked()
    {
        if (dontShowAgainToggle != null && dontShowAgainToggle.isOn)
        {
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }
        StartCoroutine(FadeOutAndDisable());
    }

    IEnumerator FadeCanvas(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (panel != null)
                panel.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        if (panel != null)
            panel.alpha = to;
    }

    IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvas(panel.alpha, 0f);
        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    static readonly string IntroText =
        "This is where you can make a mess.\n" +
        "Not to judge yourself. Not to be correct. Just to draw. this is the one place where you get to just be the artist that you are today.\n\n" +
        "You don't have to know what to make\u2014the prompts will help you.\n" +
        "You don't have to remember what you learned\u2014the shapes and tools are already in your hands.\n" +
        "If you don't need prompts, that's good too.\n\n" +
        "The only rule here is:\n\"If it harm none, do what you will.\"\n\n" +
        "The canvas is like a roll of butcher paper\u2014it never ends.\n" +
        "You can scroll down forever or add space when you need it.\n\n" +
        "So let go of your expectations.\n" +
        "Sit with yourself.\n" +
        "And let what's inside you come out.";
}
