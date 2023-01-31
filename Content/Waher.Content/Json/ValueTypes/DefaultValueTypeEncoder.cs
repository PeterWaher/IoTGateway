using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ValueTypes
{
	/// <summary>
	/// Encoder used for value types, if no other encoder is found.
	/// </summary>
	public class DefaultValueTypeEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encoder used for value types, if no other encoder is found.
		/// </summary>
		public DefaultValueTypeEncoder()
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
			Json.Append('"');
			Json.Append(JSON.Encode(Object.ToString()));
			Json.Append('"');
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType.GetTypeInfo().IsValueType ? Grade.Barely : Grade.NotAtAll;
		}
	}
}
