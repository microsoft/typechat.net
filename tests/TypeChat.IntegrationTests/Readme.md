# Running Tests
The integration tests require an Open AI key and will just skip if one is not available. 
To supply a key:
- Create appSettings.Development.json in this folder and override the ApiKey property defined in appSettings.json.example
- In Visual Studio, set File properties to Copy Always
- Build
