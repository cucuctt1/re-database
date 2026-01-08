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
        var schema = SchemaBuilder.Create("user")
        .AddField("id",DataType.Int32,nullable:false)
        .AddField("name",DataType.FixedString,length :50)
        .AddField("age",DataType.Int32)
        .Build();
        // get some schema info
        Console.WriteLine($"Schema Name: {schema.Name}");
        Console.WriteLine($"Schema Version: {schema.Version}");
        

        // debug assert
        Debug.Assert(schema.Fields[0].Offset == schema.NullBitmapSize);
        Debug.Assert(schema.RecordSize > 0);
    }
}
}