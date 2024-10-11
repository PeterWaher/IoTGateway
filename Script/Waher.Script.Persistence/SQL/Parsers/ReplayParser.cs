using System.Collections.Generic;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses a REPLAY statement
	/// </summary>
	public class ReplayParser : IKeyWord
	{
		/// <summary>
		/// Parses a REPLAY statement
		/// </summary>
		public ReplayParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "REPLAY";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] 
		{ 
			"AS", 
			"FROM", 
			"WHERE", 
			"TOP", 
			"OFFSET",
			"TO",
			"XML",
			"JSON",
			"COUNTERS",
			"TABLE"
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

			List<ScriptNode> Columns;
			List<ScriptNode> ColumnNames;
			ScriptNode Top = null;
			string s;

			s = Parser.PeekNextToken().ToUpper();
			if (string.IsNullOrEmpty(s))
				return false;

			while (s == "TOP" || s == "DISTINCT" || s == "GENERIC")
			{
				switch (s)
				{
					case "TOP":
						Parser.NextToken();
						Top = Parser.ParseNoWhiteSpace();
						break;
				}

				s = Parser.PeekNextToken().ToUpper();
				if (string.IsNullOrEmpty(s))
					return false;
			}

			if (s == "*")
			{
				Parser.NextToken();
				Columns = null;
				ColumnNames = null;
			}
			else
			{
				Columns = new List<ScriptNode>();
				ColumnNames = new List<ScriptNode>();

				ScriptNode Node;
				ScriptNode Name;

				while (true)
				{
					Node = Parser.ParseNoWhiteSpace();

					Name = null;
					Parser.SkipWhiteSpace();

					s = Parser.PeekNextToken().ToUpper();
					if (!string.IsNullOrEmpty(s) && s != "," && s != "FROM")
					{
						if (s == "AS")
							Parser.NextToken();

						Name = Parser.ParseNoWhiteSpace();
						s = Parser.PeekNextToken();
					}
					else if (Node is VariableReference Ref)
						Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);
					else if (Node is NamedMember NamedMember)
						Name = new ConstantElement(new StringValue(NamedMember.Name), Node.Start, Node.Length, Node.Expression);

					Columns.Add(Node);
					ColumnNames.Add(Name);

					if (s != ",")
						break;

					Parser.NextToken();
				}
			}

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

				s = Parser.PeekNextToken().ToUpper();
			}

			ScriptNode Offset = null;

			if (s == "OFFSET")
			{
				Parser.NextToken();
				Offset = Parser.ParseNoWhiteSpace();
				s = Parser.PeekNextToken().ToUpper();
			}

			ScriptNode To = null;

			if (s == "TO")
			{
				Parser.NextToken();
				To = Parser.ParseNoWhiteSpace();
			}

			Result = new Replay(Columns?.ToArray(), ColumnNames?.ToArray(), Source, Where, 
				Top, Offset, To, Parser.Start, Parser.Length, Parser.Expression);

			return true;
		}

	}
}
