# Running Tests
The integration tests **require an Azure or OpenAI key** and will just skip if one is not available. 

## API Keys for Tests

- Create appSettings.Development.json in this folder and override the ApiKey property defined in [appSettings.json](./appSettings.json)
- In Visual Studio, set File properties to Copy Always
- Build

### About API Keys
Read about API Keys in the [Project README](../../README.md) 