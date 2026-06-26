using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.FromString
{
	/// <summary>
	/// Converts strings to Boolean values.
	/// </summary>
	public class StringToBoolean : ITypeConverter
	{
		/// <summary>
		/// Converts string values to Boolean values.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(bool);

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
			if (Value is string s)
			{
				switch (s.ToLower())
				{
					case "1":
					case "true":
					case "yes":
					case "on":
						Result = true;
						return true;

					case "0":
					case "false":
					case "no":
					case "off":
						Result = false;
						return true;
				}
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
			if (this.TryConvert(Value, out object d) && d is bool b)
			{
				Result = new BooleanValue(b);
				return true;
			}

			Result = null;
			return false;
		}
	}
}
