# Multimodal

Demonstrates sending **images** to a vision capable language model and translating what the model
sees into a **strongly typed** object using `JsonTranslator`.

## What it shows

- Building a multimodal prompt with [`MultimodalPromptSection`](../../src/typechat/MultimodalPromptSection.cs),
  mixing text and images.
- Referencing images either by url or by local file (`PromptImage.FromFile`, which base64 encodes the image
  into a data uri).
- Translating the multimodal request into a typed [`ImageResponse`](./ImageSchema.cs) — TypeChat validates and
  repairs the model's JSON exactly as it does for text-only requests.

```cs
Prompt request = new Prompt();
request.Add(new MultimodalPromptSection()
    .AddText("Describe the following image.")
    .AddImage(PromptImage.FromFile("cat.jpg")));

ImageResponse response = await translator.TranslateAsync(request, null);
```

## Requirements

- A **vision capable** model such as `gpt-4o`. Set the model name in `appSettings.Development.json`.

## Running

The example ships with the official Microsoft logo (`microsoft-logo.png`, sourced from
[../../assets](../../assets)) which is copied next to the built app, so it runs out of the box.

- Interactive: run the project and paste an image file path or an http(s) url at the prompt.
- Batch: pass an input file whose lines are image paths/urls, e.g. `dotnet run -- input.txt`.
  The bundled [input.txt](./input.txt) points at `microsoft-logo.png`.

## OpenAI Responses API

The built-in `LanguageModel` also supports the OpenAI **Responses API**. It is selected automatically when
the configured endpoint path ends with `/responses`, or you can force it:

```cs
OpenAIConfig config = Config.LoadOpenAI();
config.UseResponsesApi = true; // or false to force Chat Completions
```
