﻿namespace Waher.Security.SHA3
{
	/// <summary>
	/// Implements the SHA3 SHAKE256 extendable-output functions, as defined in 
	/// section 6.2 in the NIST FIPS 202: 
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
	/// </summary>
	public class SHAKE256 : Keccak1600
	{
        /// <summary>
        /// Implements the SHA3 SHAKE256 extendable-output functions, as defined in 
        /// section 6.2 in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="DigestSize">Digest size, in bits.</param>
        public SHAKE256(int DigestSize)
			: base(512, 0b1111, 4, DigestSize)
		{
		}
	}
}
