// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonResponse
{
    public JsonResponse(string responseText)
    {
        ResponseText = responseText;
        HasCompleteJson = false;
    }

    /// <summary>
    /// Full response text
    /// </summary>
    public string ResponseText { get; private set; }
    /// <summary>
    /// Json included in response: any text contained within braces: {}
    /// </summary>
    public string? Json { get; private set; }
    /// <summary>
    /// Anything that came before the json, if any
    /// </summary>
    public string? Prologue { get; private set; }
    /// <summary>
    /// Anything that came after the json, if any
    /// </summary>
    public string? Epilogue { get; private set; }
    /// <summary>
    /// Was the json enclosed in braces? {} 
    /// </summary>
    public bool HasCompleteJson { get; private set; }

    public bool HasJson => !string.IsNullOrEmpty(Json);
    public bool HasPrologue => !string.IsNullOrEmpty(Prologue);
    public bool HasEpilogue => !string.IsNullOrEmpty(Epilogue);

    // Message combines Prologue and Epilogue
    public string? Message()
    {
        if (HasPrologue || HasEpilogue)
        {
            return string.Join('\n', Prologue, Epilogue);
        }
        return null;
    }

    public static JsonResponse Parse(string responseText)
    {
        JsonResponse response = new JsonResponse(responseText);
        if (string.IsNullOrEmpty(responseText))
        {
            return response;
        }
        int lastIndex = responseText.Length - 1;
        int iJsonStartAt = responseText.IndexOf('{');
        int iJsonEndAt = responseText.LastIndexOf('}');

        if (iJsonStartAt < 0)
        {
            // No Json. Treat everything as Prologue
            response.Prologue = responseText;
            return response;
        }

        response.HasCompleteJson = false; // { indicates that Json was attempted
        if (iJsonStartAt > 0)
        {
            // Prologue precedes Json
            response.Prologue = responseText.Substring(0, iJsonStartAt);
        }
        if (iJsonEndAt < 0)
        {
            // Incomplete json. Treat remaining text as epilogue
            response.Json = responseText.Substring(iJsonStartAt, lastIndex - iJsonStartAt);
            return response;
        }

        response.Json = responseText.Substring(iJsonStartAt, (iJsonEndAt - iJsonStartAt) + 1);
        response.HasCompleteJson = true; // We have at least {}
        ++iJsonEndAt;
        if (iJsonEndAt < lastIndex)
        {
            // Epilogue follows json
            response.Epilogue = responseText.Substring(iJsonEndAt, (lastIndex - iJsonEndAt) + 1);
        }
        return response;
    }
}
