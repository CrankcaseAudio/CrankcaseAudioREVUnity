www.crankcaseaudio.com

## What is REV
REV is a unique engine audio creation system that utilizes spectral analysis and granular synthesis techniques to manipulate sound recordings of vehicles in motion and reproduce them in a very realistic way, while offering complete control of playback speed and direction. In essence, it allows you to utilize a very economical set of source recordings, and produce a great sounding vehicle in your game with a minimum time investment

## REV Unity Demo 
REV Demo is a demo of the tachometer simulator of Rev, also available for [download here](http://www.crankcaseaudio.com/download) along with other variants of downloads.

## Licensing 
Your access to and use of REV on GitHub is governed by the [License Agreement](LICENSE.md). If you don't agree to those terms, as amended from time to time, you are not permitted to access or use REV.


## Installing the demo into your own app
1) Copy contents of Unity\Assets\scripts\ to your project
2) Copy plugin binaries from various source folders do your destination folders

* Android : Unity\Assets\Plugins\Android\Libs\**\libREVRuntime.so
* iOS :     Unity\Assets\Plugins\iOS\libREVRuntime.a
* MacOSX :  Unity\Assets\Plugins\REVRuntime.bundle\
* x64 :   Unity\Assets\Plugins\x86_64\REVRuntime.dll, Unity\Assets\Plugins\x86_64\REVRuntime.lib
   
    
Example Setup:

```
void Start()
{
    car = new GameObject("Car");
    car.AddComponent<CrankcaseAudio.Unity.REVPhysicsSimulator>(); //OPTIONAL
    car.AddComponent<CrankcaseAudio.Unity.REVEnginePlayer>();

    byte [] fileData = null;
    var asset  = Resources.Load<TextAsset>(enginName ); 
    fileData = asset.bytes;

    engineSimulator.LoadModelFileData (fileData);
    engineSimulator.Volume = 1.0f;

    //OPTIONAL
    physicsSimulator.LoadWithPhysicsControlData(engineSimulator.ModelFileHeader.mVehiclePhysicsControlData);
}

void Update()
{
    //If you're using physics simulator.
    if(physicsSimulator != null)
    {
        CrankcaseAudio.Mono.EngineSimulationUpdateParams outputParams = physicsSimulator.OutputParams;
        engineSimulator.Throttle = outputParams.Throttle;
        engineSimulator.Velocity = outputParams.Velocity;
        engineSimulator.Rpm = outputParams.Rpm;
        engineSimulator.Gear = outputParams.Gear;
    }
    else{ //Use the physics from your own game.
        engineSimulator.Volume = volume;
        engineSimulator.Throttle = gamePhysics.Throttle;
        engineSimulator.Velocity = gamePhysics.Velocity;
        engineSimulator.Rpm = gamePhysics.Rpm;
        engineSimulator.Gear = gamePhysics.Gear;
    }
}
```

### Contents

##### scripts/REV.Runtime/.*
These are wrapper classes for the native C++ library.
    
##### REVPhysicsSimulator
This class is largely for demo purposes. It provides the engine simulation with the needed physics simulation values. Most car video games will have their own physics engine and be able to provide the needed RPM/Throttle/Gear/Velocity values that the engine simulation requires.
    
##### REVEnginePlayer
This is the core class your car game will likely need to instantiate. Update the engine with the needed values taken from physics like RPM/Throttle/Gear/... 
    
##### Binary Assets : Camaro_ss.bytes or You're own model 
The Binary assets that REVEnginePlayer loads should be renamed from .mod extension to .bytes for use in Unity. The assets can be generated by REV.Tool if you've purchased a release license. 