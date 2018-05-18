using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content
{
	/// <summary>
	/// Static class that does BASE64URL encoding (using URL and filename safe alphabet), 
	/// as defined in RFC4648:
	/// https://tools.ietf.org/html/rfc4648#section-5
	/// </summary>
	public static class Base64Url
    {
		/// <summary>
		/// Converts a Base64URL-encoded string to its binary representation.
		/// </summary>
		/// <param name="Base64Url">Base64URL-encoded string.</param>
		/// <returns>Binary representation.</returns>
		public static byte[] Decode(string Base64Url)
		{
			int c = Base64Url.Length;
			int i = c & 3;

			Base64Url = Base64Url.Replace('-', '+'); // 62nd char of encoding
			Base64Url = Base64Url.Replace('_', '/'); // 63rd char of encoding

			switch (i)
			{
				case 1:
					Base64Url += "A==";
					break;

				case 2:
					Base64Url += "==";
					break;

				case 3:
					Base64Url += "=";
					break;
			}

			return Convert.FromBase64String(Base64Url);
		}

		/// <summary>
		/// Converts a binary block of data to a Base64URL-encoded string.
		/// </summary>
		/// <param name="Data">Data to encode.</param>
		/// <returns>Base64URL-encoded string.</returns>
		public static string Encode(byte[] Data)
		{
			string s = Convert.ToBase64String(Data);
			int c = Data.Length;
			int i = c % 3;

			if (i == 1)
				s = s.Substring(0, s.Length - 2);
			else if (i == 2)
				s = s.Substring(0, s.Length - 1);

			s = s.Replace('+', '-'); // 62nd char of encoding
			s = s.Replace('/', '_'); // 63rd char of encoding

			return s;
		}
	}
}
