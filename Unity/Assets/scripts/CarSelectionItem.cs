using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarSelectionItem : MonoBehaviour
{
    public event EventHandler<CarEventArgs> onSelect;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public Image carImage;
    public Image grayscaleImage;
    public Image textOverlayImage;
    public Image borderImage;
    public Button selectButton;
    public CanvasGroup cg;

    public Car car;
    public bool isSelected { get; private set; }

    private Color originalFontColor;
    private float originalOverlayAlpha;

    /// <summary>
    /// Update item from Car
    /// </summary>
    public void Initialize(Car car)
    {
        this.car = car;
        this.gameObject.name += "_" + car.title;
        titleText.text = car.title;
        subtitleText.text = car.subtitle;
        string fullImagePath = GetImagePath(car.imageName);
        IOHelper.GetSprite(Main.instance, fullImagePath, (sprite) =>
        {
            carImage.sprite = sprite;
            grayscaleImage.sprite = sprite;
        });

        selectButton.onClick.AddListener(() =>
        {
            if (onSelect != null)
                onSelect(this, new CarEventArgs(car));
        });

        originalFontColor = titleText.color;
        originalOverlayAlpha = textOverlayImage.color.a;
    }

    private float GetRelativeDistanceFromCenter()
    {
        // Get relative screen position (0-1)
        Vector2 screenPosRelative = this.transform.position / new Vector2(Screen.width, Screen.height);
#if UNITY_EDITOR
        screenPosRelative = this.transform.position / IOHelper.GetMainGameViewSize();
#endif
        // Get relative distance from center
        float distance = HelperFunctions.Retarget(screenPosRelative.y, 0f, 1f, -1f, 1f);
        distance = Mathf.Abs(distance);
        // Narrow wideness
        distance *= 2f;
        return distance;
    }
    
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        SetGrayscale((isSelected) ? 0f : 1f);
    }

    public void SetGrayscale(float grayscale)
    {
        grayscaleImage.color = new Color(1f, 1f, 1f, grayscale);
        textOverlayImage.color = new Color(textOverlayImage.color.r, textOverlayImage.color.g, textOverlayImage.color.b, (1 - grayscale) * originalOverlayAlpha);
        borderImage.color = new Color(borderImage.color.r, borderImage.color.g, borderImage.color.b, 1 - grayscale);
    }

    /// <summary>
    /// Get full image path under {Application}\\StreamingAssets\\Images\\{model}.jpg
    /// </summary>
    public string GetImagePath(string imageName)
    {
        return string.Format("{0}\\Images\\{1}", Application.streamingAssetsPath, imageName);
    }

    public class CarEventArgs : EventArgs
    {
        public Car car;
        public CarEventArgs(Car car)
        {
            this.car = car;
        }
    }

}
