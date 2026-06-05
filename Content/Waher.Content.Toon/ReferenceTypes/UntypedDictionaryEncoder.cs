using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IDictionary"/> values.
	/// </summary>
	public class UntypedDictionaryEncoder : MultiRowToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IDictionary"/> values.
		/// </summary>
		public UntypedDictionaryEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public override void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			IDictionary Dictionary = (IDictionary)Object;

			ChunkedList<KeyValuePair<string, object>> Properties = new ChunkedList<KeyValuePair<string, object>>();

			foreach (object Key in Dictionary.Keys)
				Properties.Add(new KeyValuePair<string, object>(Key.ToString(), Dictionary[Key]));

			TOON.Encode(Properties, Indent, Toon);
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(IDictionary).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
