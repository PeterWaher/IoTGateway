using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts an object of one type to an object of another type.
	/// </summary>
	public interface ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		Type From
		{
			get;
		}

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		Type To
		{
			get;
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		object Convert(object Value);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>, encapsulated in an <see cref="IElement"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		IElement ConvertToElement(object Value);
	}
}
