using System;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ValueTypes
{
	/// <summary>
	/// Encoder used for value types, if no other encoder is found.
	/// </summary>
	public class DefaultValueTypeEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encoder used for value types, if no other encoder is found.
		/// </summary>
		public DefaultValueTypeEncoder()
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
			Toon.Append(TOON.Encode(Object.ToString(), !Toon.Empty));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return ObjectType.IsValueType ? Grade.Barely : Grade.NotAtAll;
		}
	}
}
