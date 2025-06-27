using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Mode { Level1, MessHall, Level2 }

    public static GameManager Instance { get; private set; }

    public Mode ActiveMode { get; private set; } = Mode.Level1;
    public bool IsLevel2Unlocked = true;

    public static event Action OnGrammarChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void EnterLevel1()
    {
        ActiveMode = Mode.Level1;
        OnGrammarChanged?.Invoke();
    }

    public void EnterMessHall()
    {
        ActiveMode = Mode.MessHall;
    }

    public void EnterLevel2()
    {
        if (!IsLevel2Unlocked)
            return;
        ActiveMode = Mode.Level2;
        OnGrammarChanged?.Invoke();
    }

    public string GetCardsDataPath()
    {
        return ActiveMode == Mode.Level2 ? "grammer2/level2_shape_grammar_cards" : "Data/shape_grammar_cards";
    }

    public string GetShapePrefabPath(string shapeName)
    {
        if (ActiveMode == Mode.Level2)
        {
            string sanitized = shapeName.ToLower().Replace(" + ", " on ");
            return "grammer2/" + sanitized;
        }
        return "the" + shapeName.ToLower();
    }
}
