using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="Int64Field"/> to a <see cref="Int64"/>.
	/// </summary>
	public class Int64FieldToInt64 : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="Int64Field"/> to a <see cref="Int64"/>.
		/// </summary>
		public Int64FieldToInt64()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(Int64Field);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(long);

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
			if (Value is Int64Field Field)
			{
				Result = Field.Value;
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
			if (Value is Int64Field Field)
			{
				Result = new ObjectValue(Field.Value);
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
