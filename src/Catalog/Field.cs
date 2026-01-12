// field object


namespace Catalog
{
    public class Field
    {
        public DataType Type { get;  } // set using string get get datatype from enum
        public ushort Length { get;  } // length of field
        public string Name { get; }
        public ushort Version { get; } // version of field
        public ushort Offset { get; internal set; } // offset in record
        public ushort FieldId { get;  } // field id
        public bool IsNullable { get; } // is nullable
        public bool Indexed { get;}

        internal Field(
            ushort fieldId,
            string name,
            DataType type,
            ushort length,
            ushort offset,
            bool isNullable,
            bool indexed
        )
        {
            FieldId = fieldId;
            Name = name;
            Type = type;
            Length = length;
            Offset = offset;
            IsNullable = isNullable;
            Indexed = indexed;
        }
    }
}