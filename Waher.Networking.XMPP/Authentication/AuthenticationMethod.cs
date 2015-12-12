using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Base class for all authentication methods.
	/// </summary>
	public abstract class AuthenticationMethod
	{
		/// <summary>
		/// Base class for all authentication methods.
		/// </summary>
		public AuthenticationMethod()
		{
		}

		/// <summary>
		/// Computes a response given a challenge from the server.
		/// </summary>
		/// <param name="Challenge">Challenge text.</param>
		/// <param name="Client">XMPP Client</param>
		/// <returns>Response text.</returns>
		public abstract string Challenge(string Challenge, XmppClient Client);

		/// <summary>
		/// Parses a parameter list in a challenge string.
		/// </summary>
		/// <param name="s">Encoded parameter list.</param>
		/// <returns>Parsed parameters.</returns>
		protected KeyValuePair<string, string>[] ParseCommaSeparatedParameterList(string s)
		{
			List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();
			StringBuilder sb = new StringBuilder();
			string Key = string.Empty;
			int State = 0;

			foreach (char ch in s)
			{
				switch (State)
				{
					case 0:		// ID
						if (ch == '=')
						{
							Key = sb.ToString();
							sb.Clear();
							State++;
						}
						else if (ch == ',')
						{
							Result.Add(new KeyValuePair<string, string>(sb.ToString(), string.Empty));
							sb.Clear();
						}
						else
							sb.Append(ch);
						break;

					case 1:	// Value, first character
						if (ch == '"')
							State += 2;
						else if (ch == ',')
						{
							Result.Add(new KeyValuePair<string, string>(Key, string.Empty));
							sb.Clear();
							State = 0;
							Key = string.Empty;
						}
						else
						{
							sb.Append(ch);
							State++;
						}
						break;

					case 2:	// Value, following characters
						if (ch == ',')
						{
							Result.Add(new KeyValuePair<string, string>(Key, sb.ToString()));
							sb.Clear();
							State = 0;
							Key = string.Empty;
						}
						else
							sb.Append(ch);
						break;

					case 3:	// Value, between quotes
						if (ch == '"')
							State--;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 4:	// Escaped character
						sb.Append(ch);
						State--;
						break;
				}
			}

			if (State == 2 && !string.IsNullOrEmpty(Key))
				Result.Add(new KeyValuePair<string, string>(Key, sb.ToString()));

			return Result.ToArray();
		}

		protected static byte[] CONCAT(params byte[][] Data)
		{
			int c = 0;

			foreach (byte[] Part in Data)
				c += Part.Length;

			int i = 0;
			int j;
			byte[] Result = new byte[c];

			foreach (byte[] Part in Data)
			{
				j = Part.Length;
				Array.Copy(Part, 0, Result, i, j);
				i += j;
			}

			return Result;
		}

		protected static string CONCAT(params string[] Parameters)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string s in Parameters)
				sb.Append(s);

			return sb.ToString();
		}

		protected static byte[] CONCAT(byte[] Data, params string[] Parameters)
		{
			return CONCAT(Data, System.Text.Encoding.UTF8.GetBytes(CONCAT(Parameters)));
		}

		protected static string HEX(byte[] Data)
		{
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Data)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		protected static byte[] XOR(byte[] U1, byte[] U2)
		{
			int i, c = U1.Length;
			if (U2.Length != c)
				throw new Exception("Arrays must be of the same size.");

			byte[] Response = new byte[c];

			for (i = 0; i < c; i++)
				Response[i] = (byte)(U1[i] ^ U2[i]);

			return Response;
		}

	}
}
