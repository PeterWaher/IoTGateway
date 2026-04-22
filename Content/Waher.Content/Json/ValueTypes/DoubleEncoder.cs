using System;
using System.Globalization;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ValueTypes
{
	/// <summary>
	/// Encodes <see cref="double"/> values.
	/// </summary>
	public class DoubleEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="double"/> values.
		/// </summary>
		public DoubleEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			double Value = (double)Object;
			string s = Value.ToString(CultureInfo.InvariantCulture);
			int i = s.IndexOf('.');

			if (i < 0)
			{
				Json.Append(s);
				Json.Append(".0");
			}
			else
			{
				Json.Append(s.Substring(0, i));
				Json.Append('.');
				Json.Append(s.Substring(i + 1));
			}
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(double) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
