using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="StringField"/> to a <see cref="String"/>.
	/// </summary>
	public class StringFieldToString : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="StringField"/> to a <see cref="String"/>.
		/// </summary>
		public StringFieldToString()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(StringField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(String);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is StringField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a StringField.", nameof(Value));
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
			if (Value is StringField Field)
				return new StringValue(Field.Value);
			else
				throw new ArgumentException("Not a StringField.", nameof(Value));
		}
	}
}
