using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable{T}"/> values.
	/// </summary>
	public class TypedDictionaryEncoder2 : ObjectToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable{T}"/> values.
		/// </summary>
		public TypedDictionaryEncoder2()
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
			if (Toon.KeyFolding)
			{
				IEnumerable<KeyValuePair<string, object>> Prepared = (IEnumerable<KeyValuePair<string, object>>)Object;
				Dictionary<string, object> Dictionary = new Dictionary<string, object>();

				foreach (KeyValuePair<string, object> P in Prepared)
					Dictionary[P.Key] = P.Value;

				Toon.AppendAsObject(Dictionary, Indent, Dictionary.ContainsKey);
			}
			else
				Toon.AppendAsObject((IEnumerable<KeyValuePair<string, object>>)Object, Indent, null);
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object)
		{
			return ((IEnumerable<KeyValuePair<string, object>>)Object).GetEnumerator();
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
			bool Found = false;

			FoldedName = null;
			FoldedValue = null;

			foreach (KeyValuePair<string, object> P in (IEnumerable<KeyValuePair<string, object>>)Object)
			{
				if (Found)
					return false;
				else
				{
					Found = true;
					FoldedName = P.Key;
					FoldedValue = P.Value;
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
			return typeof(IEnumerable<KeyValuePair<string, object>>).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
