using System;
using System.Globalization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.FromString
{
	/// <summary>
	/// Converts string values to 64-bit unsigned integer values.
	/// </summary>
	public class StringToUInt64 : ITypeConverter
	{
		/// <summary>
		/// Converts string values to 64-bit unsigned integer values.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(ulong);

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		public double Weight => 0.5;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvert(object Value, out object Result)
		{
			if (Value is string s && ulong.TryParse(s, NumberStyles.Integer, null, out ulong i))
			{
				Result = i;
				return true;
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvertToElement(object Value, out IElement Result)
		{
			if (Value is string s && ulong.TryParse(s, NumberStyles.Integer, null, out ulong i))
			{
				Result = new ObjectValue(i);
				return true;
			}
			else
			{
				Result = null;
				return false;
			}
		}
	}
}
