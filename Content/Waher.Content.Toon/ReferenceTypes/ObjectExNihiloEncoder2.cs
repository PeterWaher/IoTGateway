using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable{T}"/> values.
	/// </summary>
	public class ObjectExNihiloEncoder2 : ObjectToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable{T}"/> values.
		/// </summary>
		public ObjectExNihiloEncoder2()
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
				Dictionary<string, object> Prepared = Prepare(Object);
				Toon.AppendAsObject(Prepared, Indent, Prepared.ContainsKey);
			}
			else
				Toon.AppendAsObject((IEnumerable<KeyValuePair<string, IElement>>)Object, Indent, null);
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object)
		{
			return Prepare(Object).GetEnumerator();
		}

		private static Dictionary<string, object> Prepare(object Object)
		{
			Dictionary<string, object> Parameters = new Dictionary<string, object>();

			foreach (KeyValuePair<string, IElement> P in (IEnumerable<KeyValuePair<string, IElement>>)Object)
				Parameters[P.Key] = P.Value.AssociatedObjectValue;

			return Parameters;
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

			foreach (KeyValuePair<string, IElement> P in (IEnumerable<KeyValuePair<string, IElement>>)Object)
			{
				if (Found)
					return false;
				else
				{
					Found = true;
					FoldedName = P.Key;
					FoldedValue = P.Value.AssociatedObjectValue;
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
			return typeof(IEnumerable<KeyValuePair<string, IElement>>).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
