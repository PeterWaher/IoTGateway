using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable{T}"/> values.
	/// </summary>
	public class TypedDictionaryEncoder2 : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable{T}"/> values.
		/// </summary>
		public TypedDictionaryEncoder2()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			JSON.Encode((IEnumerable<KeyValuePair<string, object>>)Object, Indent, Json, null);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(IEnumerable<KeyValuePair<string, object>>).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
