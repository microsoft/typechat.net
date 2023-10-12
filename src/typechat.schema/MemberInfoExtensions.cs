// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public static class MemberInfoExtensions
{
    public static JsonVocabAttribute? JsonVocabAttribute(this MemberInfo member)
    {
        return member.GetCustomAttribute(typeof(JsonVocabAttribute)) as JsonVocabAttribute;
    }

    internal static IEnumerable<CommentAttribute> CommentAttributes(this MemberInfo member)
    {
        object[] attributes = member.GetCustomAttributes(typeof(CommentAttribute), false);
        foreach (var attribute in attributes)
        {
            if (attribute is CommentAttribute comment)
            {
                yield return comment;
            }
        }
    }

    internal static IEnumerable<string> Comments(this MemberInfo member)
    {
        return from comment in member.CommentAttributes()
               where comment.HasText
               select comment.Text;
    }

    internal static bool IsRequired(this MemberInfo property)
    {
#if NET7_0_OR_GREATER
        var json_attrib = property.GetCustomAttribute(typeof(JsonRequiredAttribute));
        if (json_attrib != null)
        {
            return true;
        }
#endif
        var attrib = property.GetCustomAttribute(typeof(RequiredAttribute));
        return attrib != null;
    }

    internal static bool IsIgnore(this MemberInfo member)
    {
        JsonIgnoreAttribute? attr = (JsonIgnoreAttribute)member.GetCustomAttribute(typeof(JsonIgnoreAttribute), true);
        return attr != null;
    }

    internal static string PropertyName(this MemberInfo member)
    {
        JsonPropertyNameAttribute? attr = (JsonPropertyNameAttribute)member.GetCustomAttribute(typeof(JsonPropertyNameAttribute), true);
        return attr != null ?
                attr.Name :
                member.Name;
    }
}
