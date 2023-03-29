using FluentAssertions;
using NickStrupat.ObjectToDictionary;
using Xunit;

namespace UnitTests;

public class ToDictionaryUnitTests
{
	[Fact]
	public void SealedClass_WhereReferenceAndInstanceTypeMatch()
	{
		Sealed @sealed = new Sealed { Text = "Test" };
		var dict = @sealed.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Text");
		dict["Text"].Should().Be("Test");
	}
	
	[Fact]
	public void SealedClass_WhereReferenceTypeIsBaseAndInstanceTypeIsDerived()
	{
		Interface @sealed = new Sealed { Text = "Test" };
		var dict = @sealed.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Text");
		dict["Text"].Should().Be("Test");
	}
	
	[Fact]
	public void DerivedClass_WherePropertyGetterIsOverridden()
	{
		Abstract @abstract = new Derived { Text = "Test" };
		var dict = @abstract.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Text");
		dict["Text"].Should().Be("TestDerived");
	}
	
	[Fact]
	public void StructType()
	{
		Struct @struct = new Struct { Text = "Test" };
		var dict = @struct.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Text");
		dict["Text"].Should().Be("Test");
	}

	[Fact]
	public void AnonymousType()
	{
		var anon = new {Text = "Test"};
		var dict = anon.ToDictionary();

		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Text");
		dict["Text"].Should().Be("Test");
	}
	
	[Fact]
	public void TupleType()
	{
		var tuple = Tuple.Create("Test");
		var dict = tuple.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(1);
		dict.Should().ContainKey("Item1");
		dict["Item1"].Should().Be("Test");
	}
	
	[Fact]
	public void ValueTupleType()
	{
		var tuple = ("Test", 1);
		var dict = tuple.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().HaveCount(2);
		dict.Should().ContainKey("Item1");
		dict["Item1"].Should().Be("Test");
		dict.Should().ContainKey("Item2");
		dict["Item2"].Should().Be(1);
	}
	[Fact]
	public void EnumType()
	{
		var @enum = Enum.What;
		var dict = @enum.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().BeEmpty();
	}
	
	[Fact]
	public void DelegateType()
	{
		var @delegate = new Delegate(() => { });
		var dict = @delegate.Method.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().NotBeEmpty();
	}

	[Theory]
	[InlineData(1)]
	[InlineData(1u)]
	[InlineData(1L)]
	[InlineData(1UL)]
	[InlineData(1.0d)]
	[InlineData(1.0f)]
	[InlineData(true)]
	[InlineData('a')]
	[InlineData("Test")]
	public void PrimitiveTypes<T>(T value) where T : notnull
	{
		var dict = value.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().BeEmpty();
	}

	[Fact]
	public void ArrayType()
	{
		var array = new[] {1, 2, 3};
		var dict = array.ToDictionary();
		
		dict.Should().BeOfType<Dictionary<String, Object>>();
		dict.Should().BeEmpty();
	}

	sealed class Sealed : Interface { public required String Text { get; set; } }
	class Virtual { public required String Text { get; set; } }
	struct Struct { public required String Text { get; set; } }
	enum Enum { What }
	interface Interface {}
	delegate void Delegate();
	abstract class Abstract { public virtual required String Text { get; set; } }
	class Derived : Abstract { public override required String Text { get => base.Text + "Derived"; set => base.Text = value; } }
}