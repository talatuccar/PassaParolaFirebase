using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    public static Dictionary<string, string> myData = new Dictionary<string, string>();
    public static void SaveDictionary(Dictionary<string, string> comingData)
    {
        ClearPreData();

        myData = comingData;

        DictionaryWrapper wrapper = new DictionaryWrapper();

        foreach (var pair in myData)
        {
            wrapper.items.Add(new StringIntPair { key = pair.Key, value = pair.Value });
        }

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("MyDictionary", json);
        PlayerPrefs.Save();
    }

    public static Dictionary<string, string> LoadDictionary()
    {
        myData.Clear();
        if (PlayerPrefs.HasKey("MyDictionary"))
        {
            string json = PlayerPrefs.GetString("MyDictionary");
            DictionaryWrapper wrapper = JsonUtility.FromJson<DictionaryWrapper>(json);

            foreach (var pair in wrapper.items)
            {
                myData[pair.key] = pair.value;
            }
        }
        else
            return null;
       
        return myData;
    }

    [Serializable]
    public class StringIntPair
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class DictionaryWrapper
    {
        public List<StringIntPair> items = new List<StringIntPair>();
    }
    public static bool HasKey()
    {
        if (PlayerPrefs.HasKey("MyDictionary"))
        {
            return true;
        }
        return false;
    }
    public static void ClearPreData()
    {
        if (PlayerPrefs.HasKey("MyDictionary"))
        {
            myData.Clear();
            PlayerPrefs.DeleteKey("MyDictionary");

        }
    }
}
