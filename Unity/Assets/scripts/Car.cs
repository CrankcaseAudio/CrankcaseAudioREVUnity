using System;
using UnityEngine;

[System.Serializable]
public struct Car
{
    public string fileName;
    public string title;
    public string subtitle;
    public string imageName;

    public Car(string fileName, string title, string hasImage, string subtitle)
    {
        this.fileName = fileName;
        this.title = title;
        this.subtitle = subtitle;
        if (hasImage == "TRUE")
        {
            this.imageName = fileName + ".jpg";
        }
        else
        {
            this.imageName = null;
        }
    }

    // Check if entry is contains all data
    public bool IsValid()
    {
        var isValid = imageName != null &&
                (title.Length > 0) &&
                (subtitle.Length > 0);

        if (!isValid)
        {
            return false;
        }
//#if DEBUG
//        Debug.LogError("DebugEngineExistancText. Remove From Prod:" + Filepath);
//        // Try loading the engine to ensure it exists in debug only. Its expensive, but there's no Resource.Exists(filename);
//        LoadEngineData();
//#endif

        return true;
    }

    public override string ToString()
    {
        return string.Format("filename:{0}\ntitle:{1}\nsubtitle:{2}\nimageName:{3}", fileName, title, subtitle, imageName);
    }

    public String Filepath
    {
        get
        {
            var fileBasePath = "48000\\";

#if APPSTORE
            fileBasePath = "encrypted\\" + fileBasePath;
#endif
            var filePath = "engines\\" + fileBasePath + fileName;
            return filePath;
        }
    }

    public byte[] LoadEngineData()
    {

        Debug.Log("Load Engine: " + Filepath);

        byte[] fileData = null;

        var asset = Resources.Load<TextAsset>(Filepath);

        if (asset == null)
            throw new UnityException("No asset:" + Filepath);

        fileData = asset.bytes;
        Resources.UnloadUnusedAssets();

#if APPSTORE
        fileData = REVDemo.Security.Decode(fileData);
#endif

        return fileData;
    }
}
