using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.ToDouble
{
	/// <summary>
	/// Converts strings to double numbers.
	/// </summary>
	public class StringToDouble : ITypeConverter
	{
		/// <summary>
		/// Converts string numbers to double numbers.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(double);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is string s && Expression.TryParse(s, out double d))
				return d;
			else
				throw new ArgumentException("Expected double-precision floating-point value.", nameof(Value));
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
			if (Value is string s && Expression.TryParse(s, out double d))
				return new DoubleNumber(d);
			else
				throw new ArgumentException("Expected double-precision floating-point value.", nameof(Value));
		}
	}
}
