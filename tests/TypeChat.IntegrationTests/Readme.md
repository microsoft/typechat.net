# Running Tests
The integration tests require an OpenAI key and will just skip if one is not available. 
To supply a key:
- Create appSettings.Development.json in this folder and override the ApiKey property defined in appSettings.json
- In Visual Studio, set File properties to Copy Always
- Build
