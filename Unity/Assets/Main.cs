using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CrankcaseAudio.Unity;
using CrankcaseAudio.Wrappers;
using UnityEngine.Assertions;
using UnityEngine.Audio;

public class Main : MonoBehaviour
{
    public AudioMixer audioMixer;
    // Use this for initialization
    CrankcaseAudio.Unity.REVPhysicsSimulator physicsSimulator;
    CrankcaseAudio.Unity.REVEnginePlayer engineSimulator;
    GameObject car;
    
    float throttleSlider = 0.0f;

    private String appVersion = "0.0";
    
    bool EngineRunning = false;
    
    
    List<string> demoModels = new List<string>
    {
#if APPSTORE
        "Volkswagen_Golf_VR6_2004",
        "Toyota_GT86",
        "Porsche_997",
        "Pontiac_Grand_Prix",
        "Nissan_370z",
        "Mazda_RX8_2006",
        "Mazda_RX8_2006_v2",
        "Honda_NSX_1991",
        "Honda_Civic",
        "Ford_RS200",
        "Ford_Mustang",
        "Ferrari_575M",
        "Ferrari_430",
        "Dodge_Viper",
        "Dodge_Challenger_Sixpack_1970",
        "Dodge_Challenger_Sixpack_1970_v2",
        "De_Tomaso_Pantera",
        "Chevrolet_Corvette_Z06_2006",
        "Chevrolet_Corvette_Z06_2006_v2",
        "Chevrolet_Camaro_Super_Hugger",
        "Chevrolet_Camaro_SS",
        "Camaro_SS",
        "Audi_R8",
        "Alfa_Romeo_8C_Competizione",
        "Acura_Integra_1992"
#else 
        "Camaro_SS"
#endif
    };
    
    UIState uiState = UIState.Main;
    
    Dial Dial
    {
        get
        {
            return GetComponentInChildren<Dial> ();
        }
    }
    
    RPM RPM
    {
        get
        {
            return GetComponentInChildren<RPM> ();
        }
    }
    
    Vector3 scaleVector = Vector3.zero;
    
    enum UIState
    {
        Main,
        SelectCar
    }
    
    string errorMessage;
    void Start ()
    {
        
        Debug.Log("Starting up REV");

        //Size to the largest model you have, in bytes, as well as the number of instances you will want.
        try
        {
            LoadEngine(demoModels[0]);
            StartEngine();
        }
        catch(System.Exception ex)
        {
            errorMessage = ex.ToString();
        }

    }
    
    // Update is called once per frame
    void Update ()
    {
        
        if(engineSimulator != null && physicsSimulator != null)
        {
            REVPhysicsSimulator.PhysicsOutputParameters outputParams = physicsSimulator.OutputParams;
            engineSimulator.Throttle = outputParams.Throttle;
            engineSimulator.Velocity = outputParams.Velocity;
            engineSimulator.Rpm = outputParams.Rpm;
            engineSimulator.Gear = outputParams.Gear;
        }
    }
    
    void RenderMain ()
    {
        if(errorMessage != null)
        {
            GUI.Label(new Rect(10,10,280,400),errorMessage);
            Dial.enabled =false;
            RPM.enabled = false;
            return;
        }
        
        
        GUI.Label(new Rect(10,150,50,20), "v"+ appVersion);
        
        Dial.enabled = true;
        RPM.enabled = true;
        
        
        // Make a background box
        GUI.Box (new Rect (10, 10, 300, 120), "Controls");
        
        Rect toggleButtonLocation = new Rect (20, 35, 100, 20);
        
        if (!EngineRunning)
        {
            
            // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
            if (GUI.Button(toggleButtonLocation, "Start"))
            {
                StartEngine ();
            }
        }
        else
        {
            
            // Make the second button.
            if (GUI.Button (toggleButtonLocation, "Stop"))
            {
                PauseEngine();
            }
        }
        
        if(GUI.Button(new Rect(140,35, 150,20),"Select Car"))
        {
            PauseEngine();
            uiState = UIState.SelectCar;
        }
        
        if(EngineRunning)
        {
            UpdateThrottle ();
            UpdateVolume ();
            UpdatePitch ();
        }
    }
    
    float volume = 0.8f;
    
    void UpdateVolume ()
    {
        volume = GUI.HorizontalSlider (new Rect (20, 80, 280, 20), volume, 0.0f, 1.0f);
        GUI.Label(new Rect(20,60,280,20),"Volume");
        
        if (engineSimulator != null)
        {
            engineSimulator.Volume = volume;
            
        }
    }
    
    
    float pitch = 1.0f;
    
    void UpdatePitch()
    {
        pitch = GUI.HorizontalSlider(new Rect(240,100,60,20),pitch,0.8f,1.2f);
        GUI.Label(new Rect(200,100,60,20),"Pitch");
        if (engineSimulator != null)
        {
            engineSimulator.Pitch = pitch;
        }
    }
    void UpdateThrottle ()
    {
        throttleSlider = GUI.HorizontalSlider (new Rect (20, 460, 280, 20), throttleSlider, -0.30f, 0.7f);
        GUI.Label (new Rect (20, 430, 40, 20), "Brake");
        GUI.Label (new Rect (250, 430, 50, 20), "Throttle");
        GUI.Label (new Rect (100, 430, 40, 20), "Idle");
        
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
        
        
        if (physicsSimulator != null)
        {
            REVPhysicsSimulator.PhysicsUpdateParams updateParams = new REVPhysicsSimulator.PhysicsUpdateParams();
            updateParams.Throttle = throttle;
            updateParams.Break = breaking;
            
            physicsSimulator.UpdateParams = updateParams;
            
            float angle = engineSimulator.VisualRpm * 270.0f;
            Dial.angle = angle;
        }
    }
    
    const int menuHeightPerButton = 30;
    const int menuSpacingPerButton = 40;

    public Vector2 scrollPosition = Vector2.zero;

    void RenderSelect () {
        
        Dial.enabled = false;
        RPM.enabled = false;
        
        // Make a background box
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 320,480), scrollPosition, new Rect(0,0,300,20 + menuSpacingPerButton * demoModels.Count),false,true);

        GUI.Box (new Rect (10, 10, 300, 20 + menuSpacingPerButton * demoModels.Count), "Select Car");

        

        for (int i =0; i < demoModels.Count; i++) {

            // Make the second button.
            if (GUI.Button (new Rect (20, 30 + menuSpacingPerButton * i, 280, 30), demoModels [i])) {
                LoadEngine (demoModels [i]);
            }
        }

        GUI.EndScrollView();
    }
    
    void OnGUI ()
    {
        try
        {
            float screenScaleX = (float)(Screen.width / 320.0);
            float screenScaleY = (float)(Screen.height / 480.0);
            Matrix4x4 scaledMatrix = Matrix4x4.identity;
            scaleVector = new Vector3 (screenScaleX, screenScaleY, 1.0f);
            scaledMatrix = scaledMatrix * Matrix4x4.Scale (scaleVector);
            GUI.matrix = scaledMatrix;
            
            switch (this.uiState)
            {
                default:
                case UIState.Main:
                    RenderMain ();
                    break;
                case UIState.SelectCar:
                    RenderSelect ();
                    break;
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.Log (ex.ToString ());
        }
    }
    
    void LoadEngine (string enginName)
    {
        appVersion = REVEnginePlayer.VERSION;

        var fileBasePath = AudioSettings.outputSampleRate.ToString() + "\\";
            

        DestroyEngine();
        

        car = new GameObject("Car");
        car.AddComponent<CrankcaseAudio.Unity.REVPhysicsSimulator>();
        car.AddComponent<CrankcaseAudio.Unity.REVEnginePlayer>();
        
        
        if(!car)
            throw new UnityException("Failed to instantiate Car class");
        
        physicsSimulator = car.GetComponentInChildren<CrankcaseAudio.Unity.REVPhysicsSimulator>();
        if(!physicsSimulator)
            throw new UnityException("Failed to instantiate REVPhysicsSimulator class");
        
        engineSimulator =  car.GetComponentInChildren<CrankcaseAudio.Unity.REVEnginePlayer>();
        if(!engineSimulator)
            throw new UnityException("Failed to instantiate REVEnginePlayer class");

    
//        string _OutputMixer = "Engine";
//        var engineMixer = audioMixer.FindMatchingGroups(_OutputMixer);
//        engineSimulator.audioSource.outputAudioMixerGroup = engineMixer[0];
        


#if APPSTORE
        fileBasePath = "encrypted\\" + fileBasePath;
#endif
        var filePath = "engines\\" + fileBasePath + enginName;

        Debug.Log ("Load Engine: " + filePath);
        
        byte [] fileData = null;
        
        var asset  = Resources.Load<TextAsset>(filePath);
        
        if(asset == null)
            throw new UnityException("No asset:" + filePath);
        
        fileData = asset.bytes;
        asset = null;
        Resources.UnloadUnusedAssets( );

#if APPSTORE
        fileData = REVDemo.Security.Decode(fileData);
#endif

        engineSimulator.LoadModelFileData(fileData);
        engineSimulator.Volume = volume;

        if (engineSimulator.PhysicsControlData != null)
        {
            physicsSimulator.LoadWithPhysicsControlData(engineSimulator.PhysicsControlData);
        }
        uiState = UIState.Main;
        
        EngineRunning = false;

    }
    
    
    void StartEngine ()
    {
        if (physicsSimulator != null)
        {
            physicsSimulator.StartEngine ();
        }
        
        if(engineSimulator != null)
        {
            engineSimulator.StartEngine();
        }
        
        EngineRunning = true;
        
    }
    
    void PauseEngine ()
    {
        if (physicsSimulator != null)
        {
            physicsSimulator.PauseEngine();
        }
        
        if(engineSimulator != null)
        {
            engineSimulator.PauseEngine();
        }
        
        EngineRunning = false;
        
    }

    IEnumerator WaitForCarToDestroy() {
        print("Waiting until car is ready");
        yield return new WaitForSeconds(0.1f);
    }
    
    void DestroyEngine()
    {
        if (car != null)
        {
            //NOTE: Destroy immediately, otherwise the async destruction of the REVEnginePlayer may cause 2 engines to be in memory at the same
            //time.
            GameObject.DestroyImmediate(car);

         
            car = null;
            physicsSimulator = null;
            engineSimulator = null;
        }

        EngineRunning = false;
    }
    
}
