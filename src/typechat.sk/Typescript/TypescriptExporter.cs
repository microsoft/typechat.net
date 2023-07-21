// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat.SemanticKernel.Typescript;

public class TypescriptExporter
{
    public static TypeSchema GenerateSchema(Type type)
    {
        using StringWriter writer = new StringWriter();
        TypescriptExporter exporter = new TypescriptExporter(writer);
        exporter.Export(type);
        string schema = writer.ToString();

        return new TypeSchema(type, schema);
    }

    TypescriptWriter _writer;
    HashSet<Type>? _exportedTypes;
    HashSet<Type> _nonExportTypes;
    Queue<Type>? _pendingTypes;

    public TypescriptExporter(TextWriter writer)
        : this(new TypescriptWriter(writer))
    {
    }

    public TypescriptExporter(TypescriptWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        _writer = writer;
        _nonExportTypes = new HashSet<Type>()
        {
            typeof(string),
            typeof(Array),
            typeof(Nullable<>)
        };
    }

    public TypescriptWriter Writer => _writer;
    public bool IncludeSubclasses { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
    public bool EnumsAsLiterals { get; set; } = false;

    public TypescriptExporter Clear()
    {
        _writer.Clear();
        _exportedTypes?.Clear();
        _pendingTypes?.Clear();
        return this;
    }

    public TypescriptExporter Export(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        AddPending(type);
        return ExportQueued();
    }

    public TypescriptExporter ExportQueued()
    {
        Type type;
        while ((type = GetPending()) != null)
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
                if (IncludeSubclasses)
                {
                    AddPending(type.Subclasses());
                    AddPending(type.Implementors());
                }
            }
        }
        return this;
    }

    public TypescriptExporter ExportClass(Type type)
    {
        Debug.Assert(type.IsClass);

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
            ExportMembers(type);
            _writer.PopIndent();
        }
        _writer.EndInterface(typeName);

        AddExported(type);

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

    TypescriptExporter ExportMembers(Type type)
    {
        return ExportProperties(type).
               ExportFields(type);
    }

    TypescriptExporter ExportProperties(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
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

    TypescriptExporter ExportEnumLiterals(Type type)
    {
        if (!IncludeComments)
        {
            _writer.Literals(Enum.GetNames(type));
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

    TypescriptExporter ExportProperty(PropertyInfo property)
    {
        ExportComments(property);
        ExportVariable(property, property.PropertyType);
        return this;
    }

    TypescriptExporter ExportField(FieldInfo field)
    {
        ExportComments(field);
        ExportVariable(field, field.FieldType);
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
        Type? nullableType = type.GetNullableType();
        _writer.
            SOL().
                Variable(
                    member.PropertyName(),
                    DataType((nullableType != null) ? nullableType : type),
                    type.IsArray,
                    (nullableType != null)
                ).
            EOL();
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

    void AddPending(Type type)
    {
        if (!IsExported(type) && ShouldExport(type))
        {
            _pendingTypes ??= new Queue<Type>();
            _pendingTypes.Enqueue(type);
        }
    }

    public void AddPending(IEnumerable<Type> types)
    {
        foreach (var t in types)
        {
            AddPending(t);
        }
    }

    public Type? GetPending()
    {
        if (_pendingTypes != null && _pendingTypes.Count > 0)
        {
            return _pendingTypes.Dequeue();
        }

        return null;
    }

    bool IsExported(Type type)
    {
        return (_exportedTypes != null) ? _exportedTypes.Contains(type) : false;
    }

    bool ShouldExport(Type type)
    {
        if (type.IsPrimitive ||
            type.IsNullable())
        {
            return false;
        }
        if (type.IsArray)
        {
            type = type.GetElementType();
        }
        return (!type.IsPrimitive && !_nonExportTypes.Contains(type));
    }

    void AddExported(Type type)
    {
        _exportedTypes ??= new HashSet<Type>();
        _exportedTypes.Add(type);
    }
}
