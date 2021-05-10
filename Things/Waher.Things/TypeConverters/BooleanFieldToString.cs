using System;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="BooleanField"/> to a <see cref="Boolean"/>.
	/// </summary>
	public class BooleanFieldToBoolean : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="BooleanField"/> to a <see cref="Boolean"/>.
		/// </summary>
		public BooleanFieldToBoolean()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(BooleanField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Boolean);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is BooleanField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a BooleanField.", nameof(Value));
		}
	}
}
