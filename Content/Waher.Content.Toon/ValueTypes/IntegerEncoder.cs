using System;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ValueTypes
{
	/// <summary>
	/// Encodes <see cref="double"/> values.
	/// </summary>
	public class IntegerEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="double"/> values.
		/// </summary>
		public IntegerEncoder()
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
			Toon.Append(Object.ToString());
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return 
				ObjectType == typeof(int) || 
				ObjectType == typeof(long) || 
				ObjectType == typeof(short) || 
				ObjectType == typeof(byte) ||
				ObjectType == typeof(uint) || 
				ObjectType == typeof(ulong) || 
				ObjectType == typeof(ushort) || 
				ObjectType == typeof(sbyte) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
