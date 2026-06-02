using System;
using System.Text;
using Waher.Content.Toon;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Encoding
{
	/// <summary>
	/// Encodes <see cref="string"/> values.
	/// </summary>
	public class CaseInsensitiveStringToonEncoder : IToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="string"/> values.
		/// </summary>
		public CaseInsensitiveStringToonEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			CaseInsensitiveString s = (CaseInsensitiveString)Object;
			
			Toon.Append(TOON.Encode(s.Value));
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(CaseInsensitiveString) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
