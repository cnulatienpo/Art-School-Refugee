using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Mode { Level1, MessHall, Level2 }

    public static bool IsLevel2Unlocked { get; private set; }
    public Mode ActiveMode { get; private set; }

    public const string Level2Key = "Level2Unlocked";

    public static event System.Action Level2Unlocked;

    void Awake()
    {
        IsLevel2Unlocked = PlayerPrefs.GetInt(Level2Key, 0) == 1;
    }

    public void EnterLevel1()
    {
        ActiveMode = Mode.Level1;
    }

    public void EnterMessHall()
    {
        ActiveMode = Mode.MessHall;
    }

    public void EnterLevel2()
    {
        ActiveMode = Mode.Level2;
    }

    public void MarkLevel1Complete()
    {
        if (!IsLevel2Unlocked)
        {
            IsLevel2Unlocked = true;
            PlayerPrefs.SetInt(Level2Key, 1);
            PlayerPrefs.Save();
            Level2Unlocked?.Invoke();
        }
    }
}
