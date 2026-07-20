using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.FromString
{
	/// <summary>
	/// Converts a string value to a <see cref="Uri"/>, if possible.
	/// </summary>
	public class StringToUri : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Uri);

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
			if (Value is string s && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri Url))
			{
				Result = Url;
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
			if (Value is string s && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri Url))
			{
				Result = new ObjectValue(Url);
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
