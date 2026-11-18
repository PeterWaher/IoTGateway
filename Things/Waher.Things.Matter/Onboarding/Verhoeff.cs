namespace Waher.Things.Matter.Onboarding
{
	/// <summary>
	/// Verhoeff check digit algorithm (base 10), used by Matter manual pairing codes.
	/// </summary>
	public static class Verhoeff
	{
		private static readonly byte[,] d = new byte[,]
		{
			{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
			{ 1, 2, 3, 4, 0, 6, 7, 8, 9, 5 },
			{ 2, 3, 4, 0, 1, 7, 8, 9, 5, 6 },
			{ 3, 4, 0, 1, 2, 8, 9, 5, 6, 7 },
			{ 4, 0, 1, 2, 3, 9, 5, 6, 7, 8 },
			{ 5, 9, 8, 7, 6, 0, 4, 3, 2, 1 },
			{ 6, 5, 9, 8, 7, 1, 0, 4, 3, 2 },
			{ 7, 6, 5, 9, 8, 2, 1, 0, 4, 3 },
			{ 8, 7, 6, 5, 9, 3, 2, 1, 0, 4 },
			{ 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }
		};

		private static readonly byte[,] p = new byte[,]
		{
			{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
			{ 1, 5, 7, 6, 2, 8, 3, 0, 9, 4 },
			{ 5, 8, 0, 3, 7, 9, 6, 1, 4, 2 },
			{ 8, 9, 1, 6, 0, 4, 3, 5, 2, 7 },
			{ 9, 4, 5, 3, 1, 2, 6, 8, 7, 0 },
			{ 4, 2, 8, 6, 5, 7, 3, 9, 0, 1 },
			{ 2, 7, 9, 3, 8, 0, 6, 4, 1, 5 },
			{ 7, 0, 4, 6, 9, 1, 3, 2, 5, 8 }
		};

		private static readonly byte[] inv = new byte[] { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

		/// <summary>
		/// Computes the Verhoeff check digit of a string of decimal digits.
		/// </summary>
		/// <param name="Digits">Decimal digits.</param>
		/// <returns>Check digit.</returns>
		public static char ComputeCheckDigit(string Digits)
		{
			int c = 0;
			int i, n = Digits.Length;

			for (i = 0; i < n; i++)
			{
				int Digit = Digits[n - 1 - i] - '0';
				if (Digit < 0 || Digit > 9)
					throw new MatterDecodingException("Not a decimal digit.");

				c = d[c, p[(i + 1) % 8, Digit]];
			}

			return (char)('0' + inv[c]);
		}

		/// <summary>
		/// Validates a string of decimal digits ending with a Verhoeff check digit.
		/// </summary>
		/// <param name="DigitsWithCheck">Digits, including check digit.</param>
		/// <returns>If the check digit is valid.</returns>
		public static bool Validate(string DigitsWithCheck)
		{
			if (string.IsNullOrEmpty(DigitsWithCheck))
				return false;

			return ComputeCheckDigit(DigitsWithCheck.Substring(0, DigitsWithCheck.Length - 1)) == DigitsWithCheck[DigitsWithCheck.Length - 1];
		}
	}
}
