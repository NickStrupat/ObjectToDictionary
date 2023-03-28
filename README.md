# ObjectToDictionary
Convert .NET objects to `Dictionary<string, object>` with cached IL-emitted converters for both value and reference types.

It is an optimized version of the following code:
```csharp
using static System.Reflection.BindingFlags;

public static class Extensions
{
    public static Dictionary<string, object?> ToDictionary(this object obj) => obj
        .GetType()
        .GetProperties(Public | Instance)
        .Where(x => x.GetGetMethod() != null) // where the property has a public getter
        .ToDictionary(x => x.Name, x => x.GetValue(obj));
}
```

# Sample C# 11 program

```csharp
using NickStrupat.ObjectToDictionary;

var foo = new Foo
{
	Name = "Nick",
	BirthDate = new DateTime(1986, 7, 17)
};
var d1 = foo.ToDictionary();

// d1 is a Dictionary<string, object> with ["Name"] = "Nick" and ["BirthDate"] = new DateTime(1986, 7, 17)

Bar bar = foo;
var d2 = bar.ToDictionary();

// d2 is the same as d1, despite being accessed through a base class reference

Baz baz = new Baz { Name = "Nick" };
var d3 = baz.ToDictionary();

// d3 is a Dictionary<string, object> with ["Name"] = "Nick"
// Note that Baz is a struct

class Foo : Bar
{
	public DateTime BirthDate { get; set; }
}

class Bar
{
	public required string Name { get; set; }
}

struct Baz
{
	public required string Name { get; set; }
}
```

## Implementation details

- The returned Dictionary is an ordinary instance of the `Dictionary<string, object>` class
- Primitive types, strings, and arrays return an empty dictionary
- Only public fields and properties with a public getter are included in the dictionary
- Keys are the names of the properties of the object
- Values are the values of the properties of the object
- Values are boxed if they are value types
- Values are not boxed if they are reference types

The library uses a combination of reflection and IL-emitted code to convert objects to dictionaries. Reflection is used to get the properties of the object and to get the values of the properties. The IL-emitted methods are cached, so that the same converter is used for the same type.

If the type of the object is sealed or a value type, the cached converter method is stored in a static generic class. This means the runtime can look up the converter method without having to go through the dictionary lookup.

If the type of the object is not sealed nor a value type, the cached converter method is stored in a static dictionary. This means the runtime has to go through the dictionary lookup to find the converter method. This is still much faster than using reflection to get the properties and values of the object.