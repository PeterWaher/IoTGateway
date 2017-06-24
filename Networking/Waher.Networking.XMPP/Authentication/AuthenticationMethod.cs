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
		/// Checks if the authentication made by the server is valid.
		/// </summary>
		/// <param name="Success">Success text.</param>
		/// <param name="Client">XMPP Client</param>
		/// <returns>If the authentication should be accepted.</returns>
		public abstract bool CheckSuccess(string Success, XmppClient Client);

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

		/// <summary>
		/// Concatenates binary blocks of data.
		/// </summary>
		/// <param name="Data">Binary blocks of data.</param>
		/// <returns>Concatenated binary block of data.</returns>
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

		/// <summary>
		/// Concatenates an array of strings.
		/// </summary>
		/// <param name="Parameters">String parameters to concatenate.</param>
		/// <returns>Concatenated string.</returns>
		protected static string CONCAT(params string[] Parameters)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string s in Parameters)
				sb.Append(s);

			return sb.ToString();
		}

		/// <summary>
		/// Concatenates a binary block of data, with an array of strings. UTF-8 encoding will be used to encode the strings.
		/// </summary>
		/// <param name="Data">Binary block of data.</param>
		/// <param name="Parameters">Strings.</param>
		/// <returns>Concatenaton.</returns>
		protected static byte[] CONCAT(byte[] Data, params string[] Parameters)
		{
			return CONCAT(Data, Encoding.UTF8.GetBytes(CONCAT(Parameters)));
		}

		/// <summary>
		/// Creates a lowercase hexadecimal string from a binary block of data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hexadecimal string.</returns>
		protected static string HEX(byte[] Data)
		{
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Data)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		/// <summary>
		/// XORs elements of two binary blocks of data of equal size.
		/// </summary>
		/// <param name="U1">Binary block 1.</param>
		/// <param name="U2">Binary block 2.</param>
		/// <returns>XOR(U1,U2)</returns>
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
