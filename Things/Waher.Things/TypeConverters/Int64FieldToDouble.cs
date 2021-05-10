using System;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="Int64Field"/> to a <see cref="Double"/>.
	/// </summary>
	public class Int64FieldToDouble : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="Int64Field"/> to a <see cref="Double"/>.
		/// </summary>
		public Int64FieldToDouble()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(Int64Field);

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
			if (Value is Int64Field Field)
				return (double)Field.Value;
			else
				throw new ArgumentException("Not a Int64Field.", nameof(Value));
		}
	}
}
