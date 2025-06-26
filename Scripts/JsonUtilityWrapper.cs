using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityWrapper
{
    public static List<T> FromJsonList<T>(string json)
    {
        string wrappedJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}
