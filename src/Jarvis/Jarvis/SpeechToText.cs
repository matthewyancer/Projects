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

using Azure.AI.OpenAI;
using May.Jarvis.Settings;

namespace May.Jarvis;
public class SpeechToText
{
    private readonly SoundMixer m_SoundMixer;
    private readonly Microphone m_Microphone;
    private readonly SpeechToTextSettings m_Settings;
    private readonly string m_OpenAiKey;

    private int m_SavedMixerVolume = 100;

    public SpeechToText(SoundMixer soundMixer, Microphone microphone, SpeechToTextSettings settings, string? openAiKey)
    {
        if(string.IsNullOrEmpty(openAiKey))
            throw new ArgumentNullException(nameof(openAiKey));

        m_SoundMixer = soundMixer ?? throw new ArgumentNullException(nameof(soundMixer));
        m_Microphone = microphone ?? throw new ArgumentNullException(nameof(microphone));
        m_Settings = settings ?? throw new ArgumentNullException(nameof(settings));

        m_OpenAiKey = openAiKey;
    }

    public void PushToTalkPressing(object? sender, EventArgs e)
    {
        Task.Run(async () =>
        {
            m_SavedMixerVolume = await m_SoundMixer.GetVolumeAsync();

            int reducedVolumeLevel = Convert.ToInt32(m_Settings.ReducedVolumeLevel);

            if (m_SavedMixerVolume > reducedVolumeLevel)
                m_SoundMixer.SetVolume(reducedVolumeLevel);  //reduce volume so Jarvis doesn't talk to himself
        }).Wait();

        m_Microphone.StartRecording();
    }

    public string? PushToTalkPressed(object? sender, EventArgs e)
    {
        string? result = null;

        byte[] microphoneData = m_Microphone.StopRecording();

        m_SoundMixer.SetVolume(m_SavedMixerVolume);  //restore volume to previous level

        if (microphoneData.Length > 0)
        {
            //set whisper api options
            AudioTranscriptionOptions transcriptionOptions = new()
            {
                DeploymentName = "whisper-1",
                Language = m_Settings.Language,
                ResponseFormat = AudioTranscriptionFormat.Simple,
                Filename = "speechtotext.wav",
                AudioData = BinaryData.FromBytes(microphoneData)
            };

            //use whisper api to convert speech to text
            OpenAIClient openAiClient = new(m_OpenAiKey);
            Azure.Response<AudioTranscription> openAiResponse = openAiClient.GetAudioTranscription(transcriptionOptions);
            result = openAiResponse.Value.Text;
        }

        return result;
    }
}
