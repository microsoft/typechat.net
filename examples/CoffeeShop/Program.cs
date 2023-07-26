// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace CoffeeShop;

public class CoffeeShop : ConsoleApp
{
    CoffeeShop()
    {
    }

    protected override async Task ProcessRequestAsync(string line, CancellationToken cancelToken)
    {
    }

    static TypeSchema GetSchema()
    {
        return TypescriptExporter.GenerateSchema(typeof(Cart), CoffeeShopVocabs.All());
    }

    /*
    public static async Task<int> Main(string[] args)
    {
        CoffeeShop app = new CoffeeShop();
        await app.RunAsync();
        return 0;
    }
    */

    public static int Main(string[] args)
    {
        try
        {
            var schema = GetSchema();
            Console.WriteLine(schema.Schema.Text);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return 0;
    }
}
