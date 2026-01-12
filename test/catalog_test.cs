// catalog test class

//lib load
using System;
using System.Diagnostics;
using Catalog;

namespace Test{
class CatalogTest
{
    public static void Run(string[] args)
    {
        Console.WriteLine("Start testing");
        CatalogManager catalogManager = new CatalogManager();
        var schema = SchemaBuilder
            .Create("users")
            .AddField("id", DataType.Int32, nullable: false)
            .AddField("name", DataType.FixedString, length: 16)
            .Build();

        var writer = new MetaWriter("meta.dat");
        writer.WriteSchemas(new[] { schema });

        // ---- restart engine ----

        var reader = new MetaReader("meta.dat");
        var schemas = reader.ReadSchemas();

        var loaded = schemas["users"];
        Debug.Assert(loaded.RecordSize == schema.RecordSize);
        Debug.Assert(loaded.Fields[1].Name == "name");
    }
}
}