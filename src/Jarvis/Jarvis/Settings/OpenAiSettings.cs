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
public class OpenAiSettings
{
    private string m_Model = "gpt-3.5-turbo-0125";  //see https://platform.openai.com/docs/models/overview
    private string m_MaxTokens = "300";
    private string m_Temperature = "0.7";

    public string? OpenAiKey    //no default
    { get; set; }

    public string? Model
    {
        get
        {
            return m_Model;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_Model = value;
        }
    }

    public string? MaxTokens
    {
        get
        {
            return m_MaxTokens;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_MaxTokens = value;
        }
    }

    public string? Temperature
    {
        get
        {
            return m_Temperature;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
                m_Temperature = value;
        }
    }
}
