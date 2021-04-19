using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Represents an EXIF RATIONAL value.
	/// </summary>
	public class Rational
	{
		private readonly uint numerator;
		private readonly uint denominator;

		/// <summary>
		/// Represents an EXIF RATIONAL value.
		/// </summary>
		/// <param name="Numerator">Numerator of Tag Value.</param>
		/// <param name="Denominator">Denominator of Tag Value.</param>
		public Rational(uint Numerator, uint Denominator)
		{
			this.numerator = Numerator;
			this.denominator = Denominator;
		}

		/// <summary>
		/// Typed EXIF Tag Value Numerator
		/// </summary>
		public uint Numerator => this.numerator;

		/// <summary>
		/// Typed EXIF Tag Value Denominator
		/// </summary>
		public uint Denominator => this.denominator;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.numerator.ToString() + "/" + this.denominator.ToString();
		}
	}
}
