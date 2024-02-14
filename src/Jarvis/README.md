# Jarvis

A Jarvis style AI assistant with conversational chat using C# and OpenAI GPT model. Using eSpeak NG for text-to-speech and OpenAI Whisper for speech-to-text capabilities.

See my blog posts for more information:

1. [Conversational Chat](https://www.matthewyancer.com/2024/01/16/building-jarvis-in-csharp-part-1.html)
2. [Text-to-Speech](https://www.matthewyancer.com/2024/01/23/building-jarvis-in-csharp-part-2.html)  
3. [Prompting and Date/Time](https://www.matthewyancer.com/2024/01/30/building-jarvis-in-csharp-part-3.html)
4. [The Push-to-Talk Button](https://www.matthewyancer.com/2024/02/08/building-jarvis-in-csharp-part-4.html)
5. [Cross-Platform Audio](https://www.matthewyancer.com/2024/02/10/building-jarvis-in-csharp-part-5.html)
6. [Speech-to-Text](https://www.matthewyancer.com/2024/02/13/building-jarvis-in-csharp-part-6.html)

## Dependencies

Jarvis has several dependencies, some of which will require further setup:

- OpenAI: You will need to set your OpenAI key in the appsettings.json file and ensure you have non-expired credit in your OpenAI account. If you do not have an OpenAI key, see my [blog post](https://www.matthewyancer.com/2024/01/16/building-jarvis-in-csharp-part-1.html) for how to obtain one.
- eSpeak NG: You will need to install eSpeak NG for your operating system, see my [blog post](https://www.matthewyancer.com/2024/01/23/building-jarvis-in-csharp-part-2.html) for more info.
- arecord and amixer: On Linux systems, these cli applications are required. These are already installed on many distributions, such as Debian or Ubuntu. If already installed, no further setup required.

NuGet Packages - these are already referenced in the project file, so you should not need to do anything further to install.

- Azure.AI.OpenAI, version 1.0.0-beta.12
- NAudio, version 2.2.1
