using System;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses a DROP statement
	/// </summary>
	public class DropParser : IKeyWord
	{
		/// <summary>
		/// Parses a DROP statement
		/// </summary>
		public DropParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "DROP";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "INDEX", "ON" };

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
				switch (s)
				{
					case "INDEX":
						ScriptNode Name = Parser.ParseNoWhiteSpace();

						s = Parser.NextToken().ToUpper();
						if (s != "ON")
							return false;

						if (!SelectParser.TryParseSources(Parser, out SourceDefinition Source))
							return false;

						Result = new DropIndex(Name, Source, Parser.Start, Parser.Length, Parser.Expression);

						return true;

					case "TABLE":
					case "COLLECTION":
						if (!SelectParser.TryParseSources(Parser, out Source))
							return false;

						Result = new DropCollection(Source, Parser.Start, Parser.Length, Parser.Expression);

						return true;

					default:
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
