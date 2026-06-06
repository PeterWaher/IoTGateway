using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IDictionary"/> values.
	/// </summary>
	public class UntypedDictionaryEncoder : ObjectToonEncoder
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
		public override void Encode(object Object, int? Indent, ToonOutput Toon)
		{
			Dictionary<string, object> Typed = Prepare((IDictionary)Object);
			Toon.AppendAsObject(Typed, Indent, Typed.ContainsKey);
		}

		private static Dictionary<string, object> Prepare(IDictionary Dictionary)
		{
			Dictionary<string, object> Properties = new Dictionary<string, object>();
			IDictionaryEnumerator e = Dictionary.GetEnumerator();

			while (e.MoveNext())
				Properties[e.Key?.ToString() ?? string.Empty] = e.Value;

			return Properties;
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object)
		{
			return Prepare((IDictionary)Object).GetEnumerator();
		}

		/// <summary>
		/// Checks if an object can be folded to a shorter representation, and if so, 
		/// returns the folded name and value.
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="FoldedName">Folded name</param>
		/// <param name="FoldedValue">Folded value.</param>
		/// <returns>True if the object can be folded, otherwise false.</returns>
		public override bool CanFold(object Object, out string FoldedName, out object FoldedValue)
		{
			IDictionaryEnumerator e = ((IDictionary)Object).GetEnumerator();
			bool Found = false;

			FoldedName = null;
			FoldedValue = null;

			while (e.MoveNext())
			{
				if (Found)
					return false;
				else
				{
					Found = true;
					FoldedName = e.Key?.ToString() ?? string.Empty;
					FoldedValue = e.Value;
				}
			}

			return Found;
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
