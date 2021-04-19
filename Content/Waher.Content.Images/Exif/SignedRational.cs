using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Represents an EXIF SRATIONAL value.
	/// </summary>
	public class SignedRational
	{
		private readonly int numerator;
		private readonly int denominator;

		/// <summary>
		/// Represents an EXIF RATIONAL value.
		/// </summary>
		/// <param name="Numerator">Numerator of Tag Value.</param>
		/// <param name="Denominator">Denominator of Tag Value.</param>
		public SignedRational(int Numerator, int Denominator)
		{
			this.numerator = Numerator;
			this.denominator = Denominator;
		}

		/// <summary>
		/// Typed EXIF Tag Value Numerator
		/// </summary>
		public int Numerator => this.numerator;

		/// <summary>
		/// Typed EXIF Tag Value Denominator
		/// </summary>
		public int Denominator => this.denominator;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.numerator.ToString() + "/" + this.denominator.ToString();
		}
	}
}
