using Waher.Script.Model;
using Waher.Script.Operators;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Ledger operation to record.
	/// </summary>
	public enum RecordOperation
	{
		/// <summary>
		/// New entry
		/// </summary>
		New,

		/// <summary>
		/// Update entry
		/// </summary>
		Update,

		/// <summary>
		/// Delete entry
		/// </summary>
		Delete
	}

	/// <summary>
	/// Parses a RECORD statement
	/// </summary>
	public class RecordParser : IKeyWord
	{
		/// <summary>
		/// Parses a RECORD statement
		/// </summary>
		public RecordParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "RECORD";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] 
		{ 
			"INTO", 
			"NEW", 
			"UPDATE",
			"DELETE",
			"OBJECT", 
			"OBJECTS" 
		};

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			Result = null;

			string s = Parser.NextToken().ToUpper();

			if (s != "INTO")
				return false;

			if (!SelectParser.TryParseSources(Parser, out SourceDefinition Source))
				return false;

			RecordOperation? Operation = null;

			while (true)
			{
				switch (Parser.PeekNextToken().ToUpper())
				{
					case "NEW":
						if (Operation.HasValue)
							return false;

						Parser.NextToken();
						Operation = RecordOperation.New;
						continue;

					case "UPDATE":
						if (Operation.HasValue)
							return false;

						Parser.NextToken();
						Operation = RecordOperation.Update;
						continue;

					case "DELETE":
						if (Operation.HasValue)
							return false;

						Parser.NextToken();
						Operation = RecordOperation.Delete;
						continue;

					case "OBJECT":
					case "OBJECTS":
						Parser.NextToken();

						ScriptNode Node = Parser.ParseList();
						if (!(Node is ElementList Objects))
							Objects = new ElementList(new ScriptNode[] { Node }, Node.Start, Node.Length, Node.Expression);

						Result = new RecordObjects(Source, Operation ?? RecordOperation.New,
							Objects, Parser.Start, Parser.Length, Parser.Expression);
						return true;

					default:
						return false;
				}
			}
		}

	}
}
