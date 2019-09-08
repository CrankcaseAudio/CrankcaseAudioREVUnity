using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Helper class for UI Methods
/// </summary>
public static class IOHelper
{
    public static readonly char columnSeparator = ',';
    public static readonly bool skipFirstLine = true;

    /// <summary>
    /// Get all Car entries from file (csv)
    /// File format: {title};{subtitle};{imageName}
    /// </summary>
    public static List<Car> ReadConfigFile(string file)
    {
        List<Car> cars = new List<Car>();

        // Check if config file exists
        var asset = Resources.Load<TextAsset>(file);

        if (asset == null)
        {
            var errorMessage = string.Format("Configuration File doesn't exists\n{0}", file);
            Debug.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        // Read all lines form file
        string[] lines = asset.text.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            if (skipFirstLine && i == 0)
                continue;

            // Read all columns in line
            string[] columns = lines[i].Split(columnSeparator);

            // Check column integrity
            if (columns.Length < 4)
            {
                Debug.LogWarning(string.Format("Skipping invalid entry. Column dimension mismatch. Expected 4, but found {0}\n{1}", columns.Length, lines[i]));
                continue;
            }

            // Assign each column
            Car entry = new Car(columns[0], columns[1], columns[2], columns[3]);

            // Add entry
            if (entry.IsValid())
            {
                cars.Add(entry);
            }
            else
            {
                Debug.LogWarning(string.Format("Skipping invalid entry:\n{0}", entry.ToString()));
            }
        }

        return cars;
    }

    /// <summary>
    /// Load Sprite from file (StreamingAssets) 
    /// </summary>
    public static void GetSprite(MonoBehaviour mono, string path, Action<Sprite> callback)
    {
        mono.StartCoroutine(LoadSprite(path, (sprite) =>
        {
            callback(sprite);
        }));
    }

    /// <summary>
    /// Load Sprite enumerator
    /// </summary>
    private static IEnumerator LoadSprite(string file, Action<Sprite> callback)
    {
        if (!File.Exists(file))
        {
            Debug.LogWarning("Car image doesn't exist\n" + file);
            yield break;
        }

        string url = string.Format("file://{0}", file);
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();
        Texture2D myTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;
        Sprite sprite = Sprite.Create(myTexture as Texture2D, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.one / 2);

        // Get the name
        string[] s = file.Split('/', '\\');
        string name = s[s.Length - 1];
        name = name.Remove(name.Length - 4, 4);

        sprite.name = name;

        if (callback != null)
            callback(sprite);
    }

    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }
}

public static class HelperFunctions
{
    /// <summary>
    /// Remap a value from a range {A->B} to another {X->Y}
    /// </summary>
    public static float Retarget(float value, float currentMin, float currentMax, float targetMin, float targetMax, bool clamp = true)
    {
        float temp = value;

        // Normalize
        temp -= currentMin;
        temp /= (currentMax - currentMin);

        // Retarget
        temp *= (targetMax - targetMin);
        temp += targetMin;

        if (clamp)
            temp = Mathf.Clamp(temp, targetMin, targetMax);

        return temp;
    }
}

