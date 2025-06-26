using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example component that applies simple modifiers to a shape and logs
/// the action to <see cref="MessHallSessionTracker"/>.
/// </summary>
public class ShapeModifierHandler : MonoBehaviour
{
    [Header("Session Tracking")] public MessHallSessionTracker sessionTracker;

    [System.Serializable] public class ModifierEvent : UnityEvent<string> {}

    public ModifierEvent onModifierApplied;

    /// <summary>
    /// Apply a modifier identified by <paramref name="tag"/>.
    /// The actual modification of the GameObject is left to the user.
    /// </summary>
    public void ApplyModifier(string tag)
    {
        sessionTracker?.AddModifierTag(tag);
        onModifierApplied?.Invoke(tag);
    }
}
