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

namespace May.Jarvis.Settings;
public class TextToSpeechSettings
{
    private string m_ExecutableName = "espeak-ng";
    private string m_Voice = "en-gb-x-rp";
    private string m_Speed = "162";
    private string m_Volume = "90";

    public string? ExecutableName
    {
        get
        {
            return m_ExecutableName;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_ExecutableName = value;
        }
    }

    public string? Voice
    {
        get
        {
            return m_Voice;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_Voice = value;
        }
    }

    public string? Speed
    {
        get
        {
            return m_Speed;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_Speed = value;
        }
    }

    public string? Volume
    {
        get
        {
            return m_Volume;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_Volume = value;
        }
    }
}
