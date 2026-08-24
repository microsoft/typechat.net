// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;

namespace Multimodal;

/// <summary>
/// This example shows how to send images to a vision capable model and translate what the model
/// "sees" into a strongly typed object.
///
/// Each input line is either:
///   - a local image file path (e.g. C:\pics\cat.jpg), or
///   - an http(s) url to an image (e.g. https://.../cat.jpg)
///
/// The image is placed into a multimodal prompt alongside an instruction, and JsonTranslator
/// translates the response into an <see cref="ImageResponse"/>.
///
/// NOTE: This requires a vision capable model (e.g. gpt-4o). Set the model name in appSettings.
/// </summary>
public class MultimodalApp : ConsoleApp
{
    private readonly JsonTranslator<ImageResponse> _translator;

    public MultimodalApp()
    {
        OpenAIConfig config = Config.LoadOpenAI();

        // TIP: To use the OpenAI Responses API instead of Chat Completions, either point
        // config.Endpoint at a url whose path ends with /responses, or force it explicitly:
        //   config.UseResponsesApi = true;

        _translator = new JsonTranslator<ImageResponse>(new LanguageModel(config));
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        PromptImage image = IsUrl(input) ?
                            new PromptImage(input, ImageDetail.Auto) :
                            PromptImage.FromFile(input);

        // Build a multimodal request: an instruction followed by the image.
        Prompt request = new Prompt();
        request.Add(new MultimodalPromptSection()
            .AddText("Describe the following image.")
            .AddImage(image));

        ImageResponse response = await _translator.TranslateAsync(request, null, null, cancelToken);

        Console.WriteLine($"Caption: {response.Caption}");
        Console.WriteLine($"Contains text: {response.ContainsText}");
        if (response.Objects is not null && response.Objects.Length > 0)
        {
            Console.WriteLine("Objects: " + string.Join(", ", response.Objects));
        }
        if (response.Colors is not null && response.Colors.Length > 0)
        {
            Console.WriteLine("Colors: " + string.Join(", ", response.Colors));
        }
    }

    private static bool IsUrl(string input)
    {
        return input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               input.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            MultimodalApp app = new MultimodalApp();
            // Pass an input file as arg[0] to run in batch mode.
            await app.RunAsync("🖼️> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            Console.ReadLine();
            return -1;
        }

        return 0;
    }
}
