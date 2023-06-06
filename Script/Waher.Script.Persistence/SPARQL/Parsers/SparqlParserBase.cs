using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Parsers
{
	/// <summary>
	/// Parses a SPARQL statement that begins with the BASE keyword.
	/// </summary>
	public class SparqlParserBase : IKeyWord
	{
		/// <summary>
		/// Parses a SPARQL statement that begins with the BASE keyword.
		/// </summary>
		public SparqlParserBase()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "BASE";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => SparqlParser.RefInstance.InternalKeywords;

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			SparqlParser SparqlParser = new SparqlParser("BASE ");
			return SparqlParser.TryParse(Parser, out Result);
		}
	}
}