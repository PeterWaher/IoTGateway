using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ValueTypes
{
	/// <summary>
	/// Encodes <see cref="Enum"/> values.
	/// </summary>
	public class EnumEncoder : IToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Enum"/> values.
		/// </summary>
		public EnumEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			Toon.Append(TOON.Encode(Object.ToString()));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType.IsEnum ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
