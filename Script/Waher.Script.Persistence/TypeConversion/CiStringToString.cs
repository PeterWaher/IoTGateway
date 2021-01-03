using System;
using Waher.Persistence;
using Waher.Script.TypeConversion;

namespace Waher.Script.Persistence.TypeConversion
{
	/// <summary>
	/// Converts case-insensitive strings to normal strings.
	/// </summary>
	public class CiStringToString : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(CaseInsensitiveString);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(string);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (!(Value is CaseInsensitiveString CiString))
				throw new ArgumentException("Expected case-insensitive string.", nameof(Value));

			return CiString.Value;
		}
	}
}
