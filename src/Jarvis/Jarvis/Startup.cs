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

namespace May.Jarvis;
public class Startup
{
    public Startup()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        
        IConfiguration config = builder.Build();

        PromptSettings = config.GetSection("PromptSettings").Get<PromptSettings>() ?? new PromptSettings();
        ApiSettings = config.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();
        TtsSettings = config.GetSection("TtsSettings").Get<TtsSettings>() ?? new TtsSettings();

        if(string.IsNullOrEmpty(ApiSettings.OpenAiKey))
            throw new Exception("Missing required setting \"ApiSettings.OpenAiKey\" from appsettings.json file.");
    }

    public PromptSettings PromptSettings
    { get; private set; }

    public ApiSettings ApiSettings
    { get; private set; }

    public TtsSettings TtsSettings
    { get; private set; }
}
