using System.Text;

namespace Waher.Content.Asn1.Exceptions
{
	/// <summary>
	/// ASN.1 syntax exceptions.
	/// </summary>
	public class Asn1SyntaxException : Asn1Exception
	{
		private readonly string text;
		private readonly string module;
		private readonly int pos;
		private readonly int row;
		private readonly int column;

		/// <summary>
		/// ASN.1 syntax exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Text">ASN.1 text</param>
		/// <param name="Position">Position where exception was raised.</param>
		/// <param name="Row">Row number</param>
		/// <param name="Column">Column number</param>
		/// <param name="Module">Module where error was found</param>
		public Asn1SyntaxException(string Message, string Text, int Position,
			int Row, int Column, string Module)
			: base(Format(Message, Text, Position, Row, Column, Module))
		{
			this.text = Text;
			this.pos = Position;
			this.row = Row;
			this.column = Column;
			this.module = Module;
		}

		/// <summary>
		/// ASN.1 text
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Position into text where exception was raised.
		/// </summary>
		public int Position => this.pos;

		/// <summary>
		/// Row number
		/// </summary>
		public int Row => this.row;

		/// <summary>
		/// Column number
		/// </summary>
		public int Column => this.column;

		/// <summary>
		/// Module where error was found
		/// </summary>
		public string Module => this.module;

		private static string Format(string Message, string Text, int Position,
			int Row, int Column, string Module)
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

			StringBuilder sb = new StringBuilder();

			sb.Append(Message);
			sb.Append(" (");
			sb.Append(Module);
			sb.Append(", row ");
			sb.Append(Row.ToString());
			sb.Append(", column ");
			sb.Append(Column.ToString());
			sb.Append(')');
			sb.AppendLine();
			sb.AppendLine();
			sb.Append(s);

			return sb.ToString();
		}
	}
}
