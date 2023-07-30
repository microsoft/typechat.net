// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptExporter : TypeExporter<Type>
{
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

    const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    TypescriptWriter _writer;
    HashSet<Type> _nonExportTypes;
    TypescriptVocabExporter? _vocabExporter;
    NullabilityInfoContext _nullableInfo;
    VocabCollection _usedVocabs;

    public TypescriptExporter(TextWriter writer)
        : this(new TypescriptWriter(writer))
    {
    }

    public TypescriptExporter(TypescriptWriter writer)
        : base()
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        _writer = writer;
        _nonExportTypes = new HashSet<Type>()
        {
            typeof(string),
            typeof(Array),
            typeof(Nullable<>)
        };
        _nullableInfo = new NullabilityInfoContext();
    }

    public TypescriptWriter Writer => _writer;
    public bool IncludeSubclasses { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
    public bool EnumsAsLiterals { get; set; } = false;
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
        _nullableInfo = null;
    }

    public override void ExportQueued()
    {
        base.ExportQueued();
        _vocabExporter?.ExportQueued();
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
        if (!type.IsClass)
        {
            throw new ArgumentException($"{type.Name} must be a class");
        }
        ArgumentNullException.ThrowIfNull(type, nameof(type));

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
        _writer.EndInterface(typeName);

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
        Debug.Assert(type.IsEnum);
        ArgumentNullException.ThrowIfNull(type, nameof(type));

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
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var properties = type.GetProperties(MemberFlags);
        return ExportProperties(properties);
    }

    TypescriptExporter ExportProperties(IEnumerable<PropertyInfo> properties)
    {
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        foreach (var property in properties)
        {
            ExportProperty(property);
        }
        return this;
    }

    TypescriptExporter ExportFields(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

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

    TypescriptExporter ExportComments(MemberInfo member)
    {
        if (IncludeComments)
        {
            foreach (var comment in member.Comments())
            {
                _writer.SOL().Comment(comment);
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
            isNullable = (nullableType != null);
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
    bool ExportJsonVocab(MemberInfo member, Type type, bool isNullable)
    {
        JsonVocabAttribute? vocabAttr = member.JsonVocabAttribute();
        if (vocabAttr == null)
        {
            // No vocab
            return false;
        }
        IVocab? vocab = null;
        VocabType? vocabType = null;
        if (vocabAttr.HasVocab)
        {
            // VocabAttribute has hardcoded vocabulary
            vocab = vocabAttr.Vocab;
            if (vocabAttr.HasName)
            {
                vocabType = new VocabType(vocabAttr.Name, vocab);
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
                throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, vocabAttr.Name);
            }
            ExportVocabInline(member, isNullable, vocab);
        }
        else
        {
            if (vocabType == null)
            {
                // No vocab
                throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, vocabAttr.Name);
            }
            ExportVocabType(member, type, vocabType, isNullable);
        }

        if (!vocabAttr.HasPropertyName)
        {
            vocabAttr.PropertyName = member.PropertyName();
        }

        if (vocabType != null)
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

    void ExportVocabType(MemberInfo member, Type type, VocabType vocabType, bool isNullable)
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
            _writer.
            SOL().
                Variable("$type", $"'{type.Name}'").
            EOL();
        }
        return this;
    }

    string DataType(Type type)
    {
        if (type.IsArray)
        {
            return DataType(type.GetElementType());
        }

        string typeName = Typescript.Types.ToPrimitive(type);
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
        var info = _nullableInfo.Create(prop);
        return (info.WriteState == NullabilityState.Nullable);
    }

    bool IsNullableRef(FieldInfo field)
    {
        var info = _nullableInfo.Create(field);
        return (info.WriteState == NullabilityState.Nullable);
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
        return (!type.IsPrimitive && !_nonExportTypes.Contains(type));
    }
}
