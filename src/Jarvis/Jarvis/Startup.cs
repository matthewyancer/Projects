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

using Microsoft.Extensions.Configuration;
using May.Jarvis.Settings;

namespace May.Jarvis;
public class Startup
{
    public Startup()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        
        IConfiguration config = builder.Build();

        PromptSettings = config.GetSection("Prompt").Get<PromptSettings>() ?? new PromptSettings();
        OpenAiSettings = config.GetSection("OpenAi").Get<OpenAiSettings>() ?? new OpenAiSettings();
        TextToSpeechSettings = config.GetSection("TextToSpeech").Get<TextToSpeechSettings>() ?? new TextToSpeechSettings();
        SpeechToTextSettings = config.GetSection("SpeechToText").Get<SpeechToTextSettings>() ?? new SpeechToTextSettings();

        if(string.IsNullOrEmpty(OpenAiSettings.OpenAiKey))
            throw new Exception("Missing required setting \"OpenAi.OpenAiKey\" from appsettings.json file.");
    }

    public PromptSettings PromptSettings
    { get; private set; }

    public OpenAiSettings OpenAiSettings
    { get; private set; }

    public TextToSpeechSettings TextToSpeechSettings
    { get; private set; }

    public SpeechToTextSettings SpeechToTextSettings
    { get; private set; }
}
