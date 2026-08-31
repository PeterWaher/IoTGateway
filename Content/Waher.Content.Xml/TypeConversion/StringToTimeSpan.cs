using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;

namespace Waher.Content.Xml.TypeConversion
{
	/// <summary>
	/// Converts a <see cref="string"/> to a <see cref="TimeSpan"/>.
	/// </summary>
	public class StringToTimeSpan : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="string"/> to a <see cref="TimeSpan"/>.
		/// </summary>
		public StringToTimeSpan()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(TimeSpan);

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		public double Weight => 0.9;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvert(object Value, out object Result)
		{
			if (Value is string s &&
				TimeSpan.TryParse(s, out TimeSpan TS))
			{
				Result = TS;
				return true;
			}

			Result = null;
			return false;
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
			if (Value is string s &&
				TimeSpan.TryParse(s, out TimeSpan TS))
			{
				Result = new ObjectValue(TS);
				return true;
			}

			Result = null;
			return false;
		}
	}
}
