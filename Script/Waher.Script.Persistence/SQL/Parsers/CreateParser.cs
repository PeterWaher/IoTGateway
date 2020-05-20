using System;
using System.Collections.Generic;
using System.Linq;
using Waher.Script.Model;
using Waher.Script.Operators;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Comparisons;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses a CREATE statement
	/// </summary>
	public class CreateParser : IKeyWord
	{
		/// <summary>
		/// Parses a CREATE statement
		/// </summary>
		public CreateParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "CREATE";

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
				if (s != "INDEX")
					return false;

				ScriptNode Name = Parser.ParseNoWhiteSpace();

				s = Parser.NextToken().ToUpper();
				if (s != "ON")
					return false;

				if (!SelectParser.TryParseSources(Parser, out SourceDefinition Source))
					return false;

				s = Parser.NextToken();
				if (s != "(")
					return false;

				this.ParseList(Parser, out ScriptNode[] Columns, out bool[] Ascending);

				if (Columns.Length == 0)
					return false;

				if (Parser.NextToken() != ")")
					return false;

				Result = new CreateIndex(Name, Source, Columns, Ascending, Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal void ParseList(ScriptParser Parser, out ScriptNode[] Columns, out bool[] Ascending)
		{
			List<ScriptNode> ColumnList = new List<ScriptNode>();
			List<bool> AscendingList = new List<bool>();
			string s;

			do
			{
				ScriptNode Node = Parser.ParseIf();

				switch (s = Parser.PeekNextToken().ToUpper())
				{
					case "ASC":
						Parser.NextToken();
						ColumnList.Add(Node);
						AscendingList.Add(true);
						s = Parser.PeekNextToken();
						break;

					case "DESC":
						Parser.NextToken();
						ColumnList.Add(Node);
						AscendingList.Add(false);
						s = Parser.PeekNextToken();
						break;

					case ",":
						Parser.NextToken();
						ColumnList.Add(Node);
						AscendingList.Add(true);
						break;

					default:
						ColumnList.Add(Node);
						AscendingList.Add(true);
						break;
				}
			}
			while (s == ",");

			Columns = ColumnList.ToArray();
			Ascending = AscendingList.ToArray();
		}

	}
}
