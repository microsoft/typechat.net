// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class Extensions
{
    public static object Deserialize(this JsonNode obj, Type targetType, JsonSerializerOptions? options = null)
    {
        // Until there is a more efficient way to do this..
        string json = obj.ToJsonString();
        return JsonSerializer.Deserialize(json, targetType, options);
    }

    internal static bool IsAsync(this ParameterInfo returnType)
    {
        return (returnType.ParameterType.IsAssignableTo(typeof(Task)));
    }
}
