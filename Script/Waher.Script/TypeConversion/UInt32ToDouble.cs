﻿using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts uint numbers to double numbers.
	/// </summary>
	public class UInt32ToDouble : ITypeConverter
	{
		/// <summary>
		/// Converts double numbers to uint numbers.
		/// </summary>
		public Type From => typeof(uint);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(double);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is uint d)
				return (double)d;
			else
				throw new ArgumentException("Expected uint value.", nameof(Value));
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
			if (Value is uint d)
				return new ObjectValue((double)d);
			else
				throw new ArgumentException("Expected uint value.", nameof(Value));
		}
	}
}
