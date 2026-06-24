using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="QuantityField"/> to a <see cref="PhysicalQuantity"/>.
	/// </summary>
	public class QuantityFieldToPhysicalQuantity : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="QuantityField"/> to a <see cref="PhysicalQuantity"/>.
		/// </summary>
		public QuantityFieldToPhysicalQuantity()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(QuantityField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(PhysicalQuantity);

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		public double Weight => 0.5;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvert(object Value, out object Result)
		{
			if (Value is QuantityField Field)
			{
				Result = Field.Quantity;
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
			if (Value is QuantityField Field)
			{
				Result = Field.Quantity;
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
