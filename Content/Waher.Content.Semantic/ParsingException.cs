using System;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Parsing exception
	/// </summary>
	public class ParsingException : Exception
	{
		/// <summary>
		/// Parsing exception
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Document">Document being parsed.</param>
		/// <param name="Position">Position into document where error was encountered.</param>
		public ParsingException(string Message, string Document, int Position)
			: base(Append(Message, Position, Document))
		{
			this.Document = Document;
			this.Position = Position;
		}

		/// <summary>
		/// Parsing exception
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Document">Document being parsed.</param>
		/// <param name="Position">Position into document where error was encountered.</param>
		/// <param name="InnerException">Inner exception.</param>
		public ParsingException(string Message, string Document, int Position, Exception InnerException)
			: base(Append(Message, Position, Document), InnerException)
		{
			this.Document = Document;
			this.Position = Position;
		}

		/// <summary>
		/// Document being parsed.
		/// </summary>
		public string Document { get; }

		/// <summary>
		/// Position into document where error was encountered.
		/// </summary>
		public int Position { get; }

		private static string Append(string Message, int Position, string Document)
		{
			Message += "\r\n\r\n";

			int Start = Math.Max(0, Position - 100);
			int End = Math.Min(Document.Length, Position + 100);

			Message += Document.Substring(Start, Position - Start) + "^-------------" + Document.Substring(Position, End - Position);

			return Message;
		}
	}
}
