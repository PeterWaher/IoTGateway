using System;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="DateField"/> to a <see cref="DateTime"/>.
	/// </summary>
	public class DateFieldToDate : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="DateField"/> to a <see cref="DateTime"/>.
		/// </summary>
		public DateFieldToDate()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(DateField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(DateTime);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is DateField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a DateField.", nameof(Value));
		}
	}
}
