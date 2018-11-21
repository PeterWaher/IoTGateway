using System;
using System.Collections.Generic;
using Waher.Script.Model;
using Waher.Script.Objects;

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
		public string[] InternalKeywords => new string[] { "FROM", "WHERE" };

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
				if (s != "FROM")
					return false;

				ScriptNode Source = Parser.ParseNoWhiteSpace();
				ScriptNode Where = null;

				s = Parser.PeekNextToken().ToUpper();
				if (s == "WHERE")
				{
					Parser.NextToken();
					Where = Parser.ParseOrs();
				}

				Result = new Delete(Source, Where, Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
