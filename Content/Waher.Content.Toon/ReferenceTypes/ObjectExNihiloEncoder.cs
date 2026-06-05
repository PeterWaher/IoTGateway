using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="Dictionary{String, IElement}"/> values.
	/// </summary>
	public class ObjectExNihiloEncoder : MultiRowToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Dictionary{String, IElement}"/> values.
		/// </summary>
		public ObjectExNihiloEncoder()
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
			TOON.Encode((Dictionary<string, IElement>)Object, Indent, Toon);
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(Dictionary<string, IElement>).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
