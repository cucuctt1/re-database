
using System.Text;

namespace Catalog{
    public sealed class MetaWriter
    {
        private const uint MAGIC = 0x4D455441; // lmao it's META in ascii
        private const ushort META_VERSION =1;

        private readonly string _path;

        public MetaWriter(string path)
        {
            _path = path;
        }

        public void WriteSchemas(IEnumerable<Schema> schemas)
        {
            using var fs = new FileStream(_path, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            var schemaList = schemas.ToList();

            //

            bw.Write(MAGIC);
            bw.Write(META_VERSION);
            bw.Write(schemaList.Count);

            //table entry

            foreach(var schema in schemaList)
            {
                WriteTableEntry(bw, schema);
            }


        }

        private void WriteTableEntry(BinaryWriter bw,Schema schema)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(schema.Name);
            bw.Write((ushort)nameBytes.Length);
            bw.Write(nameBytes);

            bw.Write(schema.Version);

            byte[] schemaBinary = SerializeSchema(schema);

            bw.Write(schemaBinary.Length);
            bw.Write(schemaBinary);
        }

        private byte[] SerializeSchema(Schema schema)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((ushort)schema.Fields.Count);
            bw.Write(schema.RecordSize);
            bw.Write(schema.NullBitmapSize);

            foreach(var field in schema.Fields)
            {
                bw.Write(field.FieldId);

                byte[] fieldNameBytes = Encoding.UTF8.GetBytes(field.Name);
                bw.Write((ushort)fieldNameBytes.Length);
                bw.Write(fieldNameBytes);

                bw.Write((byte)field.Type);
                bw.Write(field.Length);
                bw.Write(field.Offset);

                bw.Write((byte)(field.IsNullable ? 1 : 0));
            }

            return ms.ToArray();
        }
}
}