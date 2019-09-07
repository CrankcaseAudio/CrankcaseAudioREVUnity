using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLegacyUI : Main
{
    enum UIState
    {
        Main,
        SelectCar
    }

    UIState uiState = UIState.Main;
    float throttleSlider = 0.0f;
    float volume = 0.8f;
    float pitch = 1.0f;

    Dial Dial
    {
        get
        {
            return GetComponentInChildren<Dial>();
        }
    }

    RPM RPM
    {
        get
        {
            return GetComponentInChildren<RPM>();
        }
    }

    Vector3 scaleVector = Vector3.zero;

    private void Start()
    {
        base.Initialize();
        uiState = UIState.Main;
    }

    #region UI
    const int menuHeightPerButton = 30;
    const int menuSpacingPerButton = 40;
    public Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        try
        {
            float screenScaleX = (float)(Screen.width / 320.0);
            float screenScaleY = (float)(Screen.height / 480.0);
            Matrix4x4 scaledMatrix = Matrix4x4.identity;
            scaleVector = new Vector3(screenScaleX, screenScaleY, 1.0f);
            scaledMatrix = scaledMatrix * Matrix4x4.Scale(scaleVector);
            GUI.matrix = scaledMatrix;

            switch (this.uiState)
            {
                default:
                case UIState.Main:
                    RenderMain();
                    break;
                case UIState.SelectCar:
                    RenderSelect();
                    break;
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    void RenderSelect()
    {

        Dial.enabled = false;
        RPM.enabled = false;

        // Make a background box
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 320, 480), scrollPosition, new Rect(0, 0, 300, 20 + menuSpacingPerButton * demoModels.Count), false, true);

        GUI.Box(new Rect(10, 10, 300, 20 + menuSpacingPerButton * demoModels.Count), "Select Car");

        for (int i = 0; i < demoModels.Count; i++)
        {

            // Make the second button.
            if (GUI.Button(new Rect(20, 30 + menuSpacingPerButton * i, 280, 30), demoModels[i]))
            {
                LoadEngine(demoModels[i]);
            }
        }

        GUI.EndScrollView();
    }

    void RenderMain()
    {
        if (errorMessage != null)
        {
            GUI.Label(new Rect(10, 10, 280, 400), errorMessage);
            Dial.enabled = false;
            RPM.enabled = false;
            return;
        }


        GUI.Label(new Rect(10, 150, 50, 20), "v" + appVersion);

        Dial.enabled = true;
        RPM.enabled = true;


        // Make a background box
        GUI.Box(new Rect(10, 10, 300, 120), "Controls");

        Rect toggleButtonLocation = new Rect(20, 35, 100, 20);

        if (!isEngineRunning)
        {

            // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
            if (GUI.Button(toggleButtonLocation, "Start"))
            {
                StartEngine();
            }
        }
        else
        {

            // Make the second button.
            if (GUI.Button(toggleButtonLocation, "Stop"))
            {
                PauseEngine();
            }
        }

        if (GUI.Button(new Rect(140, 35, 150, 20), "Select Car"))
        {
            PauseEngine();
            uiState = UIState.SelectCar;
        }

        if (isEngineRunning)
        {
            UpdateThrottle();
            UpdateVolume();
            UpdatePitch();
        }
    }

    void UpdatePitch()
    {
        pitch = GUI.HorizontalSlider(new Rect(240, 100, 60, 20), pitch, 0.8f, 1.2f);
        GUI.Label(new Rect(200, 100, 60, 20), "Pitch");
        SetPitch(pitch);
    }

    void UpdateVolume()
    {
        volume = GUI.HorizontalSlider(new Rect(20, 80, 280, 20), volume, 0.0f, 1.0f);
        GUI.Label(new Rect(20, 60, 280, 20), "Volume");
        SetVolume(volume);
    }

    void UpdateThrottle()
    {
        throttleSlider = GUI.HorizontalSlider(new Rect(20, 460, 280, 20), throttleSlider, -0.30f, 0.7f);
        GUI.Label(new Rect(20, 430, 40, 20), "Brake");
        GUI.Label(new Rect(250, 430, 50, 20), "Throttle");
        GUI.Label(new Rect(100, 430, 40, 20), "Idle");

        float breaking = 0.0f;
        float throttle = 0.0f;

        if (throttleSlider < 0.0f)
        {
            float currentSliderValue = throttleSlider * -1.0f;
            currentSliderValue = currentSliderValue / 0.3f;
            breaking = currentSliderValue;
            throttle = 0.0f;
        }
        else if (throttleSlider >= 0.0f)
        {
            float currentSliderValue = throttleSlider / 0.7f;
            throttle = currentSliderValue;
            breaking = 0.0f;
        }

        
        SetThrottle(throttle, breaking);
        float angle = engineSimulator.VisualRpm * 270.0f;
        Dial.angle = angle;
    }

    #endregion

}
