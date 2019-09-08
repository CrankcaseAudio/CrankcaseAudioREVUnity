using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour
{
    public event EventHandler<CarSelectionItem.CarEventArgs> onSelectItem;

    [Header("UI")]
    public Button backButton;
    public GameObject carSelectionItemPrefab;
    public GameObject carSelectionParent;
    public ScrollRect scrollRect;
#if APPSTORE
    private string configFilename = "LicenceIDs.csv";
#else
    private string configFilename = "LicenceIDs-Demo.csv";
#endif


    private List<Car> cars = new List<Car>();
    private List<CarSelectionItem> carItems = new List<CarSelectionItem>();

    private float scrollAnimationDuration = 0.7f;
    private Coroutine scrollAnimationCR = null;

    public Car FirstCar()
    {
        return cars[0];
    }

    public void Initialize()
    {
        backButton.onClick.AddListener(BackButton_OnClick);
        string configFile = string.Format("{0}\\{1}", Application.streamingAssetsPath, configFilename);
        cars = IOHelper.ReadConfigFile(configFile);
        CreateCarList();
        this.gameObject.SetActive(false);
    }

    private void ScrollTo(int index)
    {
        if (scrollAnimationCR != null)
            StopCoroutine(scrollAnimationCR);
        scrollAnimationCR = StartCoroutine(ScrollToAnim(index));
    }

    private IEnumerator ScrollToAnim(int index)
    {
        float timeStarted = Time.time;
        float segment = 1f / (scrollRect.content.transform.childCount - 4);

        float initialValue = scrollRect.verticalScrollbar.value;
        float targetValue = 1 - segment * index;
        while (Time.time - timeStarted <= scrollAnimationDuration)
        {
            float lerp = (Time.time - timeStarted) / scrollAnimationDuration;
            scrollRect.verticalScrollbar.value = Mathf.Lerp(initialValue, targetValue, lerp);
            yield return null;
        }
    }

    public void Toggle(bool show)
    {
        this.gameObject.SetActive(show);
    }

    private void FocusItem(CarSelectionItem item)
    {
        for (int i = 0; i < carItems.Count; i++)
        {
            carItems[i].SetSelected(item == carItems[i]);
        }
        ScrollTo(item.transform.GetSiblingIndex() - 1);
    }

    private void CreateCarList()
    {
        ClearParentContainer();
        carItems.Clear();

        for (int i = 0; i < cars.Count; i++)
        {
            // Create GameObject listing
            CarSelectionItem item = GameObject.Instantiate(carSelectionItemPrefab).GetComponent<CarSelectionItem>();

            // Update details
            item.Initialize(cars[i]);
            item.onSelect += Item_onSelect;
            item.transform.SetParent(carSelectionParent.transform);
            item.transform.localScale = Vector3.one;

            carItems.Add(item);
        }
    }

    private void Item_onSelect(object sender, CarSelectionItem.CarEventArgs e)
    {
        if (onSelectItem != null)
        {
            onSelectItem(sender, e);
        }

        CarSelectionItem item = sender as CarSelectionItem;
        FocusItem(item);
    }

    private void BackButton_OnClick()
    {
        Toggle(false);
    }

    /// <summary>
    /// Delete all children in parent container
    /// </summary>
    private void ClearParentContainer()
    {
        // Find all children
        List<Transform> children = new List<Transform>();
        foreach (Transform child in carSelectionParent.transform)
        {
            children.Add(child);
        }

        // Delete all children
        for (int i = 0; i < children.Count; i++)
        {
            Destroy(children[i].gameObject);
        }
    }
}
