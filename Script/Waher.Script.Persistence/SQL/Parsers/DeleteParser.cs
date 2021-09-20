using System;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses a DELETE statement
	/// </summary>
	public class DeleteParser : IKeyWord
	{
		/// <summary>
		/// Parses a DELETE statement
		/// </summary>
		public DeleteParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "DELETE";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "LAZY", "FROM", "WHERE" };

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			Result = null;

			try
			{
				string s = Parser.NextToken().ToUpper();
				bool Lazy = s == "LAZY";

				if (Lazy)
					s = Parser.NextToken().ToUpper();

				if (s != "FROM")
					return false;

				if (!SelectParser.TryParseSources(Parser, out SourceDefinition Source))
					return false;

				ScriptNode Where = null;

				s = Parser.PeekNextToken().ToUpper();
				if (s == "WHERE")
				{
					Parser.NextToken();
					Where = Parser.ParseOrs();
				}

				Result = new Delete(Source, Where, Lazy, Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
