using System;
using CrankcaseAudio.Unity;
using TMPro;
using UnityEngine.Experimental.PlayerLoop;

namespace Assets.scripts
{
    public class AboutText: TextMeshProUGUI
    {
        protected override void Awake()
        {
            base.Awake();
            m_text = String.Format("Crankcase Audio Inc.\n2019 All Rights Reserved\nwww.crankcaseaudio.com\nsupport@crankcaseaudio.com\ngithub.com/CrankcaseAudio\nv{0}", REVEnginePlayer.VERSION);

        }
    }

    
}