using System;
using System.Collections;
using System.Collections.Generic;
using CrankcaseAudio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour
{
    public event EventHandler<CarSelectionItem.CarEventArgs> onSelectItem;
    public event EventHandler<EventArgs> onBackButton;

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

    public void SelectCar(Car car)
    {
        var indexOfCar = cars.IndexOf(car);
        if (indexOfCar == -1)
        {
            return;
        }

        for (int i = 0; i < carItems.Count; i++)
        {
            var carItem = carItems[i];
            carItem.SetSelected(indexOfCar == i);
        }
        
    }

    public void Initialize()
    {
        this.gameObject.SetActive(false);

        backButton.onClick.AddListener(BackButton_OnClick);
        cars = IOHelper.ReadConfigFile(configFilename);
        CreateCarList();
    }

    private void ScrollTo(int index)
    {
        if (scrollAnimationCR != null)
            StopCoroutine(scrollAnimationCR);

        if (index >= cars.Count - 2)
        {
            scrollAnimationCR = StartCoroutine(ScrollToBottom());
            return;
        } else if (index > 0)
        {
            index -= 1;
        }

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
            scrollRect.verticalScrollbar.value = (float) Curve.Lerp(initialValue, targetValue, lerp, eCurveType.S_CURVE);
            yield return null;
        }
    }


    private IEnumerator ScrollToBottom()
    {
        float timeStarted = Time.time;

        float initialValue = scrollRect.verticalScrollbar.value;
        float targetValue = 0;
        while (Time.time - timeStarted <= scrollAnimationDuration)
        {
            float lerp = (Time.time - timeStarted) / scrollAnimationDuration;
            scrollRect.verticalScrollbar.value = (float) Curve.Lerp(initialValue, targetValue, lerp, eCurveType.S_CURVE);
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
        ScrollTo(item.transform.GetSiblingIndex());
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
        onBackButton?.Invoke(this, EventArgs.Empty);
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
