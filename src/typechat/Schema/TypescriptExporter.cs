// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Export NET Types to schema for JSON objects. The schema is expressed in Typescript and passed to
/// JsonTranslators
/// 
/// Typescript is a concise and expressive schema definition language, making it easy to to express the
/// shape and structure of Json. We want language models to translate user requests into JSON
/// of a particular shape that is defined by a schema. We then want to deserialize this JSON into strong
/// types and validate the types for correctness.
///
/// The exporter is written entirely in C# and does not depend on any Typescript components.
/// It is also intended to support scenarios where every request has a different schema that is contextual.
/// 
/// The exporter has limitations because Type systems are complex. It aims to support common scenarios.
///  - Will export classes, enums, value types, properties, fields etc.. including inheritance, mapped to their Typescript base types
///  - Obeys nullable and '?' annotations in C#
///  - Incorporates Serialization attributes such as JsonPropertyName and JsonIgnore
///  - Vocabularies: this is a new concept that greatly simplifies Json programming
///  - Json Polymorphism is supported. But polymorphism Json serialiezer attributes are currently not looked at
///  By default, the discriminator used to support polymorphism must be the same as the typename
/// 
/// You can always author the Typescript schema by hand and give it to the JsonTranslator using a suitable
/// constructor overload
/// </summary>
public class TypescriptExporter : TypeExporter<Type>
{
    /// <summary>
    /// Export the given .NET type as schema... expressed in Typescript
    /// </summary>
    /// <param name="type"></param>
    /// <param name="knownVocabs"></param>
    /// <returns></returns>
    public static TypescriptSchema GenerateSchema(Type type, IVocabCollection? knownVocabs = null)
    {
        using StringWriter writer = new StringWriter();
        TypescriptExporter exporter = new TypescriptExporter(writer);
        if (knownVocabs != null)
        {
            exporter.Vocabs = knownVocabs;
        }
        exporter.Export(type);
        string schema = writer.ToString();
        return new TypescriptSchema(type, schema, exporter.UsedVocabs);
    }

    /// <summary>
    /// Export the given API interface as Typescript
    /// </summary>
    /// <param name="type">Must be an interface</param>
    /// <returns>A typescript schema</returns>
    public static TypescriptSchema GenerateAPI(Type type)
    {
        using StringWriter writer = new StringWriter();
        TypescriptExporter exporter = new TypescriptExporter(writer);
        exporter.ExportAPI(type);
        string schema = writer.ToString();
        return new TypescriptSchema(type, schema, exporter.UsedVocabs);
    }

    const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    TypescriptWriter _writer;
    HashSet<Type> _nonExportTypes;
    TypescriptVocabExporter? _vocabExporter;
#if NET6_0_OR_GREATER
    NullabilityInfoContext _nullableInfo;
#endif
    VocabCollection _usedVocabs;

    public TypescriptExporter(TextWriter writer)
        : this(new TypescriptWriter(writer))
    {
    }

    public TypescriptExporter(TypescriptWriter writer)
        : base()
    {
        ArgumentVerify.ThrowIfNull(writer, nameof(writer));
        _writer = writer;
        _nonExportTypes = new HashSet<Type>()
        {
            typeof(object),
            typeof(string),
            typeof(Array),
            typeof(Nullable<>)
        };
#if NET6_0_OR_GREATER
        _nullableInfo = new NullabilityInfoContext();
#endif
    }

    /// <summary>
    /// Typescript writer the exporter is using
    /// </summary>
    public TypescriptWriter Writer => _writer;
    //
    // Use this to *customize* how a .NET Type Name is mapped to a Typescript type name
    // Return null if you can't map and defaults are used.
    //
    public Func<Type, string?> TypeNameMapper { get; set; }
    /// <summary>
    /// Use this to customize how type discriminators are produced. 
    /// </summary>
    public Func<Type, string> DiscriminatorGenerator { get; set; }
    /// <summary>
    /// Should export subclasses of a given type: automatically exporting type hierarchies?
    /// Default is true
    /// </summary>
    public bool IncludeSubclasses { get; set; } = true;
    /// <summary>
    /// Include comments? You can provide includable comments using the [Comment] attribute
    /// Default is true
    /// </summary>
    public bool IncludeComments { get; set; } = true;
    /// <summary>
    /// Export enums as string literals? Typescript allows that and in some situations, this works
    /// better with the some models
    /// Default is false
    /// </summary>
    public bool EnumsAsLiterals { get; set; } = false;
    /// <summary>
    /// Discriminators are needed for Json Polymorphism: so the Json Deserializer can distinguish which 
    /// object should be deserialized to instances of which sub-class
    /// By default discriminators are emitted and in a format recognized by System.Text.Json serilization
    ///   "$type": "{type.Name}"
    /// You can customize this using 
    /// </summary>
    public bool IncludeDiscriminator { get; set; } = true;

    public IVocabCollection Vocabs
    {
        get
        {
            return _vocabExporter?.Vocabs;
        }
        set
        {
            if (value != null)
            {
                _vocabExporter = new TypescriptVocabExporter(_writer, value);
            }
            else
            {
                _vocabExporter = null;
            }
        }
    }

    public VocabCollection? UsedVocabs => _usedVocabs;

    public override void Clear()
    {
        base.Clear();
        _writer.Clear();
        _vocabExporter?.Clear();

#if NET6_0_OR_GREATER
        _nullableInfo = null;
#endif
    }

    public override void ExportPending()
    {
        base.ExportPending();
        _vocabExporter?.ExportPending();
    }

    public override void ExportType(Type type)
    {
        if (type.IsArray)
        {
            AddPending(type.GetElementType());
        }
        else
        {
            if (type.IsEnum)
            {
                ExportEnum(type);
            }
            else
            {
                ExportClass(type);
            }
        }
    }

    public TypescriptExporter ExportClass(Type type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        if (!type.IsClass)
        {
            ArgumentVerify.Throw($"{type.Name} must be a class");
        }

        if (IsExported(type))
        {
            return this;
        }

        Type? baseClass = type.BaseClass();
        string baseClassName = null;
        if (baseClass != null)
        {
            ExportClass(baseClass);
            baseClassName = baseClass.Name;
        }
        string typeName = type.Name;

        ExportComments(type);
        _writer.BeginInterface(typeName, baseClassName);
        {
            _writer.PushIndent();

            if (this.IncludeDiscriminator && baseClass != null)
            {
                ExportDiscriminator(type);
            }
            ExportMembers(type);

            _writer.PopIndent();
        }
        _writer.EndInterface();

        AddExported(type);

        if (IncludeSubclasses)
        {
            AddPending(type.Subclasses());
            AddPending(type.Implementors());
        }

        return this;
    }

    public TypescriptExporter ExportEnum(Type type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        if (!type.IsEnum)
        {
            ArgumentVerify.Throw($"{type.Name} must be Enum");
        }

        if (IsExported(type))
        {
            return this;
        }

        if (EnumsAsLiterals)
        {
            ExportEnumAsLiterals(type);
        }
        else
        {
            ExportEnumAsEnum(type);
        }
        AddExported(type);

        return this;
    }

    /// <summary>
    /// Export an API as an interface, but will not automatically export interfaces that are inherited from
    /// </summary>
    public TypescriptExporter ExportAPI(Type type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        if (!type.IsInterface)
        {
            ArgumentVerify.Throw($"{type.Name} must be an interface");
        }

        if (IsExported(type))
        {
            return this;
        }

        string typeName = type.Name;
        ExportComments(type);
        _writer.BeginInterface(typeName);
        {
            _writer.PushIndent();
            ExportMethods(type);
            _writer.PopIndent();
        }
        _writer.EndInterface();

        AddExported(type);

        return this;
    }

    TypescriptExporter ExportEnumAsEnum(Type type)
    {
        string typeName = type.Name;
        ExportComments(type);
        _writer.Enum(typeName).Space().StartBlock();
        {
            _writer.PushIndent();
            ExportEnumValues(type);
            _writer.PopIndent();
        }
        _writer.EndBlock();
        return this;
    }

    TypescriptExporter ExportEnumAsLiterals(Type type)
    {
        string typeName = type.Name;
        ExportComments(type);
        _writer.Type(typeName).Space().Assign().EOL();
        {
            ExportEnumLiterals(type);
        }
        _writer.EOL();
        return this;
    }

    TypescriptExporter ExportEnumLiterals(Type type)
    {
        if (!IncludeComments)
        {
            _writer.Literals(Enum.GetNames(type)).Semicolon();
            return this;
        }
        string[] names = Enum.GetNames(type);
        var fields = type.GetFields();
        for (int i = 0; i < names.Length; ++i)
        {
            _writer.SOL();
            {
                ExportComments(fields[i + 1]);
                _writer.Literal(names[i]);
                if (i < names.Length - 1)
                {
                    _writer.Space().Or();
                }
                else
                {
                    _writer.Semicolon();
                }
            }
            _writer.EOL();
        }
        return this;
    }

    TypescriptExporter ExportEnumValues(Type type)
    {
        string[] names = Enum.GetNames(type);
        var fields = type.GetFields();
        for (int i = 0; i < names.Length; ++i)
        {
            ExportComments(fields[i + 1]);
            _writer.SOL();
            {
                _writer.Name(names[i]);
                if (i < names.Length - 1)
                {
                    _writer.Comma();
                }
            }
            _writer.EOL();
        }
        return this;
    }

    TypescriptExporter ExportMembers(Type type)
    {
        return ExportProperties(type).
               ExportFields(type);
    }

    TypescriptExporter ExportProperties(Type type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));

        var properties = type.GetProperties(MemberFlags);
        return ExportProperties(properties);
    }

    TypescriptExporter ExportProperties(IEnumerable<PropertyInfo> properties)
    {
        ArgumentVerify.ThrowIfNull(properties, nameof(properties));

        foreach (var property in properties)
        {
            ExportProperty(property);
        }
        return this;
    }

    TypescriptExporter ExportFields(Type type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));

        var fields = type.GetFields(MemberFlags);
        return ExportFields(fields);
    }

    TypescriptExporter ExportFields(IEnumerable<FieldInfo> fields)
    {
        foreach (var field in fields)
        {
            ExportField(field);
        }
        return this;
    }

    TypescriptExporter ExportProperty(PropertyInfo property)
    {
        if (!property.IsAbstract() &&
            !property.IsIgnore())
        {
            ExportComments(property);
            ExportVariable(property, property.PropertyType);
        }
        return this;
    }

    TypescriptExporter ExportField(FieldInfo field)
    {
        if (!field.IsIgnore())
        {
            ExportComments(field);
            ExportVariable(field, field.FieldType);
        }
        return this;
    }

    TypescriptExporter ExportMethods(Type type)
    {
        MethodInfo[] methods = type.GetMethods(MemberFlags);
        foreach (var method in methods)
        {
            ExportMethod(method);
        }
        return this;
    }

    TypescriptExporter ExportMethod(MethodInfo methodInfo)
    {
        ExportComments(methodInfo);
        _writer.BeginMethodDeclare(methodInfo.Name);
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo parameter = parameters[i];
                ExportParameter(parameter, i, parameters.Length);
            }
        }
        _writer.EndMethodDeclare(DataType(methodInfo.ReturnType), methodInfo.ReturnType.IsNullableValueType());
        return this;
    }

    TypescriptExporter ExportComments(MemberInfo member)
    {
        if (IncludeComments)
        {
            foreach (var comment in member.Comments())
            {
                _writer.SOL().Comment(comment);
            }
            if (member.IsRequired())
            {
                _writer.SOL().Comment("Required");
            }
        }
        return this;
    }

    TypescriptExporter ExportVariable(MemberInfo member, Type type)
    {
        Type actualType;
        bool isNullable;
        if (type.IsValueType)
        {
            Type? nullableType = type.GetNullableValueType();
            isNullable = nullableType != null;
            actualType = isNullable ? nullableType : type;
        }
        else
        {
            actualType = type;
            isNullable = IsNullableRef(member);
        }

        if (actualType.IsString() &&
            ExportJsonVocab(member, actualType, isNullable))
        {
            return this;
        }

        _writer.SOL();
        {
            _writer.Variable(
                member.PropertyName(),
                DataType(actualType),
                type.IsArray,
                isNullable
            );
        }
        _writer.EOL();
        return this;
    }

    TypescriptExporter ExportParameter(ParameterInfo parameter, int i, int count)
    {
        Type type = parameter.ParameterType;
        Type actualType;
        bool isNullable;
        if (type.IsValueType)
        {
            Type? nullableType = type.GetNullableValueType();
            isNullable = nullableType != null;
            actualType = isNullable ? nullableType : type;
        }
        else
        {
            actualType = type;
            isNullable = IsNullableRef(parameter);
        }
        _writer.Parameter(
            parameter.Name,
            DataType(type),
            i,
            count,
            type.IsArray,
            isNullable
            );
        return this;
    }

    bool ExportJsonVocab(MemberInfo member, Type type, bool isNullable)
    {
        JsonVocabAttribute? vocabAttr = member.JsonVocabAttribute();
        if (vocabAttr == null)
        {
            // No vocab
            return false;
        }
        IVocab? vocab = null;
        NamedVocab? vocabType = null;
        if (vocabAttr.HasVocab)
        {
            // VocabAttribute has hardcoded vocabulary
            vocab = vocabAttr.Vocab;
            if (vocabAttr.HasName)
            {
                vocabType = new NamedVocab(vocabAttr.Name, vocab);
            }
        }
        else
        {
            // Resolve vocabulary from an external source
            vocabType = _vocabExporter?.Vocabs.Get(vocabAttr.Name);
            vocab = vocabType?.Vocab;
        }

        if (vocabAttr.Inline)
        {
            if (vocab == null)
            {
                // No vocab
                throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"Vocabulary {vocabAttr.Name} not found");
            }
            ExportVocabInline(member, isNullable, vocab);
        }
        else
        {
            if (vocabType == null)
            {
                // No vocab
                SchemaException.ThrowVocabNotFound(vocabAttr.Name);
            }
            ExportVocabType(member, type, vocabType, isNullable);
        }

        if (!vocabAttr.HasPropertyName)
        {
            vocabAttr.PropertyName = member.PropertyName();
        }

        if (vocabType != null && vocabAttr.Enforce)
        {
            _usedVocabs ??= new VocabCollection();
            _usedVocabs.Add(vocabType);
        }

        return true;
    }

    void ExportVocabInline(MemberInfo member, bool isNullable, IVocab vocab)
    {
        _writer.SOL();
        {
            _writer.Variable(
                member.PropertyName(),
                isNullable,
                vocab.Strings()
            );
        }
        _writer.EOL();
    }

    void ExportVocabType(MemberInfo member, Type type, NamedVocab vocabType, bool isNullable)
    {
        _writer.SOL();
        {
            _writer.Variable(
                member.PropertyName(),
                vocabType.Name,
                type.IsArray,
                isNullable
            );
        }
        _writer.EOL();

        _vocabExporter.AddPending(vocabType);
    }

    protected virtual TypescriptExporter ExportDiscriminator(Type type)
    {
        if (!type.IsAbstract)
        {
            string discriminator = (DiscriminatorGenerator != null) ?
                                    DiscriminatorGenerator(type) :
                                    $"'{type.Name}'";

            _writer.SOL();
            _writer.Variable("$type", discriminator);
            _writer.EOL();
        }
        return this;
    }

    string DataType(Type type)
    {
        if (type.IsArray)
        {
            return DataType(type.GetElementType());
        }

        string? typeName = null;
        if (TypeNameMapper != null)
        {
            typeName = TypeNameMapper(type);
        }
        if (typeName == null)
        {
            typeName = Typescript.Types.ToPrimitive(type);
        }
        if (string.IsNullOrEmpty(typeName))
        {
            typeName = type.Name;
            AddPending(type);
        }
        return typeName;
    }

    bool IsNullableRef(MemberInfo member)
    {
        if (member is PropertyInfo p)
        {
            return IsNullableRef(p);
        }
        else if (member is FieldInfo f)
        {
            return IsNullableRef(f);
        }
        return false;
    }

    bool IsNullableRef(PropertyInfo prop)
    {
#if NET6_0_OR_GREATER
        var info = _nullableInfo.Create(prop);
        return info.WriteState == NullabilityState.Nullable;
#else
        // In runtimes older than net6.0, we only support nullable value types (nullable reference types unsupported).
        return prop.PropertyType.IsNullableValueType();
#endif
    }

    bool IsNullableRef(FieldInfo field)
    {
#if NET6_0_OR_GREATER
        var info = _nullableInfo.Create(field);
        return info.WriteState == NullabilityState.Nullable;
#else
        // In runtimes older than net6.0, we only support nullable value types (nullable reference types unsupported).
        return field.FieldType.IsNullableValueType();
#endif
    }

    bool IsNullableRef(ParameterInfo pinfo)
    {
#if NET6_0_OR_GREATER
        var info = _nullableInfo.Create(pinfo);
        return info.WriteState == NullabilityState.Nullable;
#else
        // In runtimes older than net6.0, we only support nullable value types (nullable reference types unsupported).
        return pinfo.ParameterType.IsNullableValueType();
#endif
    }

    protected override bool ShouldExport(Type type)
    {
        if (type.IsPrimitive ||
            type.IsNullableValueType())
        {
            return false;
        }
        if (type.IsArray)
        {
            type = type.GetElementType();
        }
        return !type.IsPrimitive && !_nonExportTypes.Contains(type);
    }
}
