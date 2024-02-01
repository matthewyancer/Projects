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

namespace May.Jarvis;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Startup startup = new();

        Console.WriteLine("## Jarvis Chat ##");
        Console.WriteLine();

        string prompt = "Please act as an AI assistant named Jarvis and provide responses in the style of Jarvis from the Iron Man movies.";

        //Initialize message history with prompt
        List<ChatRequestMessage> messageHistory = new()
        {
            new ChatRequestSystemMessage(prompt)
        };

        while (true)
        {
            //Get user input
            string? userInput = null;
            while (userInput == null || userInput == string.Empty)
            {
                Console.Write(">> ");
                userInput = Console.ReadLine();
            }

            //Add user input to message history
            messageHistory.Add(new ChatRequestUserMessage(userInput));

            //Setup OpenAI chat options
            ChatCompletionsOptions chatOptions = new()
            {
                DeploymentName = "gpt-3.5-turbo-1106",
                MaxTokens = 300,
                Temperature = 0.7f,
                ChoiceCount = 1
            };

            foreach (var message in messageHistory)
                chatOptions.Messages.Add(message);

            //Send to OpenAI
            OpenAIClient client = new(startup.ApiSettings.OpenAiKey);
            Azure.Response<ChatCompletions> openAiResponse = await client.GetChatCompletionsAsync(chatOptions);
            string assistantResponse = openAiResponse.Value.Choices[0].Message.Content;

            //Add assistant response to message history
            messageHistory.Add(new ChatRequestAssistantMessage(assistantResponse));

            //Output assistant response
            Console.WriteLine(assistantResponse);
            Console.WriteLine();
        }
    }
}