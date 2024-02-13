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

using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace May.Jarvis;
public class SoundMixer
{
    public async Task<int> GetVolumeAsync()
    {
        int result = 100;

        if (OperatingSystem.IsWindows())
        {
            using MMDeviceEnumerator enumerator = new();
            using MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            float volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            result = (int)Math.Floor(volume * 100);
        }
        else
        {
            using (Process mixerProcess = new())
            {
                mixerProcess.StartInfo.FileName = "amixer";
                mixerProcess.StartInfo.Arguments = $"-D pulse sget Master";
                mixerProcess.StartInfo.UseShellExecute = false;
                mixerProcess.StartInfo.RedirectStandardOutput = true;
                mixerProcess.StartInfo.RedirectStandardError = true;
                mixerProcess.EnableRaisingEvents = true;

                TaskCompletionSource<string> tcs = new();
                mixerProcess.Exited += (sender, e) =>
                {
                    string output = mixerProcess.StandardOutput.ReadToEnd();
                    tcs.SetResult(output);
                };

                mixerProcess.Start();

                await tcs.Task;
                string output = tcs.Task.Result;

                //find 1 to 3 digits with % enclosed in brackets
                Regex regex = new(@"\[(\d{1,3})%\]");
                Match match = regex.Match(output);

                if (match.Success)
                {
                    string volumeStr = match.Groups[1].Value;

                    bool success = int.TryParse(volumeStr, out int volume);

                    if (success)
                        result = volume;
                }
            }
        }

        return result;
    }

    public void SetVolume(int volume)
    {
        if (OperatingSystem.IsWindows())
        {
            using MMDeviceEnumerator enumerator = new();
            using MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
        }
        else
        {
            using Process mixerProcess = new();
            mixerProcess.StartInfo.FileName = "amixer";
            mixerProcess.StartInfo.Arguments = $"-D pulse sset Master {volume}%";
            mixerProcess.StartInfo.UseShellExecute = false;
            mixerProcess.StartInfo.RedirectStandardOutput = true;
            mixerProcess.StartInfo.RedirectStandardError = true;
            mixerProcess.Start();
        }
    }
}
