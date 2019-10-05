using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Exceptions
{
	/// <summary>
	/// ASN.1 syntax exceptions.
	/// </summary>
	public class Asn1SyntaxException : Asn1Exception
	{
		private readonly string text;
		private readonly int pos;

		/// <summary>
		/// ASN.1 syntax exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Text">ASN.1 text</param>
		/// <param name="Position">Position where exception was raised.</param>
		public Asn1SyntaxException(string Message, string Text, int Position)
			: base(Format(Message, Text, Position))
		{
			this.text = Text;
			this.pos = Position;
		}

		/// <summary>
		/// ASN.1 text
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Position into text where exception was raised.
		/// </summary>
		public int Position => this.pos;

		private static string Format(string Message, string Text, int Position)
		{
			int Pos = Position - 100;
			int End = Pos + 200;
			int NewPos = 100;

			if (Pos < 0)
			{
				NewPos += Pos;
				Pos = 0;
			}

			if (End > Text.Length)
				End = Text.Length;

			string s = Text.Substring(Pos, End - Pos);
			s = s.Insert(NewPos, "^^^-------- " + Message + " -----");

			return Message + "\r\n\r\n" + s;
		}
	}
}
