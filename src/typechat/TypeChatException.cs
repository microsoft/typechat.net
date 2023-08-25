// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class TypeChatException : Exception
{
    public static class ErrorMessages
    {
        public const string TranslationFailed = "Translation Failed";
        public const string NoJsonReturned = "No JSON object returned";
        public const string IncompleteJson = "Json Parse Error. Json Object is incomplete";
    }

    public enum ErrorCode
    {
        Unknown,
        NoJsonObject,
        IncompleteJsonObject,
        JsonValidation
    }

    string _request;
    ErrorCode _errorCode;
    JsonResponse? _response;

    public TypeChatException(ErrorCode code, string request, JsonResponse response, string? message)
        : base(message)
    {
        _errorCode = code;
        _request = request;
        _response = response;
    }

    public ErrorCode Code => _errorCode;
    public string Request => _request;
    public JsonResponse Response => _response;

    public override string ToString()
    {
        return $"Error: {Code}\n{Message}\n\nRequest:\n{Request}\n\nResponse:\n{Response.ResponseText}";
    }

    internal static void ThrowNoJson(string request, JsonResponse response)
    {
        throw new TypeChatException(
            TypeChatException.ErrorCode.NoJsonObject,
            request,
            response,
            TypeChatException.ErrorMessages.NoJsonReturned
            );
    }

    internal static void ThrowJsonValidation(string request, JsonResponse response, string message)
    {
        throw new TypeChatException(
            TypeChatException.ErrorCode.JsonValidation,
            request,
            response,
            message
            );
    }

    internal static string IncompleteJson(JsonResponse response)
    {
        return $"{TypeChatException.ErrorMessages.IncompleteJson}:\n{response.Json}";
    }
}
