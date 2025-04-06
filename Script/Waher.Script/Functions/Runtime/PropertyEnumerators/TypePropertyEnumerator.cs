using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Runtime.PropertyEnumerators
{
	/// <summary>
	/// Enumerates properties in a <see cref="Type"/> object.
	/// </summary>
	public class TypePropertyEnumerator : IPropertyEnumerator
	{
		/// <summary>
		/// Enumerates properties in a <see cref="Type"/> object.
		/// </summary>
		public TypePropertyEnumerator()
		{
		}

		/// <summary>
		/// Enumerates the properties of an object (of a type it supports).
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Property enumeration as a script element.</returns>
		public Task<IElement> EnumerateProperties(object Object)
		{
			if (Object is Type T)
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (PropertyInfo PI in T.GetRuntimeProperties())
				{
					if (PI.CanRead && PI.GetMethod.IsPublic && PI.GetIndexParameters().Length == 0)
						Elements.Add(new StringValue(PI.Name));
				}

				return Task.FromResult<IElement>(new ObjectVector(Elements));
			}
			else
				return Task.FromResult<IElement>(ObjectValue.Null);
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			if (typeTypeInfo.IsAssignableFrom(Object.GetTypeInfo()))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		private static readonly TypeInfo typeTypeInfo = typeof(Type).GetTypeInfo();
	}
}
