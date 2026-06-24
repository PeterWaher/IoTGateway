using System;
using System.Security.Cryptography;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts an object of one type to an object of another type.
	/// </summary>
	public interface ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		Type From { get; }

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		Type To { get; }

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		double Weight { get; }

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		bool TryConvert(object Value, out object Result);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		bool TryConvertToElement(object Value, out IElement Result);
	}
}
