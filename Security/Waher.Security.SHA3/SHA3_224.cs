﻿namespace Waher.Security.SHA3
{
	/// <summary>
	/// Implements the SHA3-224 hash function, as defined in section 6.1
	/// in the NIST FIPS 202: 
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
	/// </summary>
	public class SHA3_224 : Keccak1600
	{
		/// <summary>
		/// Implements the SHA3-224 hash function, as defined in section 6.1
		/// in the NIST FIPS 202: 
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
		/// </summary>
		public SHA3_224()
			: base(448, 0b10, 224)
		{
		}
	}
}
