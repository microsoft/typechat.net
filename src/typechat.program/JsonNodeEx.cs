// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class JsonNodeEx
{
    public static bool IsConvertibleToJsonNode(this Type type)
    {
        return (type.IsPrimitive || type.IsString());
    }

    public static JsonNode ToJsonNode(dynamic obj)
    {
        if (obj == null)
        {
            return obj;
        }

        if (obj is dynamic[] darray)
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < darray.Length; ++i)
            {
                dynamic value = darray[i];
                jsonArray.Add(ToJsonNode(value));
            }

            return jsonArray;
        }
        else if (obj is Array array)
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < array.Length; ++i)
            {
                dynamic value = array.GetValue(i);
                jsonArray.Add(ToJsonNode(value));
            }

            return jsonArray;
        }

        return obj;
    }
}
