
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class MainUGUI : Main
{
    [Header("UI")]
    public CanvasGroup canvas;
    public Button selectCarButton;
    public Slider volumeSlider;
    public Slider pitchSlider;
    public Slider throttleSlider;
    public Image fillImage;
    public TMPro.TextMeshProUGUI rpmText;
    public Animator animator;
    public GameObject popUpWindow;
    public GameObject errorWindow;
    public SubMenuSlider subMenuSlider;
    public TMPro.TextMeshProUGUI errorTextBox;

    private CarSelectionManager carSelectionManager;

    private IEnumerator Start()
    {
        base.Initialize();

        ShowPopUp(false);   
        HideError();
        try {

            carSelectionManager = FindObjectOfType<CarSelectionManager>();
            carSelectionManager.Initialize();
            carSelectionManager.onSelectItem += CarSelectionManager_onSelectItem;
            carSelectionManager.onBackButton += CarSelectionManagerOnOnBackButton;

            
            pitchSlider.minValue = 0.8f;
            pitchSlider.maxValue = 1.2f;
            pitchSlider.value = 1f;

            throttleSlider.minValue = -0.3f;
            throttleSlider.maxValue = 0.7f;

            volumeSlider.value = 0.8f;

            // Add handlers after setting initial values
            selectCarButton.onClick.AddListener(SelectButton_OnClick);
            pitchSlider.onValueChanged.AddListener(PitchSlider_OnValueChanged);
            volumeSlider.onValueChanged.AddListener(VolumeSlider_OnValueChanged);
            throttleSlider.onValueChanged.AddListener(ThrottleSlider_OnValueChanged);

        }
        catch (Exception exception)
        {
            ShowError(exception.ToString());
        }


        // Wait animation to end
        float animationDuration = this.animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration + 0.5f);

        // Unlock elements
        animator.enabled = false;

        try
        {
            //LoadEngineFirstEngine();
        }
        catch (Exception exception)
        {
            ShowError(exception.ToString());
        }
        
    }

    private void CarSelectionManagerOnOnBackButton(object sender, EventArgs e)
    {
        this.StartEngine();
        ToggleCarSelection(false);
    }


    protected new void Update()
    {
        if (!base.isEngineRunning) return;

        base.Update();

        // There is an offset visually, so add offset to roughly match
        float visualRpm = HelperFunctions.Retarget(base.GetRPM(), 0f, 1f, 0.15f, 0.85f);
        fillImage.fillAmount = visualRpm;

        // Remap rpm (0-1) to (1000-8000)
        float textRpm = HelperFunctions.Retarget(base.GetRPM(), 0f, 1f, 1000f, 8000f);
        rpmText.text = textRpm.ToString("0000");
    }

    protected void LoadEngineFirstEngine()
    {
        var car = carSelectionManager.FirstCar();
        carSelectionManager.SelectCar(car);
        this.LoadEngine(car);
    }

    private IEnumerator StartEngineDelay(CarSelectionItem.CarEventArgs e, float delay = 1.0f)
    {
        yield return new WaitForSeconds(delay);
        print("StartEngineDelay: WaitForSeconds");
        ToggleCarSelection(false);
        base.LoadEngine(e.car);
    }

    private void ToggleCarSelection(bool visible)
    {
        canvas.alpha = (!visible) ? 1 : 0;
        canvas.interactable = !visible;

        carSelectionManager.Toggle(visible);
    }

    public void HideError()
    {
        errorWindow.SetActive(false);
    }


    public void ShowError(String message)
    {
        print(message);
        errorWindow.SetActive(true);
        const int MAX_MESSAGE_LENGTH = 1000;
        if (message.Length > MAX_MESSAGE_LENGTH)
        {
            message = message.Substring(0, MAX_MESSAGE_LENGTH);
        }
        errorTextBox.text = message;
    }

    public void ShowPopUp(bool show)
    {
        // Hide menu for NonMobile
        if (!Application.isMobilePlatform && show)
            subMenuSlider.ToggleSlide();

        popUpWindow.SetActive(show);
    }

    public void ToggleSlide()
    {
        if (Application.isMobilePlatform)
        {
            popUpWindow.SetActive(true);
        }
        else
        {
            subMenuSlider.ToggleSlide();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    #region EventHandlers

    private void CarSelectionManager_onSelectItem(object sender, CarSelectionItem.CarEventArgs e)
    {
        base.FadeEngine();
        this.throttleSlider.value = 0.0f;
        StartCoroutine(StartEngineDelay(e, 0.8f));
    }

    private void SelectButton_OnClick()
    {
        this.PauseEngine();
        ToggleCarSelection(true);
    }

    private void VolumeSlider_OnValueChanged(float arg0)
    {
        base.SetVolume(arg0);
    }

    private void PitchSlider_OnValueChanged(float arg0)
    {
        base.SetPitch(arg0);
    }

    private void ThrottleSlider_OnValueChanged(float arg0)
    {
        // Break slider values into 2 chunks. 30% for brake. 70% for throttle
        float throttle = 0;
        if (arg0 >= 0f)
        {
            throttle = HelperFunctions.Retarget(arg0, 0f, 0.7f, 0f, 1f);
        }
        float breaking = 0f;
        if (arg0 < 0f)
        {
            arg0 *= -1f;
            breaking = HelperFunctions.Retarget(arg0, 0f, 0.3f, 0f, 1f);
        }
        base.SetThrottle(throttle, breaking);
    }

    #endregion

}
