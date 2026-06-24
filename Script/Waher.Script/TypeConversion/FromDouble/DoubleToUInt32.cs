using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.FromDouble
{
	/// <summary>
	/// Converts double numbers to 32-bit unsigned integer values.
	/// </summary>
	public class DoubleToUInt32 : ITypeConverter
	{
		/// <summary>
		/// Converts double numbers to 32-bit unsigned integer values.
		/// </summary>
		public Type From => typeof(double);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(uint);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is double d && d >= uint.MinValue && d <= uint.MaxValue && Math.Round(d) == d)
				return (uint)d;
			else 
				throw new ArgumentException("Expected 32-bit unsigned integer value.", nameof(Value));
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
			if (Value is double d && d >= uint.MinValue && d <= uint.MaxValue && Math.Round(d) == d)
				return new ObjectValue((uint)d);
			else
				throw new ArgumentException("Expected 32-bit unsigned integer value.", nameof(Value));
		}
	}
}
