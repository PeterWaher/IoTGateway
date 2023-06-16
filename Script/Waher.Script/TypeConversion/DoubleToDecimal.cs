using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts double numbers to decimal numbers.
	/// </summary>
	public class DoubleToDecimal : ITypeConverter
	{
		/// <summary>
		/// Converts double numbers to decimal numbers.
		/// </summary>
		public Type From => typeof(double);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(decimal);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is double d)
				return (decimal)d;
			else
				throw new ArgumentException("Expected double value.", nameof(Value));
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>, encapsulated in an <see cref="IElement"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public IElement ConvertToElement(object Value)
		{
			if (Value is double d)
				return new ObjectValue((decimal)d);
			else
				throw new ArgumentException("Expected double value.", nameof(Value));
		}
	}
}
