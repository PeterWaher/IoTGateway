using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="QuantityField"/> to a <see cref="Double"/>.
	/// </summary>
	public class QuantityFieldToDouble : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="QuantityField"/> to a <see cref="Double"/>.
		/// </summary>
		public QuantityFieldToDouble()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(QuantityField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Double);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is QuantityField Field)
				return Field.Quantity.Magnitude;
			else
				throw new ArgumentException("Not a QuantityField.", nameof(Value));
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
			if (Value is QuantityField Field)
				return new DoubleNumber(Field.Value);
			else
				throw new ArgumentException("Not a QuantityField.", nameof(Value));
		}
	}
}
