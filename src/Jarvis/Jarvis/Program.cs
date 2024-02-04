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
using System.Text;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;

namespace May.Jarvis;
internal class Program
{
    private static readonly Dictionary<int, Process> m_TtsProcesses = new();

    private static async Task Main(string[] args)
    {
        Startup startup = new();

        Console.WriteLine("## Jarvis Chat ##");
        Console.WriteLine();

        StringBuilder prompt = new StringBuilder()
            .Append("Please act as an AI assistant named Jarvis and provide responses in the style of Jarvis from the Iron Man movies. ")
            .Append("Include a slight amount of wit in responses, and dry sarcasm 5% of the time. ")
            .Append($"Keep responses shorter and limit to {startup.ApiSettings.MaxTokens} tokens. ")
            .Append($"My name is {startup.PromptSettings.UserName}. You can refer to me by name or as {startup.PromptSettings.UserHonorific}. ")
            .Append("Do not prompt user about \"further assistance\". ");

        List<ChatRequestMessage> messageHistory = new()
        {
            new ChatRequestSystemMessage(prompt.ToString())
        };

        bool isMuted = false;

        while (true)
        {
            string userInput = GetInput();

            switch (userInput.ToUpper())
            {
                case "MUTE":
                    isMuted = true;
                    KillTtsProcesses();
                    continue;
                case "UNMUTE":
                    isMuted = false;
                    continue;
                case "STOP":
                    KillTtsProcesses();
                    continue;
            }

            messageHistory.Add(new ChatRequestUserMessage(userInput));

            string assistantResponse = await GetOpenAiResponseAsync(messageHistory.ToArray(), startup.ApiSettings);

            messageHistory.Add(new ChatRequestAssistantMessage(assistantResponse));

            if(!isMuted)
                Speak(assistantResponse, startup.TtsSettings);

            Display(assistantResponse);
        }
    }

    private static string GetInput()
    {
        string? userInput = null;
        while (userInput == null || userInput == string.Empty)
        {
            Console.Write(">> ");
            userInput = Console.ReadLine();
        }

        return userInput;
    }

    private static void KillTtsProcesses()
    {
        foreach(Process ttsProcess in m_TtsProcesses.Values)
            ttsProcess.Kill();
    }

    private static async Task<string> GetOpenAiResponseAsync(ChatRequestMessage[] messageHistory, ApiSettings apiSettings)
    {
        //Setup OpenAI chat options
        ChatCompletionsOptions chatOptions = new()
        {
            DeploymentName = "gpt-3.5-turbo-1106",
            MaxTokens = Convert.ToInt32(apiSettings.MaxTokens),
            Temperature = 0.7f,
            ChoiceCount = 1
        };

        foreach (ChatRequestMessage message in messageHistory)
        {
            ChatRequestMessage messageToAdd = message;

            if(messageToAdd.Role == ChatRole.System)
            {
                DateTime now = DateTime.Now;
                string dateTimeRule = $"Current time is {now.ToShortTimeString()}. Today's date is {now.Date.ToShortDateString()}. It is {now.DayOfWeek}. ";

                messageToAdd = new ChatRequestSystemMessage($"{((ChatRequestSystemMessage)messageToAdd).Content} {dateTimeRule}");
            }

            chatOptions.Messages.Add(messageToAdd);
        }

        //Send to OpenAI
        OpenAIClient openAiClient = new(apiSettings.OpenAiKey);
        Azure.Response<ChatCompletions> openAiResponse = await openAiClient.GetChatCompletionsAsync(chatOptions);
        string assistantResponse = openAiResponse.Value.Choices[0].Message.Content;

        return assistantResponse;
    }

    private static void Speak(string text, TtsSettings ttsSettings)
    {
        KillTtsProcesses();

        string phoneticText = ApplyPhoneticRules(text);

        Process textToSpeech = new();
        textToSpeech.StartInfo.FileName = "espeak-ng";
        textToSpeech.StartInfo.Arguments = $"-v en-gb-x-rp -s {ttsSettings.Speed} -a {ttsSettings.Volume} \"{phoneticText.Replace("\"", string.Empty)}\"";
        textToSpeech.Exited += new EventHandler(TtsProcess_Exited);
        textToSpeech.Start();
        
        m_TtsProcesses.Add(textToSpeech.Id, textToSpeech);
    }

    private static string ApplyPhoneticRules(string text)
    {
        string phoneticText = text;

        phoneticText = Regex.Replace(phoneticText, @"[Cc]#", "C Sharp");

        phoneticText = Regex.Replace(phoneticText, @"[Qq]uinoa", "Keen Waa");

        return phoneticText;
    }

    private static void TtsProcess_Exited(object? sender, System.EventArgs e)
    {
        if(sender != null)
            m_TtsProcesses.Remove(((Process)sender).Id);
    }

    private static void Display(string text)
    {
        Console.WriteLine(text);
        Console.WriteLine();
    }
}