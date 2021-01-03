using System;
using Waher.Persistence;
using Waher.Script.TypeConversion;

namespace Waher.Script.Persistence.TypeConversion
{
	/// <summary>
	/// Converts normal strings to case-insensitive strings.
	/// </summary>
	public class StringToCiString : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(string);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(CaseInsensitiveString);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (!(Value is string String))
				throw new ArgumentException("Expected string.", nameof(Value));

			return new CaseInsensitiveString(String);
		}
	}
}
