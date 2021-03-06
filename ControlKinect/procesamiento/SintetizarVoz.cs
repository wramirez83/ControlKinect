﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace ControlKinect.procesamiento
{
    class SintetizarVoz
    {
        SpeechSynthesizer syn = new SpeechSynthesizer();
        public void Config()
        {
            syn.SetOutputToDefaultAudioDevice();
            syn.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
        }
        public void hablar(string palabra)
        {
            syn.Speak(palabra);
        }
    }
}
