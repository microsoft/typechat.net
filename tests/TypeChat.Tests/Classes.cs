// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.TypeChat.Tests;

public static class Classes
{
    public static IEnumerable<KeyValuePair<string, string>> Shops()
    {
        yield return new KeyValuePair<string, string>("CoffeeShop", "Order Coffee Drinks (Italian names included) and Baked Goods");
        yield return new KeyValuePair<string, string>("Mystery Bookshop", "A bookstore that specializes in mystery books");
        yield return new KeyValuePair<string, string>("Bookstore", "A bookstore that sells all kinds of books");
        yield return new KeyValuePair<string, string>("Drugstore", "A drugstore that sells health and beauty products");
    }
}
