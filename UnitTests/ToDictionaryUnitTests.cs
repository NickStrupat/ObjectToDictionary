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
	
	sealed class Sealed : Interface { public required String Text { get; set; } }
	class Virtual { public required String Text { get; set; } }
	struct Struct { public required String Text { get; set; } }
	enum Enum { What }
	interface Interface {}
	delegate void Delegate();
}