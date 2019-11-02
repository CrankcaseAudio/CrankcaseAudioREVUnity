using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CrankcaseAudio.Unity;
using CrankcaseAudio.Wrappers;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.Audio;

public abstract class Main : MonoBehaviour
{
    public static String appVersion = "0.0";
    public static Main instance;

    //public AudioMixer audioMixer;
    protected CrankcaseAudio.Unity.REVEnginePlayer engineSimulator { get; private set; }
    private CrankcaseAudio.Unity.REVPhysicsSimulator physicsSimulator;
    

    //Sensible fade in/out duration    
    private static float FADE_DURATION_MS = 0.4f;
    private float fadeVolume = 0.0f;
    private float masterVolume = 1.0f;
    private CrankcaseAudio.Fade fader = null;

    
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

        if(engineSimulator != null && physicsSimulator != null)
        {

            float deltaTime = Time.deltaTime;
            if(fader != null){
                this.fadeVolume = fader.Update(deltaTime);
                if(fader.isComplete) {
                    fader = null;
                }
                engineSimulator.Volume = this.masterVolume * this.fadeVolume;
            }


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
        this.masterVolume = volume;
        if (engineSimulator != null)
        {
            engineSimulator.Volume = this.masterVolume * this.fadeVolume;
        }
    }

    public void LoadEngine (Car carData)
    {
        return;
//
//        appVersion = REVEnginePlayer.VERSION;
//
//        
//        DestroyEngine();        
//
//        car = new GameObject("Car");
//        car.AddComponent<CrankcaseAudio.Unity.REVPhysicsSimulator>();
//        car.AddComponent<CrankcaseAudio.Unity.REVEnginePlayer>();
//        
//        
//        if(!car)
//            throw new UnityException("Failed to instantiate Car class");
//        
//        physicsSimulator = car.GetComponentInChildren<CrankcaseAudio.Unity.REVPhysicsSimulator>();
//        if(!physicsSimulator)
//            throw new UnityException("Failed to instantiate REVPhysicsSimulator class");
//        
//        engineSimulator =  car.GetComponentInChildren<CrankcaseAudio.Unity.REVEnginePlayer>();
//        if(!engineSimulator)
//            throw new UnityException("Failed to instantiate REVEnginePlayer class");
//
//    
////        string _OutputMixer = "Engine";
////        var engineMixer = audioMixer.FindMatchingGroups(_OutputMixer);
////        engineSimulator.audioSource.outputAudioMixerGroup = engineMixer[0];
//        
//        var fileData = carData.LoadEngineData();
//
//        engineSimulator.LoadModelFileData(fileData);
//
//        if (engineSimulator.PhysicsControlData != null)
//        {
//            physicsSimulator.LoadWithPhysicsControlData(engineSimulator.PhysicsControlData);
//        }
//        
//        StartEngine();
    }

    public void FadeEngine(Action completionAction = null)  {
        fader = new CrankcaseAudio.Fade(FADE_DURATION_MS, 1.0f, 0.0f, CrankcaseAudio.eCurveType.TO_THE_HALF);
        fader.completionAction = completionAction;
    }

    public void StartEngine ()
    {
        fader = new CrankcaseAudio.Fade(FADE_DURATION_MS, 0.0f, 1.0f, CrankcaseAudio.eCurveType.SQRD);

        if (physicsSimulator != null)
        {
            physicsSimulator.StartEngine ();
        }
        
        if(engineSimulator != null)
        {
            engineSimulator.Volume = 0.0f;
            engineSimulator.StartEngine();
        }
        
        isEngineRunning = true;
    }

    public void PauseEngine ()
    {
        FadeEngine(() =>
        {
            if (physicsSimulator != null)
            {
                physicsSimulator.PauseEngine();
            }

            if (engineSimulator != null)
            {
                engineSimulator.PauseEngine();
            }

            isEngineRunning = false;

        });
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
