// Copyright (c) Microsoft. All rights reserved.

//------------------------------------------------------
//
// A simple app that translates user requests into emojis
// using all 3 options in TypeChat.
// This application is used to test Package installs
//
//------------------------------------------------------

using Microsoft.TypeChat;
using Microsoft.TypeChat.Dialog;

namespace Emoji;

public class EmojiResponse
{
    public string EmojiTranslation { get; set; } = string.Empty;
}

public interface IEmojiService
{
    Task<string> ToEmoji(string userText);
}

internal class EmojiProgram : IEmojiService
{
    LanguageModel _model;
    JsonTranslator<EmojiResponse> _translator;
    public EmojiProgram()
    {
        _model = new LanguageModel(OpenAIConfig.FromEnvironment());
        _translator = new JsonTranslator<EmojiResponse>(_model);
    }

    public async Task<string> ToEmoji(string line)
    {
        EmojiResponse response = await _translator.TranslateAsync(line);
        return response.EmojiTranslation;
    }

    public async Task TestTranslate()
    {
        string? line;
        Console.Write(">");

        while ((line = Console.ReadLine()) != null)
        {
            string result = await ToEmoji(line);
            Console.WriteLine(result);
            Console.Write(">");
        }
    }

    public async Task TestProgram()
    {
        string? line;
        Console.Write(">");
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var api = new Api<IEmojiService>(this);
        ProgramTranslator programGen = new ProgramTranslator<IEmojiService>(_model, api);
        while ((line = Console.ReadLine()) != null)
        {
            using Program program = await programGen.TranslateAsync(line);
            string result = await program.RunAsync(api);
            Console.WriteLine(result);
            Console.Write(">");
        }
    }

    public async Task TestAgent()
    {
        Agent<EmojiResponse> agent = new AgentWithHistory<EmojiResponse>(_model);
        agent.Instructions.Append("Respond to me in emojis");
        string? line;
        Console.Write(">");
        while ((line = Console.ReadLine()) != null)
        {
            var response = await agent.GetResponseAsync(line);
            Console.WriteLine(response.EmojiTranslation);
            Console.Write(">");
        }
    }

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        EmojiProgram p = new EmojiProgram();
        //await p.TestTranslate();
        //await p.TestProgram();
        await p.TestAgent();
        return 0;
    }
}
