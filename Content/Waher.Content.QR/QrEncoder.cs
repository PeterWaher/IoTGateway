using System;

namespace Waher.Content.QR
{
	/// <summary>
	/// QR Code encoder.
	/// </summary>
	public class QrEncoder : IDisposable
	{
		/// <summary>
		/// QR Code encoder.
		/// </summary>
		public QrEncoder()
		{
		}

		/// <summary>
		/// QR Code encoder.
		/// </summary>
		public void Dispose()
		{
		}

		private int BitLengthCharacterCount(byte Version, EncodingMode Mode)
		{
			switch (Mode)
			{
				case EncodingMode.Numeric:
					if (Version < 10)
						return 10;
					else if (Version < 27)
						return 12;
					else if (Version < 41)
						return 14;
					break;

				case EncodingMode.Alphanumeric:
					if (Version < 10)
						return 9;
					else if (Version < 27)
						return 11;
					else if (Version < 41)
						return 13;
					break;

				case EncodingMode.Byte:
					if (Version < 10)
						return 8;
					else if (Version < 27)
						return 16;	// TODO: ??? or 12?
					else if (Version < 41)
						return 16;
					break;

				case EncodingMode.Kanji:
					if (Version < 10)
						return 8;
					else if (Version < 27)
						return 10;
					else if (Version < 41)
						return 12;
					break;
			}

			throw new NotSupportedException("Character Count bit length not supported.");	// TODO
		}


	}
}
