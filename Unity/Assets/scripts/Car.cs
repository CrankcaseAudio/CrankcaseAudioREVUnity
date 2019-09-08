[System.Serializable]
public struct Car
{
    public string fileName;
    public string title;
    public string subtitle;
    public string imageName;

    public Car(string fileName, string title, string imageName, string subtitle)
    {
        this.fileName = fileName;
        this.title = title;
        this.subtitle = subtitle;
        this.imageName = imageName;
    }

    // Check if entry is contains all data
    public bool IsValid()
    {
        return (imageName.ToLower().Contains("png") || imageName.ToLower().Contains("jpg")) &&
                // (fileName.Split('.').Length >= 2) &&
                (title.Length > 0) &&
                (subtitle.Length > 0);
    }

    public override string ToString()
    {
        return string.Format("filename:{0}\ntitle:{1}\nsubtitle:{2}\nimageName:{3}", fileName, title, subtitle, imageName);
    }
}
