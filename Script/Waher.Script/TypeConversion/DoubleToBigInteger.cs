using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts a double number to a <see cref="BigInteger"/>, if possible.
	/// </summary>
	public class DoubleToBigInteger : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(double);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(BigInteger);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is double d)
			{
				BigInteger i = (BigInteger)d;
				if (((double)i) == d)
					return i;
				else
					return null;
			}
			else
				throw new ArgumentException("Expected BigInteger value.", nameof(Value));
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
			if (Value is double d)
			{
				BigInteger i = (BigInteger)d;
				if (((double)i) == d)
					return new Integer(i);
				else
					return null;
			}
			else
				throw new ArgumentException("Expected double value.", nameof(Value));
		}
	}
}
