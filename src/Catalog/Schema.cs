// create schema object

// schema

using Catalog;


// create a schema object to store schema info serve for write and read
namespace Catalog
{
class Schema
{
    public string Name { get; set; }
    public List<Field> field{ get; set; } // get list of field
    public int Version { get; set; } // version of schema
    public IReadOnlyList<Field> Fields {get;}
    public int RecordSize {get;}
    public int NullBitmapSize{get;}

    internal Schema(string name,int version,List<Field> fields)
    {
        Name = name;
        Version = version;
        Fields = fields.AsReadOnly();
        NullBitmapSize = (int)Math.Ceiling(fields.Count / 8.0f);
        RecordSize = Fields.Last().Offset + Fields.Last().Length;
    }    
}

class SchemaBuilder
{
    private readonly string _name;
    private readonly List<Field> _fields = new();
    private ushort _nextFieldId = 0;
    private int _recordSize = 0;

    private SchemaBuilder(string name)
    {
        _name = name;
    }

    public static SchemaBuilder Create(string name)
    {
        return new SchemaBuilder(name);
    }

    public SchemaBuilder AddField(
        string name, DataType type,
        ushort length = 0,
        bool nullable = true,
        bool indexed = false
    )
    {
        // error handling

        if(_fields.Any(f => f.Name == name))
        {
            throw new InvalidOperationException($"Field '{name}' already exist");   
        }

        if (type == DataType.FixedString && length == 0)
        {
            throw new ArgumentException("FixedString requires length");
        }

        _fields.Add(new Field(fieldId: _nextFieldId++,
        name:name,
        type:type,
        length:length,
        isNullable:nullable,
        indexed:indexed
        )
        );

        return this;
    }

    private void ComputeLayout()
    {
        int nullBitmapSize = (int)Math.Ceiling(_fields.Count / 8.0f);
        int offset = nullBitmapSize;
        foreach (var field in _fields)
        {
            field.Offset = (ushort)offset;
            offset += GetFieldSize(field);
        }

        _recordSize = offset;  
    } 

    private int GetFieldSize(Field field)
    {
        return field.Type switch
        {
            DataType.Int32 => 4,
            DataType.Int64 => 8,
            DataType.Float => 4,
            DataType.Double => 8,
            DataType.Boolean => 1,
            DataType.FixedString => field.Length,
            DataType.String => 8, // pointer size
            DataType.Blob => field.Length, // pointer size
            _ => throw new ArgumentOutOfRangeException($"Unsupported data type: {field.Type}"),
        };
    }
    public Schema Build()
    {
        ComputeLayout();
        return new Schema(_name,version:1,_fields);
    }
}
}