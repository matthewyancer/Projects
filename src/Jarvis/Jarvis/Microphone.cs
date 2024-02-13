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

using NAudio.Wave;
using System.Diagnostics;

namespace May.Jarvis
{
    public class Microphone : IDisposable
    {
        private const int RATE = 16000;
        private const int CHANNELS = 1;

        private readonly MemoryStream m_MicrophoneStream = new();

        private Process? m_NonWindows_RecordingProcess;
        private WaveInEvent? m_Windows_WaveInEvent;

        public void StartRecording()
        {
            if (OperatingSystem.IsWindows())
            {
                if (m_Windows_WaveInEvent == null)
                {
                    m_Windows_WaveInEvent = new WaveInEvent()
                    {
                        DeviceNumber = 0,
                        WaveFormat = new WaveFormat(RATE, 16, CHANNELS),
                        BufferMilliseconds = 20
                    };

                    //use event to copy recording into memory until stopped
                    m_Windows_WaveInEvent.DataAvailable += (sender, e) =>
                    {
                        m_MicrophoneStream.Write(e.Buffer, 0, e.BytesRecorded);
                    };
                }

                try
                {
                    m_Windows_WaveInEvent.StartRecording();
                }
                catch { }   //No microphone
            }
            else
            {
                //prep for new round of recording
                if (m_NonWindows_RecordingProcess != null)
                {
                    m_NonWindows_RecordingProcess.Dispose();
                    m_NonWindows_RecordingProcess = null;
                }

                //start speech recording
                m_NonWindows_RecordingProcess = new();
                m_NonWindows_RecordingProcess.StartInfo.FileName = "arecord";
                m_NonWindows_RecordingProcess.StartInfo.Arguments = $"-f cd -r {RATE} -c {CHANNELS} -t wav -D default -q";
                m_NonWindows_RecordingProcess.StartInfo.UseShellExecute = false;
                m_NonWindows_RecordingProcess.StartInfo.RedirectStandardOutput = true;
                m_NonWindows_RecordingProcess.StartInfo.RedirectStandardError = true;

                //use separate thread to copy recording into memory until process is killed
                Thread thread = new(() =>
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = m_NonWindows_RecordingProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        m_MicrophoneStream.Write(buffer, 0, bytesRead);
                    }
                });

                m_NonWindows_RecordingProcess.Start();

                thread.Start();
            }
        }

        public byte[] StopRecording()
        {
            byte[] result = Array.Empty<byte>();

            if (OperatingSystem.IsWindows())
            {
                if (m_Windows_WaveInEvent != null)
                {
                    try
                    {
                        m_Windows_WaveInEvent.StopRecording();
                    }
                    catch { }
                }

                if ((m_MicrophoneStream != null) && (m_MicrophoneStream.Length > 0))
                {
                    byte[] rawMicrophoneData = m_MicrophoneStream.ToArray();
                    m_MicrophoneStream.SetLength(0);

                    //convert raw microphone data to in-memory .wav file
                    using (MemoryStream memoryStream = new())
                    {
                        using (WaveFileWriter writer = new(memoryStream, new WaveFormat(RATE, 16, CHANNELS)))
                        {
                            writer.Write(rawMicrophoneData, 0, rawMicrophoneData.Length);
                            result = memoryStream.ToArray();
                        }
                    }
                }
            }
            else
            {
                if (m_NonWindows_RecordingProcess != null)
                {
                    //stop recording
                    m_NonWindows_RecordingProcess.Kill();

                    if (m_MicrophoneStream != null)
                    {
                        //microphone data already in .wav file format
                        result = m_MicrophoneStream.ToArray();
                        m_MicrophoneStream.SetLength(0);
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (m_NonWindows_RecordingProcess != null)
            {
                m_NonWindows_RecordingProcess.Kill();
                m_NonWindows_RecordingProcess.Dispose();
                m_NonWindows_RecordingProcess = null;
            }

            if (m_Windows_WaveInEvent != null)
            {
                try
                {
                    m_Windows_WaveInEvent.StopRecording();
                }
                catch { }

                m_Windows_WaveInEvent.Dispose();
                m_Windows_WaveInEvent = null;
            }

            m_MicrophoneStream.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
