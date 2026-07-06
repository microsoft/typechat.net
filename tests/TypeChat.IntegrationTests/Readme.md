# Running Tests
The integration tests **require an Azure or OpenAI key** and will just skip if one is not available. 

## API Keys for Tests

- Create appSettings.Development.json in this folder and override the ApiKey property defined in [appSettings.json](./appSettings.json)
- In Visual Studio, set File properties to Copy Always
- Build

### About API Keys
Read about API Keys in the [Project README](../../README.md)

## Multimodal (image) and Responses API tests

`TestMultimodalEndToEnd` covers image input and the OpenAI Responses API:

- `DescribeMicrosoftLogo_FromFile` sends the bundled `microsoft-logo.png` to the model and validates
  the strongly typed result. It requires a **vision capable** model (e.g. gpt-4o); the api-version is
  set to a vision capable value automatically.
- `TranslateSentiment_ResponsesApi` exercises the `/responses` route. The api-version is set to a
  Responses-capable value automatically; the configured resource/deployment must support the
  Responses API. 