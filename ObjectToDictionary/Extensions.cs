using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static System.Reflection.BindingFlags;

namespace NickStrupat.ObjectToDictionary;

using Dict = Dictionary<String, Object>;

public static class Extensions
{
	public static Dict ToDictionary<T>(this T obj) where T : notnull => Cache<T>.Map(obj);
	
	private static class Cache<T> where T : notnull
	{
		public static Dict Map(T obj) => Mapper.Invoke(obj);
		
		private static readonly Func<T, Dict> Mapper =
			typeof(T).IsEnum | typeof(T).IsPrimitive | typeof(T) == typeof(String) | typeof(T).IsArray | typeof(T) == typeof(Delegate)
				? static _ => new()
				: typeof(T).IsValueType | typeof(T).IsSealed
					? CreateExactMapper()
					: CreateDynamicMapper();

		private static Func<T, Dict> CreateExactMapper()
		{
			var methodName = typeof(T).FullName ?? Guid.NewGuid().ToString("N") + "_ObjectToDictionaryMapper";
			var dm = new DynamicMethod(methodName, returnType: typeof(Dict), parameterTypes: new[] { typeof(T) });
			var il = dm.GetILGenerator();
			
			var properties = typeof(T).GetProperties(Public | Instance).Where(p => p.GetGetMethod() is not null);
			var fields = typeof(T).GetFields(Public | Instance);

			il.Emit(OpCodes.Newobj, DictionaryConstructorInfo);
			foreach (var property in properties)
			{
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Ldstr, property.Name);
				il.Emit(typeof(T).IsValueType ? OpCodes.Ldarga_S : OpCodes.Ldarg, 0);
				il.Emit(OpCodes.Call, property.GetGetMethod(nonPublic: false)!);
				if (property.PropertyType.IsValueType)
					il.Emit(OpCodes.Box, property.PropertyType);
				il.Emit(OpCodes.Callvirt, DictionaryAddMethodInfo);
			}
			foreach (var field in fields)
			{
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Ldstr, field.Name);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, field);
				if (field.FieldType.IsValueType)
					il.Emit(OpCodes.Box, field.FieldType);
				il.Emit(OpCodes.Callvirt, DictionaryAddMethodInfo);
			}
			il.Emit(OpCodes.Ret);
		
			return dm.CreateDelegate<Func<T, Dict>>();
		}

		private static Func<T, Dict> CreateDynamicMapper() =>
			static obj => DerivedMappers.GetOrAdd(obj.GetType(), DynamicMapperFactory).Invoke(obj);

		private static readonly ConcurrentDictionary<Type, Func<T, Dict>> DerivedMappers = new();

		private static Func<T, Dict> DynamicMapperFactory(Type type)
		{
			var del = CreateDynamicMapperWrapper<Object, Object>;
			var methodInfo = del.Method.GetGenericMethodDefinition().MakeGenericMethod(typeof(T), type);
			return (Func<T, Dict>) methodInfo.Invoke(null, null)!;
			
			static Func<TBase, Dict> CreateDynamicMapperWrapper<TBase, TDerived>() where TDerived : class, TBase
			{
				var mapper = Cache<TDerived>.CreateExactMapper();
				return b => mapper.Invoke(Unsafe.As<TBase, TDerived>(ref b)); // we know it's TDerived
			}
		}
	}

	private static readonly ConstructorInfo DictionaryConstructorInfo = typeof(Dict).GetConstructor(Type.EmptyTypes)!;
	private static readonly MethodInfo DictionaryAddMethodInfo = typeof(Dict).GetMethod(nameof(Dict.Add), new[] { typeof(String), typeof(Object) })!;
}