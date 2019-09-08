using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CrankcaseAudio.Unity;
using CrankcaseAudio.Wrappers;
using UnityEngine.Assertions;
using UnityEngine.Audio;

public abstract class Main : MonoBehaviour
{
    public static String appVersion = "0.0";
    public static Main instance;

    //public AudioMixer audioMixer;
    protected CrankcaseAudio.Unity.REVEnginePlayer engineSimulator { get; private set; }
    private CrankcaseAudio.Unity.REVPhysicsSimulator physicsSimulator;
    
    public bool isEngineRunning { get; private set; }

    private GameObject car;

    #region MonoBehaviour

    public void Initialize()
    {
        Debug.Log("Starting up REV");
        instance = this;
    }

    // Update is called once per frame
    protected void Update ()
    {

//        if (errorMessage)
//        {
//
//        }

        if(engineSimulator != null && physicsSimulator != null)
        {
            REVPhysicsSimulator.PhysicsOutputParameters outputParams = physicsSimulator.OutputParams;
            engineSimulator.Throttle = outputParams.Throttle;
            engineSimulator.Velocity = outputParams.Velocity;
            engineSimulator.Rpm = outputParams.Rpm;
            engineSimulator.Gear = outputParams.Gear;
        }
    }

    #endregion

    #region Public Methods

    public virtual void SetThrottle(float throttle, float breaking)
    {
        if (physicsSimulator == null) return;

        REVPhysicsSimulator.PhysicsUpdateParams updateParams = new REVPhysicsSimulator.PhysicsUpdateParams();
        updateParams.Throttle = throttle;
        updateParams.Break = breaking;

        physicsSimulator.UpdateParams = updateParams;
    }

    public virtual void SetPitch(float pitch)
    {
        if (engineSimulator != null)
        {
            engineSimulator.Pitch = pitch;
        }
    }

    public virtual void SetVolume(float volume)
    {
        if (engineSimulator != null)
        {
            engineSimulator.Volume = volume;
        }
    }

    public void LoadEngine (string engineName)
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
        var filePath = "engines\\" + fileBasePath + engineName;

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
        
        SetVolume(0.8f);

        if (engineSimulator.PhysicsControlData != null)
        {
            physicsSimulator.LoadWithPhysicsControlData(engineSimulator.PhysicsControlData);
        }
        
        StartEngine();
    }

    public void StartEngine ()
    {
        if (physicsSimulator != null)
        {
            physicsSimulator.StartEngine ();
        }
        
        if(engineSimulator != null)
        {
            engineSimulator.StartEngine();
        }
        
        isEngineRunning = true;
    }

    public void PauseEngine ()
    {
        if (physicsSimulator != null)
        {
            physicsSimulator.PauseEngine();
        }
        
        if(engineSimulator != null)
        {
            engineSimulator.PauseEngine();
        }
        
        isEngineRunning = false;
    }

    public void DestroyEngine()
    {
        if (car != null)
        {
            //NOTE: Destroy immediately, otherwise the async destruction of the REVEnginePlayer may cause 2 engines to be in memory at the same time
            GameObject.DestroyImmediate(car);         
            car = null;
            physicsSimulator = null;
            engineSimulator = null;
        }

        isEngineRunning = false;
    }

    public float GetRPM()
    {
        return (isEngineRunning) ? engineSimulator.VisualRpm : 0;
    }

    #endregion
}
