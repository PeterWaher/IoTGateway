using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Contains static functions used by different algorithms.
	/// </summary>
	public static class Primitives
	{
		/// <summary>
		/// Concatenates a series of octet strings.
		/// </summary>
		/// <param name="OctetStrings">Octet strings to concatenate.</param>
		/// <returns>Concatenated octet string.</returns>
		public static byte[] CONCAT(params byte[][] OctetStrings)
		{
			int c = 0;

			foreach (byte[] A in OctetStrings)
				c += A.Length;

			byte[] Result = new byte[c];
			int i = 0;

			foreach (byte[] A in OctetStrings)
			{
				c = A.Length;
				Array.Copy(A, 0, Result, i, c);
				i += c;
			}

			return Result;
		}
	}
}
