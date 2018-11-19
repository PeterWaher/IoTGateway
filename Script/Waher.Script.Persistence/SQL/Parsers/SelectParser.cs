using System;
using System.Collections.Generic;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses a SELECT statement
	/// </summary>
	public class SelectParser : IKeyWord
	{
		/// <summary>
		/// Parses a SELECT statement
		/// </summary>
		public SelectParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "SELECT";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "FROM", "WHERE", "GROUP", "BY", "HAVING", "ORDER", "TOP", "OFFSET", "ASC", "DESC" };

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
				List<ScriptNode> Columns;
				List<ScriptNode> ColumnNames;
				ScriptNode Top;
				string s;

				s = Parser.PeekNextToken().ToUpper();
				if (s == string.Empty)
					return false;

				if (s == "TOP")
				{
					Parser.NextToken();
					Top = Parser.ParseNoWhiteSpace();

					s = Parser.PeekNextToken();
					if (s == string.Empty)
						return false;
				}
				else
					Top = null;

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

					while (true)
					{
						ScriptNode Node = Parser.ParseNoWhiteSpace();
						ScriptNode Name = null;

						Parser.SkipWhiteSpace();

						s = Parser.PeekNextToken().ToUpper();
						if (!string.IsNullOrEmpty(s) && s != "," && s != "FROM")
						{
							Name = Parser.ParseNoWhiteSpace();
							s = Parser.PeekNextToken();
						}
						else if (Node is VariableReference Ref)
							Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);

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

				List<ScriptNode> Sources = new List<ScriptNode>();
				List<ScriptNode> SourceNames = new List<ScriptNode>();

				while (true)
				{
					ScriptNode Node = Parser.ParseNoWhiteSpace();
					ScriptNode Name = null;

					Parser.SkipWhiteSpace();

					s = Parser.PeekNextToken().ToUpper();
					if (!string.IsNullOrEmpty(s) && s != "," && s != "WHERE" && s != "GROUP" && s != "ORDER" && s != "OFFSET")
					{
						Name = Parser.ParseNoWhiteSpace();
						s = Parser.PeekNextToken().ToUpper();
					}
					else if (Node is VariableReference Ref)
						Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);

					Sources.Add(Node);
					SourceNames.Add(Name);

					if (s != ",")
						break;

					Parser.NextToken();
				}

				ScriptNode Where = null;

				if (s == "WHERE")
				{
					Parser.NextToken();
					Where = Parser.ParseOrs();
					s = Parser.PeekNextToken().ToUpper();
				}

				List<ScriptNode> GroupBy = null;
				List<ScriptNode> GroupByNames = null;
				ScriptNode Having = null;

				if (s == "GROUP")
				{
					Parser.NextToken();
					if (Parser.NextToken().ToUpper() != "BY")
						return false;

					GroupBy = new List<ScriptNode>();
					GroupByNames = new List<ScriptNode>();

					while (true)
					{
						ScriptNode Node = Parser.ParseNoWhiteSpace();
						ScriptNode Name = null;

						Parser.SkipWhiteSpace();

						s = Parser.PeekNextToken().ToUpper();
						if (!string.IsNullOrEmpty(s) && s != "," && s != "HAVING" && s != "ORDER" && s != "OFFSET")
						{
							Name = Parser.ParseNoWhiteSpace();
							s = Parser.PeekNextToken().ToUpper();
						}
						else if (Node is VariableReference Ref)
							Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);

						GroupBy.Add(Node);
						GroupByNames.Add(Name);

						if (s != ",")
							break;

						Parser.NextToken();
					}

					if (s == "HAVING")
					{
						Parser.NextToken();
						Having = Parser.ParseOrs();
						s = Parser.PeekNextToken().ToUpper();
					}
				}

				List<KeyValuePair<ScriptNode, bool>> OrderBy = null;

				if (s == "ORDER")
				{
					Parser.NextToken();
					if (Parser.NextToken().ToUpper() != "BY")
						return false;

					OrderBy = new List<KeyValuePair<ScriptNode, bool>>();

					while (true)
					{
						ScriptNode Node = Parser.ParseNoWhiteSpace();

						s = Parser.PeekNextToken().ToUpper();
						if (s == "ASC")
						{
							Parser.NextToken();
							OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Node, true));
							s = Parser.PeekNextToken().ToUpper();
						}
						else if (s == "DESC")
						{
							Parser.NextToken();
							OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Node, false));
							s = Parser.PeekNextToken().ToUpper();
						}
						else
							OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Node, true));

						if (s != ",")
							break;

						Parser.NextToken();
					}
				}

				ScriptNode Offset = null;

				if (s == "OFFSET")
				{
					Parser.NextToken();
					Offset = Parser.ParseNoWhiteSpace();
				}

				Result = new Select(Columns?.ToArray(), ColumnNames?.ToArray(), Sources.ToArray(), SourceNames.ToArray(),
					Where, GroupBy?.ToArray(), GroupByNames?.ToArray(), Having, OrderBy?.ToArray(), Top, Offset,
					Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
