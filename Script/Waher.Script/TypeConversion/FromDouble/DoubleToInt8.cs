using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion.FromDouble
{
	/// <summary>
	/// Converts double numbers to 8-bit integer values.
	/// </summary>
	public class DoubleToInt8 : ITypeConverter
	{
		/// <summary>
		/// Converts double numbers to 8-bit integer values.
		/// </summary>
		public Type From => typeof(double);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(sbyte);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is double d && d >= sbyte.MinValue && d <= sbyte.MaxValue && Math.Round(d) == d)
				return (sbyte)d;
			else 
				throw new ArgumentException("Expected 8-bit integer value.", nameof(Value));
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
			if (Value is double d && d >= sbyte.MinValue && d <= sbyte.MaxValue && Math.Round(d) == d)
				return new ObjectValue((sbyte)d);
			else
				throw new ArgumentException("Expected 8-bit integer value.", nameof(Value));
		}
	}
}
