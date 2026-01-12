
using System.Text;

namespace Catalog
{
    public sealed class MetaReader
    {
        private const uint MAGIC = 0x4D455441; // lmao it's META in ascii

        private readonly string _path;

        public MetaReader(string path)
        {
            _path = path;
        }

        public IReadOnlyDictionary<string,Schema> ReadSchemas()
        {
            using var fs = new FileStream(_path,FileMode.Open,FileAccess.Read);
            using var br = new BinaryReader(fs);

            // header validate

            uint magic = br.ReadUInt32();
            if (magic != MAGIC)
            {
                throw new InvalidDataException("invalid meta file");
            }

            ushort MetaVersion = br.ReadUInt16();
            int TableCount = br.ReadInt32();

            var schemas = new Dictionary<string,Schema>();
            for(int i = 0; i < TableCount; i++)
            {
                Schema schema =  ReadTableEntry(br);
                schemas.Add(schema.Name,schema);
            }

            return schemas;
        }
        
        private Schema ReadTableEntry(BinaryReader br)
        {
            ushort nameLength = br.ReadUInt16();
            string name = Encoding.UTF8.GetString(ReadBytesExact(br, nameLength));

            int version = br.ReadInt32();

            int schemaLength = br.ReadInt32();
            byte[] schemaBinary = ReadBytesExact(br, schemaLength);

            return DeserializeSchema(name, version, schemaBinary);
        }

        private Schema DeserializeSchema(string TableName,int schemaVersion,byte[] data)
        {
            using var ms = new MemoryStream(data, writable: false);
            using var br = new BinaryReader(ms);

            ushort fieldCount = br.ReadUInt16();
            int recordSize = br.ReadInt32();
            int nullBitmapSize = br.ReadInt32();

            var fields = new List<Field>(fieldCount);

            for(int i =0;i<fieldCount;i++)
            {
                ushort fieldId = br.ReadUInt16();

                ushort NameLength = br.ReadUInt16();
                string Name = Encoding.UTF8.GetString(ReadBytesExact(br,NameLength));

                DataType type = (DataType)br.ReadByte();
                ushort Length = br.ReadUInt16();
                ushort Offset = br.ReadUInt16();
                bool IsNullable = br.ReadByte() == 1;
                bool isIndexed = false; // indexed info is not stored in meta for now

                var field = new Field(
                    fieldId:fieldId,
                    name:Name,
                    type:type,
                    length:Length,
                    offset:Offset,
                    isNullable:IsNullable,
                    indexed:isIndexed
                );

                fields.Add(field);
            }

            return new Schema(TableName,schemaVersion,fields,recordSize,nullBitmapSize);

        }

        private static byte[] ReadBytesExact(BinaryReader br,int count)
        {
            var bytes = br.ReadBytes(count);
            if(bytes.Length != count)
            {
                throw new EndOfStreamException("unexpected end of stream");

            }
            return bytes;
        }
    }
}