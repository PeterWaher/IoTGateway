using System;
using System.Globalization;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ValueTypes
{
	/// <summary>
	/// Encodes <see cref="decimal"/> values.
	/// </summary>
	public class DecimalEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="decimal"/> values.
		/// </summary>
		public DecimalEncoder()
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
			decimal d = (decimal)Object;
			Toon.Append(d.ToString("0.############################", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(decimal) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
