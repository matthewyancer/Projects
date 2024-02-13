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

using System.Text;
using Azure.AI.OpenAI;
using May.Jarvis.Settings;

namespace May.Jarvis;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Startup startup = new();
        Input input = new();
        SoundMixer soundMixer = new();
        Microphone microphone = new();
        SpeechToText speechToText = new();
        TextToSpeech textToSpeech = new(startup.TextToSpeechSettings);

        input.PushToTalkPressing += speechToText.PushToTalkPressing;
        input.PushToTalkPressed += speechToText.PushToTalkPressed;

        try
        {
            Console.Clear();
            Console.WriteLine("## Jarvis Chat ##");
            Console.WriteLine();

            StringBuilder prompt = new StringBuilder()
                .Append("Please act as an AI assistant named Jarvis and provide responses in the style of Jarvis from the Iron Man movies. ")
                .Append("Include a slight amount of wit in responses, and dry sarcasm 5% of the time. ")
                .Append($"Keep responses shorter and limit to {startup.OpenAiSettings.MaxTokens} tokens. ")
                .Append($"My name is {startup.PromptSettings.UserName}. You can refer to me by name or as {startup.PromptSettings.UserHonorific}. ")
                .Append("Do not prompt user about \"further assistance\". ");

            List<ChatRequestMessage> messageHistory = new()
            {
                new ChatRequestSystemMessage(prompt.ToString())
            };

            bool isMuted = false;

            while (true)
            {
                string userInput = input.GetInput();

                switch (userInput.Replace(".", string.Empty).ToUpper().Trim())
                {
                    case "MUTE":
                        isMuted = true;
                        textToSpeech.StopSpeaking();
                        continue;
                    case "UNMUTE":
                        isMuted = false;
                        continue;
                    case "STOP":
                        textToSpeech.StopSpeaking();
                        continue;
                }

                messageHistory.Add(new ChatRequestUserMessage(userInput));

                string assistantResponse = await GetOpenAiResponseAsync(messageHistory.ToArray(), startup.OpenAiSettings);

                messageHistory.Add(new ChatRequestAssistantMessage(assistantResponse));

                if (!isMuted)
                    textToSpeech.Speak(assistantResponse);

                Display(assistantResponse);
            }
        }
        finally
        {
            input.Dispose();
            textToSpeech.Dispose();
            microphone.Dispose();
        }
    }

    private static async Task<string> GetOpenAiResponseAsync(ChatRequestMessage[] messageHistory, OpenAiSettings openAiSettings)
    {
        //Setup OpenAI chat options
        ChatCompletionsOptions chatOptions = new()
        {
            DeploymentName = openAiSettings.Model,
            MaxTokens = Convert.ToInt32(openAiSettings.MaxTokens),
            Temperature = Convert.ToSingle(openAiSettings.Temperature),
            ChoiceCount = 1
        };

        foreach (ChatRequestMessage message in messageHistory)
        {
            ChatRequestMessage messageToAdd = message;

            if (messageToAdd.Role == ChatRole.System)
            {
                DateTime now = DateTime.Now;
                string dateTimeRule = $"Current time is {now.ToShortTimeString()}. Today's date is {now.Date.ToShortDateString()}. It is {now.DayOfWeek}. ";

                messageToAdd = new ChatRequestSystemMessage($"{((ChatRequestSystemMessage)messageToAdd).Content} {dateTimeRule}");
            }

            chatOptions.Messages.Add(messageToAdd);
        }

        //Send to OpenAI
        OpenAIClient openAiClient = new(openAiSettings.OpenAiKey);
        Azure.Response<ChatCompletions> openAiResponse = await openAiClient.GetChatCompletionsAsync(chatOptions);
        string assistantResponse = openAiResponse.Value.Choices[0].Message.Content;

        return assistantResponse;
    }

    private static void Display(string text)
    {
        Console.WriteLine(text);
        Console.WriteLine();
    }
}