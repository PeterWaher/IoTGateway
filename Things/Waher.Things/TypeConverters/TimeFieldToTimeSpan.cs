using System;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="TimeField"/> to a <see cref="TimeSpan"/>.
	/// </summary>
	public class TimeFieldToTimeSpan : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="TimeField"/> to a <see cref="TimeSpan"/>.
		/// </summary>
		public TimeFieldToTimeSpan()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(TimeField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(TimeSpan);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is TimeField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a TimeField.", nameof(Value));
		}
	}
}
