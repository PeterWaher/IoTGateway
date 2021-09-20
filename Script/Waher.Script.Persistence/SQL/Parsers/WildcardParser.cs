using System;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses the Wildcard symbol *
	/// </summary>
	public class WildcardParser : IKeyWord
	{
		/// <summary>
		/// Parses the Wildcard symbol *
		/// </summary>
		public WildcardParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "*";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[0];

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			Result = new Wildcard(Parser.Start, Parser.Length, Parser.Expression);
			return true;
		}

	}
}
