using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Script Parser extensions
	/// </summary>
	public static class ParserExtensions
	{
		/// <summary>
		/// Parses script until first whitespace character is found.
		/// </summary>
		/// <param name="Parser">Parser</param>
		/// <returns>Script node.</returns>
		public static ScriptNode ParseNoWhiteSpace(this ScriptParser Parser)
		{
			bool Bak = Parser.CanSkipWhitespace;
			if (Bak)
				Parser.SkipWhiteSpace();

			Parser.CanSkipWhitespace = false;
			ScriptNode Result = Parser.ParseComparison();
			Parser.CanSkipWhitespace = Bak;

			return Result;
		}
	}
}
