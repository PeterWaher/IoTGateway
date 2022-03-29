using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts a <see cref="PhysicalQuantity"/> to a <see cref="Measurement"/>.
	/// </summary>
	public class PhysicalQuantityToMeasurement : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(PhysicalQuantity);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Measurement);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is PhysicalQuantity Q)
				return new Measurement(Q.Magnitude, Q.Unit, 0);
			else
				throw new ArgumentException("Expected Physical Quantity value.", nameof(Value));
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
			if (Value is PhysicalQuantity Q)
				return new Measurement(Q.Magnitude, Q.Unit, 0);
			else
				throw new ArgumentException("Expected Physical Quantity value.", nameof(Value));
		}
	}
}
