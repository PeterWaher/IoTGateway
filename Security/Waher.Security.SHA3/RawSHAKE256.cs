using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.SHA3
{
	/// <summary>
	/// Implements the SHA3 RawSHAKE256 extendable-output functions, as defined in 
	/// section 6.3 in the NIST FIPS 202: 
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
	/// </summary>
	public class RawSHAKE256 : Keccak1600
	{
		/// <summary>
		/// Implements the SHA3 RawSHAKE256 extendable-output functions, as defined in 
		/// section 6.3 in the NIST FIPS 202: 
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
		/// </summary>
		/// <param name="DigestSize">Digest size, in bits.</param>
		public RawSHAKE256(int DigestSize)
			: base(256, 0b11, DigestSize)
		{
		}
	}
}
