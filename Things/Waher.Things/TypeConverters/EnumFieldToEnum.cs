using System;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="EnumField"/> to a <see cref="Enum"/>.
	/// </summary>
	public class EnumFieldToEnum : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="EnumField"/> to a <see cref="Enum"/>.
		/// </summary>
		public EnumFieldToEnum()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(EnumField);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(Enum);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is EnumField Field)
				return Field.Value;
			else
				throw new ArgumentException("Not a EnumField.", nameof(Value));
		}
	}
}
