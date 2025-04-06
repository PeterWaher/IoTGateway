using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IDictionary"/> values.
	/// </summary>
	public class UntypedDictionaryEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IDictionary"/> values.
		/// </summary>
		public UntypedDictionaryEncoder()
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
			IDictionary Dictionary = (IDictionary)Object;

			ChunkedList<KeyValuePair<string, object>> Properties = new ChunkedList<KeyValuePair<string, object>>();

			foreach (object Key in Dictionary.Keys)
				Properties.Add(new KeyValuePair<string, object>(Key.ToString(), Dictionary[Key]));

			JSON.Encode(Properties, Indent, Json);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(IDictionary).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
