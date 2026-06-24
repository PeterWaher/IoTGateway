using System;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;

namespace Waher.Script.Persistence.TypeConversion
{
	/// <summary>
	/// Converts case-insensitive strings to normal strings.
	/// </summary>
	public class CiStringToString : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(CaseInsensitiveString);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(string);

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		public double Weight => 1.0;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvert(object Value, out object Result)
		{
			if (Value is null)
			{
				Result = null;
				return true;
			}
			else if (Value is CaseInsensitiveString s)
			{
				Result = s.Value;
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
			if (Value is null)
			{
				Result = ObjectValue.Null;
				return true;
			}
			else if (Value is CaseInsensitiveString s)
			{
				Result = new StringValue(s.Value);
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
