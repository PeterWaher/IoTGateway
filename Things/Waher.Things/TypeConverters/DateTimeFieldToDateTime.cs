using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="DateTimeField"/> to a <see cref="DateTime"/>.
	/// </summary>
	public class DateTimeFieldToDateTime : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="DateTimeField"/> to a <see cref="DateTime"/>.
		/// </summary>
		public DateTimeFieldToDateTime()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(DateTimeField);

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
			if (Value is DateTimeField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a DateTimeField.", nameof(Value));
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
			if (Value is DateTimeField Field)
				return new DateTimeValue(Field.Value);
			else
				throw new ArgumentException("Not a DateTimeField.", nameof(Value));
		}
	}
}
