using System;
using System.Collections.Generic;
using Waher.Script.Model;
using Waher.Script.Operators;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Comparisons;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses an INSERT statement
	/// </summary>
	public class InsertParser : IKeyWord
	{
		/// <summary>
		/// Parses an INSERT statement
		/// </summary>
		public InsertParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "INSERT";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "INTO", "VALUES" };

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
				if (s != "INTO")
					return false;

				ScriptNode Source = Parser.ParseNoWhiteSpace();

				if (Parser.NextToken() != "(")
					return false;

				ScriptNode Node = Parser.ParseList();
				if (!(Node is ElementList Columns))
					Columns = new ElementList(new ScriptNode[] { Node }, Node.Start, Node.Length, Node.Expression);

				if (Parser.NextToken() != ")")
					return false;

				if (Parser.NextToken().ToUpper() != "VALUES")
					return false;

				if (Parser.NextToken() != "(")
					return false;

				Node = Parser.ParseList();
				if (!(Node is ElementList Values))
					Values = new ElementList(new ScriptNode[] { Node }, Node.Start, Node.Length, Node.Expression);

				if (Values.Elements.Length != Columns.Elements.Length)
					return false;

				if (Parser.NextToken() != ")")
					return false;

				Result = new Insert(Source, Columns, Values, Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
