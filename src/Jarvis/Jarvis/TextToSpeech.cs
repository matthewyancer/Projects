// Copyright 2024 Matthew Yancer
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics;
using System.Text.RegularExpressions;
using May.Jarvis.Settings;

namespace May.Jarvis;
public class TextToSpeech : IDisposable
{
    private readonly TextToSpeechSettings m_Settings;

    private Process? m_SpeakingProcess;

    public TextToSpeech(TextToSpeechSettings settings)
    {
        m_Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void Speak(string text)
    {
        StopSpeaking();

        string phoneticText = ApplyPhoneticRules(text);

        m_SpeakingProcess = new();
        m_SpeakingProcess.StartInfo.FileName = m_Settings.ExecutableName;
        m_SpeakingProcess.StartInfo.Arguments =
            $"-v {m_Settings.Voice} -s {m_Settings.Speed} -a {m_Settings.Volume} \"{phoneticText.Replace("\"", string.Empty)}\"";
        m_SpeakingProcess.Start();
    }

    public void StopSpeaking()
    {
        if(m_SpeakingProcess != null)
        {
            m_SpeakingProcess.Kill();
            m_SpeakingProcess.Dispose();
            m_SpeakingProcess = null;
        }
    }

    private string ApplyPhoneticRules(string text)
    {
        string phoneticText = text;

        phoneticText = Regex.Replace(phoneticText, @"[Cc]#", "C Sharp");

        phoneticText = Regex.Replace(phoneticText, @"[Qq]uinoa", "Keen Waa");

        return phoneticText;
    }

    public void Dispose()
    {
        if (m_SpeakingProcess != null)
        {
            m_SpeakingProcess.Dispose();
            m_SpeakingProcess = null;
        }

        GC.SuppressFinalize(this);
    }
}
