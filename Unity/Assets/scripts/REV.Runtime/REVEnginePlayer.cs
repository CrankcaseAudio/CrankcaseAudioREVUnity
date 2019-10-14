using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CrankcaseAudio.Wrappers;
using UnityEngine;
using UnityEngine.Audio;
using CrankcaseAudio = CrankcaseAudio.Wrappers.CrankcaseAudio;

namespace CrankcaseAudio.Unity
{
    /// <summary>
    /// REV engine player.
    /// Instantiate this to play a .mod file and
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class REVEnginePlayer : MonoBehaviour
    {
        public static String VERSION { get { return Wrappers.CrankcaseAudio.gREVVersion; } }

        public AudioSource audioSource {
            get{
                return GetComponent<AudioSource>();
                }
        }

        public enum eState
        {
            UnInitialized,
            Initialized,
            Running,
            Paused
        }

        /// <summary>
        /// Gets or sets the current playback state.
        /// </summary>
        /// <value>The state.</value>
        public eState State
        {
            get;
            protected set;
        }

        public TextAsset modelData;
        private byte[] modelFileBytes = null;
        private float[] modelAudioBuffer = null;

        private int modelChannels = -1;
        private int modelSampleRate = 0;
        

        public REVEnginePlayer()
        {
            State = eState.UnInitialized;
        }
        
        public VehiclePhysicsControlData PhysicsControlData
        {
            get;
            private set;
        }
        private Wrappers.IREVPlayer Player
        {
            get;
            set;
        }
        
        private Wrappers.REVPlayerUpdateParams updateParams = new Wrappers.REVPlayerUpdateParams();


        private int hwSampleRate;


        unsafe static bool LoadModel(byte[] modelData, REVEnginePlayer player)
        {

            fixed (byte* p = modelData)
            {  

                IntPtr ptr = (IntPtr) p;

                var rawData = new SWIGTYPE_p_void(ptr, false);

                Wrappers.ModelFixupError error = ModelFixupError.ModelFixupError_HeaderInvalid;
                var errorPtr =new Wrappers.SWIGTYPE_p_CrankcaseAudio__ModelFixupError((IntPtr)error, false);
                var voidPtr = new SWIGTYPE_p_p_void(SWIGTYPE_p_void.getCPtr(new SWIGTYPE_p_void(IntPtr.Zero, true)).Handle, true);
                

                if (!Wrappers.CrankcaseAudio.FixupModel(rawData, voidPtr, errorPtr))
                {
                    throw new Exception("CRANKCASEAUDIO:  Failed to fixup Model file. Error type:" + errorPtr.ToString());
                }


                int footprint = IREVPlayer.getMemoryFootprint();
                IntPtr mem = Marshal.AllocHGlobal(footprint);

                player.Player = IREVPlayer.construct(new SWIGTYPE_p_void(mem, false), footprint);

                
                //do your stu
                player.modelFileBytes = modelData;

                var modelVoid = new SWIGTYPE_p_void(ptr, false);
                player.Player.LoadData(modelVoid);


                player.modelChannels = (int) player.Player.getNumberChannels();
                player.modelSampleRate = (int) player.Player.getSampleRate();

                Debug.Log("CRANKCASEAUDIO:  HW rate:" + AudioSettings.outputSampleRate + " Model:" + player.modelSampleRate);
                if (player.modelSampleRate != AudioSettings.outputSampleRate)
                {
                    Debug.LogWarning("CRANKCASEAUDIO: Warning, will resample file. Performance hit. HW rate:" + AudioSettings.outputSampleRate + " Model:" + player.modelSampleRate);
                }

                var physicsControlData= Wrappers.CrankcaseAudio.GetREVPhysicsControlData(modelVoid);

                player.PhysicsControlData = physicsControlData;

                var logMessage = "Loading Engine: PlayerRate:" + player.modelSampleRate + " HWRate:" + AudioSettings.outputSampleRate;
                Debug.Log(logMessage);
                player.State = eState.Initialized;
                return true;
            }
            
        }

        virtual protected void Start()
        {
            this.hwSampleRate = AudioSettings.outputSampleRate;
            lock (this)
            {
                var modelBytes = modelData != null ? modelData.bytes : modelFileBytes;

                if (modelBytes != null)
                    if (modelBytes != null && State == eState.UnInitialized)
                    {
                        if (!LoadModel(modelBytes, this))
                        {
                            return;
                        }
                        StartEngine();
                    }
            }

        }

        /// <summary>
        /// Loads the model file data. The byte[] data should be the contents of a .mod file.
        /// </summary>
        /// <param name="modelFileData">Model file data.</param>
        public void LoadModelFileData(byte[] modelFileData)
        {
            lock (this)
            {
                if (State != eState.UnInitialized)
                {
                    throw new UnityException("Engine is already initialized.");
                }

                modelFileBytes = modelFileData;
                LoadModel(modelFileBytes, this);
            }

        }

        virtual protected void Update()
        {

            if (State != eState.Running)
                return;

            float deltaTime = Time.deltaTime;

            lock (updateParams)
            {
                Player.Update(updateParams, deltaTime);
                VisualRpm = Player.getVisualRPM();
            }

        }

        void OnRead(float[] data)
        {

        }

        void OnAudioFilterRead(float[] audioBuffer, int audioBufferChannels)
        {
            lock (this)
            {

                if (hwSampleRate == 0 || audioBufferChannels == 0 || audioBuffer.Length == 0)
                {
                    return;
                }

                if (State != eState.Running)
                {
                    return;
                }
                    
                
                if (Player != null)
                {
                    float[] bufferToWriteTo = null;
                    bool usedAltBuffer = safelyFetchBuffer(audioBuffer, audioBufferChannels, out bufferToWriteTo);
                    updatePlayerModel(modelFileBytes, bufferToWriteTo, modelChannels, this);

                    if (usedAltBuffer)
                    {
                        postProcessAudioBuffer(audioBuffer, audioBufferChannels, bufferToWriteTo, modelChannels);
                    }
                }
            }
        }

        private void postProcessAudioBuffer(float[] writeBuffer, int writeBufferChannels, float[] readlAudioBuffer, int readChannels)
        {
              
                for (int channel = 0; channel < writeBufferChannels; channel++)
                {
                    int readChannel = channel % readChannels;
                    resample(writeBuffer, writeBufferChannels, channel, readlAudioBuffer, readChannels, readChannel);
                }
        }


        static public void resample(float[] writeBuffer, int writeBufferChannels, int writeBufferWriteChannel, float[] readlAudioBuffer, int readChannels, int readBufferReadChannel)
        {

            int readNumberOfSamples = readlAudioBuffer.Length / readChannels;
            int writeNumberOfSamples = writeBuffer.Length / writeBufferChannels;


            for (int i = 0; i < writeNumberOfSamples - 1; i++)
            {
                double percent = (double)i / (double)(writeNumberOfSamples - 1);

                double findex = (double)(readNumberOfSamples - 1) * percent;
                int currentIndex = (int)findex;

                double currentSample =
                    readlAudioBuffer[currentIndex * readChannels + readBufferReadChannel];

                double nextSample =
                    readlAudioBuffer[(currentIndex + 1) * readChannels + readBufferReadChannel];

                double overHang = Math.IEEERemainder(findex, 1.0);
                if (overHang < 0.0)
                {
                    overHang = Math.IEEERemainder(findex - 0.5, 1.0);
                    overHang += 0.5;
                }

                double lerpSample = (nextSample - currentSample) * overHang + currentSample;

                writeBuffer[i* writeBufferChannels + writeBufferWriteChannel] = (float)(lerpSample);
            }

            writeBuffer[(writeNumberOfSamples - 1) * writeBufferChannels + writeBufferWriteChannel] =
                readlAudioBuffer[(readNumberOfSamples - 1) * readChannels + readBufferReadChannel];
        }

        //If the passed in buffer is the correct number of channels, use it, otherwise remap
        private bool safelyFetchBuffer(float[] inputAudioBuffer, int audioBufferChannels, out float[] bufferToWriteTo)
        {

            int outputSampleRate = hwSampleRate;

            //no need to process
            if (modelChannels == audioBufferChannels && outputSampleRate == modelSampleRate)
            {
                bufferToWriteTo = inputAudioBuffer;
                return false;
            }


            int sizeOfDesiredBuffer = modelChannels * inputAudioBuffer.Length / audioBufferChannels * modelSampleRate / outputSampleRate;

            if (modelAudioBuffer == null || modelAudioBuffer.Length != sizeOfDesiredBuffer)
            {
                modelAudioBuffer = new float[sizeOfDesiredBuffer];
            }

            bufferToWriteTo = modelAudioBuffer;
            return true;
        }


        private unsafe static void updatePlayerModel(byte[] modelData, float [] audioBuffer, int channels, REVEnginePlayer player)
        {
            fixed (byte* modelDataPtr = modelData)
            {
                fixed (float* audioBufferPtr = audioBuffer)
                {

                    var buffer = new Wrappers.Buffer();
                    buffer.Init(channels, new FloatArray((IntPtr)audioBufferPtr , false).cast(), audioBuffer.Length);

                    player.Player.Rebase(new SWIGTYPE_p_void((IntPtr)modelDataPtr, false));
                    player.Player.WriteBuffer(buffer);
                }
            }
        }

        /// <summary>
        /// Sets the throttle. Values should be from [0,1] 
        /// </summary>
        /// <value>The throttle.</value>
        public float Throttle
        {
            set
            {
                if (value > 1.0f)
                    value = 1.0f;
                else if (value < 0.0f)
                    value = 0.0f;
                    
                updateParams.Throttle = value;
            }
        }

        /// <summary>
        /// Sets the velocity of the car. [0, 200+ kph] in KPH. Normally only useful to detect off the line acceleration and not used there after.
        /// </summary>
        /// <value>The velocity.</value>
        public float Velocity
        {
            set
            {

                if (value < 0.0f)
                    value = 0.0f;
            
                updateParams.Velocity = value;
            }
        }

        /// <summary>
        /// Sets the volume. [0, 10+] Linear multiplier on the volume. Default 1.0f
        /// </summary>
        /// <value>The volume.</value>
        public float Volume
        {
            set
            {
                if (value < 0.0f)
                    value = 0.0f;
                
                updateParams.Volume = value;
            }
        }

        /// <summary>
        /// Sets the rpm. The RPM should be normalized between [0,1] where 0 is idle and 1 is REV Limiter. 
        /// </summary>
        /// <value>The rpm.</value>
        public float Rpm
        {
            set
            {
                if (value > 1.0f)
                    value = 1.0f;
                else if (value < 0.0f)
                    value = 0.0f;
            
                updateParams.Rpm = value;
            }
        }

        /// <summary>
        /// Sets the pitch. [0.1 , 2.0] default 1.0f. Pitches the player up and down.
        /// </summary>
        /// <value>The pitch.</value>
        public float Pitch
        {
            set
            {
                if (value > 2.0f)
                    value = 2.0f;
                else if (value < 0.1f)
                    value = 0.1f;


                updateParams.Pitch = value;
            }
        }


        /// <summary>
        /// Sets the gear. [1,6] There is no Neutral or Reverse. When going from 1st to 2nd, don't go through neutral. Keep the values steady.
        /// </summary>
        /// <value>The gear.</value>
        public int Gear
        {
            set
            {
                if (value > 6)
                    value = 6;
                else if (value < 1)
                    value = 1;
                
                updateParams.Gear = value;
            }
        }

        /// <summary>
        /// Gets the visual rpm. This value shows a smooth visually accurate representation 
        /// of what the tachometer should actually look like its doing, without any jitters or pops.
        /// </summary>
        /// <value>The visual rpm.</value>
        public float VisualRpm
        {
            private set;
            get;
        }

        /// <summary>
        /// Pauses the engine playback state.
        /// </summary>
        public void PauseEngine()
        {
            if (State == eState.Initialized || State == eState.Running)
            {
            
                
                var pausePAram = new REVPlayerUpdateParams();
                pausePAram.Pitch = updateParams.Pitch;
                pausePAram.Gear= updateParams.Gear;
                pausePAram.EnableShifting = updateParams.EnableShifting;
                pausePAram.Throttle = updateParams.Throttle;
                pausePAram.Velocity = updateParams.Velocity;
                pausePAram.Rpm = updateParams.Rpm;
                pausePAram.Volume = 0.0f;
                Player.Update(pausePAram, 0.0f);

                this.OnAudioFilterRead(modelAudioBuffer, modelChannels);
                State = eState.Paused;
            }
        }

        /// <summary>
        /// Starts/Resumes the playback state 
        /// </summary>
        public void StartEngine()
        {
            if(State == eState.Initialized || State == eState.Paused) {
                State = eState.Running;
            }
            
        }



        virtual protected void OnDestroy()
        {
            print("REVEnginePlayer was destroyed");
            Player = null;
            modelData = null;

        }
        
    }
}
