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

namespace May.Jarvis;
public class Input : IDisposable
{
    private const string PUSH_TO_TALK_INDICATOR = "*PUSH-TO-TALK*";
    
    public delegate string? ReturnStringEventHandler(object sender, EventArgs e);
    public event EventHandler? PushToTalkPressing;
    public event ReturnStringEventHandler? PushToTalkPressed;
    
    public string GetInput()
    {
        string? userInput = null;

        Console.Write(">> ");

        bool pushToTalk = false;
        while (userInput == null || userInput == string.Empty)
        {
            //State: No Input

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo firstKey = Console.ReadKey(true);

                if(!pushToTalk)
                {
                    if (firstKey.Key == ConsoleKey.Tab)
                    {
                        //State: Start Push-to-Talk
                        pushToTalk = true;

                        ShowPushToTalkIndicator();

                        //Signal PTT Key Pressing
                        OnPushToTalkPressing(new EventArgs());
                    }
                    else if ((firstKey.Key != ConsoleKey.Backspace) &&
                        (firstKey.Key != ConsoleKey.Enter))
                    {
                        //State: Text Input
                        userInput = ReadLine(firstKey);

                        if (string.IsNullOrEmpty(userInput))
                        {
                            var pos = Console.GetCursorPosition();
                            Console.SetCursorPosition(0, pos.Top);
                            Console.Write(">> ");
                        }
                    }
                }
            }
            else if (pushToTalk)
            {
                //State: Continue Push-to-Talk
                Thread.Sleep(1000);

                if (!Console.KeyAvailable)   //push-to-talk key up
                {
                    //State: End Push-to-Talk
                    pushToTalk = false;

                    HidePushToTalkIndicator();

                    //Signal PTT Key Pressed
                    string? text = OnPushToTalkPressed(new EventArgs());
                    if(!string.IsNullOrEmpty(text))
                        userInput = text.Replace("\r", string.Empty).Replace("\n", " ");

                    if(!string.IsNullOrEmpty(userInput))
                        Console.WriteLine(userInput);
                }
            }
        }

        //State: Have Input
        return userInput;
    }

    protected virtual void OnPushToTalkPressing(EventArgs e)
    {
        PushToTalkPressing?.Invoke(this, e);
    }

    protected virtual string? OnPushToTalkPressed(EventArgs e)
    {
        string? result = null;

        result = PushToTalkPressed?.Invoke(this, e);

        return result;
    }

    private void ShowPushToTalkIndicator()
    {
        var pos = Console.GetCursorPosition();
        Console.SetCursorPosition(Console.WindowWidth - PUSH_TO_TALK_INDICATOR.Length, 0);
        Console.Write(PUSH_TO_TALK_INDICATOR);
        Console.SetCursorPosition(pos.Left, pos.Top);
    }

    private void HidePushToTalkIndicator()
    {
        var pos = Console.GetCursorPosition();

        Console.SetCursorPosition(Console.WindowWidth - PUSH_TO_TALK_INDICATOR.Length, 0);

        for (int i = 0; i < PUSH_TO_TALK_INDICATOR.Length; i++)
            Console.Write(" ");

        Console.SetCursorPosition(pos.Left, pos.Top);
    }

    private string? ReadLine(ConsoleKeyInfo firstKey)
    {
        string? userInput = null;

        List<char> inputBuffer = new();

        if ((firstKey.KeyChar != 0) &&
            (firstKey.Key != ConsoleKey.Tab) &&
            (firstKey.Key != ConsoleKey.Backspace) &&
            (firstKey.Key != ConsoleKey.Enter))
        {
            inputBuffer.Add(firstKey.KeyChar);
            Console.Write(firstKey.KeyChar);
        }

        while(true)
        {
            var nextKey = Console.ReadKey(true);

            if ((nextKey.Key == ConsoleKey.Backspace) && (inputBuffer.Count > 0))    //handle backspace
            {
                inputBuffer.RemoveAt(inputBuffer.Count - 1);

                ConsoleBackspace();

                if (inputBuffer.Count == 0)
                    break;
            }
            else if (nextKey.Key == ConsoleKey.Enter)   //finished entering message
            {
                userInput = new string(inputBuffer.ToArray());

                Console.WriteLine();

                break;
            }
            else
            {
                if ((nextKey.KeyChar != 0) && (nextKey.Key != ConsoleKey.Tab))   //add visible characters to buffer
                {
                    inputBuffer.Add(nextKey.KeyChar);
                    Console.Write(nextKey.KeyChar);
                }
            }
        }

        return userInput;
    }

    private void ConsoleBackspace()
    {
        var pos = Console.GetCursorPosition();

        if ((pos.Left == 0) && (pos.Top == 0))
        {
            Console.Write(" ");
            Console.SetCursorPosition(0, 0);
        }
        else if (pos.Left == 0)
        {
            Console.SetCursorPosition(Console.WindowWidth - 1, pos.Top - 1);
            Console.Write(" ");
            Console.SetCursorPosition(Console.WindowWidth - 1, pos.Top - 1);
        }
        else
        {
            Console.SetCursorPosition(pos.Left - 1, pos.Top);
            Console.Write(" ");
            Console.SetCursorPosition(pos.Left - 1, pos.Top);
        }
    }

    public void Dispose()
    {
        PushToTalkPressing = null;
        PushToTalkPressed = null;

        GC.SuppressFinalize(this);
    }
}
