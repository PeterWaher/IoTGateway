using System;
using Waher.Content;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="DurationField"/> to a <see cref="Duration"/>.
	/// </summary>
	public class DurationFieldToDuration : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="DurationField"/> to a <see cref="Duration"/>.
		/// </summary>
		public DurationFieldToDuration()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(DurationField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Duration);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is DurationField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a DurationField.", nameof(Value));
		}
	}
}
