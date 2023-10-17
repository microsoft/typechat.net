// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class PersonAPI : IPersonApi
{
    public static PersonAPI Default = new PersonAPI();
    public static Api Caller = new Api(Default);

    public Person makePerson(Name name, int age)
    {
        return new Person { Name = name, Age = age };
    }
    public bool isPerson(Person person, Name name, int age)
    {
        return (person.Name.CompareTo(name) == 0 &&
               person.Age == age);
    }
    public string toJson(Person person)
    {
        return Json.Stringify(person);
    }
    public string toJsonArray(Person[] person)
    {
        return Json.Stringify(person);
    }
    public string listBooks(AuthorPerson author)
    {
        string books = string.Join("|", author.Books);
        return Json.Stringify(books);
    }

    public Person[] changeCase(Person[] persons, bool upper)
    {
        foreach (var person in persons)
        {
            person.ChangeCase(upper);
        }
        return persons;
    }

    public bool haveSameName(Person[] persons)
    {
        Person prev = null;
        for (int i = 0; i < persons.Length; ++i)
        {
            Person next = persons[i];
            if (prev is not null &&
                !prev.HasSameName(next))
            {
                return false;
            }
            prev = next;
        }

        return true;
    }
}
