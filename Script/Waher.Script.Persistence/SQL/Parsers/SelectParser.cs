using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Functions.Vectors;
using Waher.Script.Operators.Membership;
using Waher.Script.Persistence.Functions;
using Waher.Script.Persistence.SQL.SourceDefinitions;

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
		public string[] InternalKeywords => new string[] { "AS", "FROM", "INNER", "OUTER", "LEFT", "RIGHT", "JOIN", "FULL", "WHERE", "GROUP", "BY", "HAVING", "ORDER", "TOP", "OFFSET", "ASC", "DESC", "DISTINCT", "GENERIC" };

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
				ScriptNode Top = null;
				string s;
				bool Distinct = false;
				bool Generic = false;

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

						case "DISTINCT":
							Parser.NextToken();
							Distinct = true;
							break;

						case "GENERIC":
							Parser.NextToken();
							Generic = true;
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
						if (s == "/" || s == "." || s == "@")
							Node = ParseXPath(Parser, true);
						else
						{
							Node = Parser.ParseNoWhiteSpace();

							if (Node is XPath XPath)
								XPath.ExtractValue = true;
						}

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
						s = Parser.PeekNextToken();
					}
				}

				s = Parser.NextToken().ToUpper();
				if (s != "FROM")
					return false;

				if (!TryParseSources(Parser, out SourceDefinition Source))
					return false;

				ScriptNode Where = null;

				s = Parser.PeekNextToken().ToUpper();
				if (s == "WHERE")
				{
					Parser.NextToken();

					s = Parser.PeekNextToken();
					if (s == "/" || s == "." || s == "@")
						Where = ParseXPath(Parser, false);
					else
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
						if (!string.IsNullOrEmpty(s) && s != "," && s != ";" && s != ")" && s != "]" && s != "}" && s != "HAVING" && s != "ORDER" && s != "OFFSET")
						{
							if (s == "AS")
								Parser.NextToken();

							Name = Parser.ParseNoWhiteSpace();
							s = Parser.PeekNextToken().ToUpper();
						}
						else
							Name = null;

						if (Name is null)
						{
							if (Node is VariableReference Ref)
								Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);
							else if (Node is NamedMember NamedMember)
								Name = new ConstantElement(new StringValue(NamedMember.Name), Node.Start, Node.Length, Node.Expression);
						}

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
				else if (!(Columns is null))
				{
					bool ImplicitGrouping = false;

					foreach (ScriptNode Column in Columns)
					{
						if (this.ContainsVectorFunction(Column))
						{
							ImplicitGrouping = true;
							break;
						}
					}

					if (ImplicitGrouping)
					{
						GroupBy = new List<ScriptNode>();
						GroupByNames = new List<ScriptNode>();
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

				Result = new Select(Columns?.ToArray(), ColumnNames?.ToArray(), Source, Where, GroupBy?.ToArray(),
					GroupByNames?.ToArray(), Having, OrderBy?.ToArray(), Top, Offset, Distinct, Generic,
					Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static XPath ParseXPath(ScriptParser Parser, bool ExtractValue)
		{
			Parser.SkipWhiteSpace();

			StringBuilder sb = new StringBuilder();
			int Start = Parser.Position;
			char ch;

			while ((ch = Parser.PeekNextChar()) > 32 && ch != 160 && ch != ',' && ch != ';')
				sb.Append(Parser.NextChar());

			return new XPath(sb.ToString(), ExtractValue, Start, Parser.Position - Start, Parser.Expression);
		}

		internal static bool TryParseSources(ScriptParser Parser, out SourceDefinition Source)
		{
			if (!TryParseSource(Parser, out Source))
				return false;

			while (true)
			{
				string s = Parser.PeekNextToken().ToUpper();

				switch (s)
				{
					case ",":
						Parser.NextToken();
						if (!TryParseSource(Parser, out SourceDefinition Source2))
							return false;

						Source = new CrossJoin(Source, Source2, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					case "INNER":
					case "JOIN":
						Parser.NextToken();

						if (s == "INNER")
						{
							if (Parser.NextToken().ToUpper() != "JOIN")
								return false;
						}

						if (!TryParseSource(Parser, out Source2))
							return false;

						ScriptNode Conditions = ParseJoinConditions(Parser);
						Source = new InnerJoin(Source, Source2, Conditions, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					case "LEFT":
						Parser.NextToken();

						switch (Parser.NextToken().ToUpper())
						{
							case "JOIN":
								break;

							case "OUTER":
								if (Parser.NextToken().ToUpper() != "JOIN")
									return false;
								break;

							default:
								return false;
						}

						if (!TryParseSource(Parser, out Source2))
							return false;

						Conditions = ParseJoinConditions(Parser);
						Source = new LeftOuterJoin(Source, Source2, Conditions, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					case "RIGHT":
						Parser.NextToken();

						switch (Parser.NextToken().ToUpper())
						{
							case "JOIN":
								break;

							case "OUTER":
								if (Parser.NextToken().ToUpper() != "JOIN")
									return false;
								break;

							default:
								return false;
						}

						if (!TryParseSource(Parser, out Source2))
							return false;

						Conditions = ParseJoinConditions(Parser);
						Source = new RightOuterJoin(Source, Source2, Conditions, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					case "FULL":
						Parser.NextToken();

						switch (Parser.NextToken().ToUpper())
						{
							case "JOIN":
								break;

							case "OUTER":
								if (Parser.NextToken().ToUpper() != "JOIN")
									return false;
								break;

							default:
								return false;
						}

						if (!TryParseSource(Parser, out Source2))
							return false;

						Conditions = ParseJoinConditions(Parser);
						Source = new FullOuterJoin(Source, Source2, Conditions, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					case "OUTER":
						Parser.NextToken();

						if (Parser.NextToken().ToUpper() != "JOIN")
							return false;

						if (!TryParseSource(Parser, out Source2))
							return false;

						Conditions = ParseJoinConditions(Parser);
						Source = new FullOuterJoin(Source, Source2, Conditions, Source.Start, Parser.Position - Source.Start, Parser.Expression);
						break;

					default:
						return true;
				}
			}
		}

		private static ScriptNode ParseJoinConditions(ScriptParser Parser)
		{
			if (Parser.PeekNextToken().ToUpper() != "ON")
				return null;

			Parser.NextToken();

			return Parser.ParseOrs();
		}

		internal static bool TryParseSource(ScriptParser Parser, out SourceDefinition Source)
		{
			Parser.SkipWhiteSpace();

			int Start = Parser.Position;
			ScriptNode Node = Parser.ParseNoWhiteSpace();
			ScriptNode Name = null;
			string s;

			Parser.SkipWhiteSpace();

			s = Parser.PeekNextToken().ToUpper();
			if (!string.IsNullOrEmpty(s) &&
				IsAlias(s) &&
				s != "INNER" &&
				s != "OUTER" &&
				s != "LEFT" &&
				s != "RIGHT" &&
				s != "FULL" &&
				s != "JOIN" &&
				s != "WHERE" &&
				s != "GROUP" &&
				s != "ORDER" &&
				s != "OFFSET" &&
				s != "ON" &&
				s != "SET" &&
				s != "SELECT" &&
				s != "OBJECT" &&
				s != "OBJECTS")
			{
				if (s == "AS")
					Parser.NextToken();

				Name = Parser.ParseNoWhiteSpace();
			}
			else if (Node is VariableReference Ref)
				Name = new ConstantElement(new StringValue(Ref.VariableName), Node.Start, Node.Length, Node.Expression);

			Source = new SourceReference(Node, Name, Start, Parser.Position - Start, Parser.Expression);

			return true;
		}

		internal static bool IsAlias(string s)
		{
			foreach (char ch in s)
			{
				if (!char.IsLetterOrDigit(ch) && ch != '_')
					return false;
			}

			return true;
		}

		private bool ContainsVectorFunction(ScriptNode Node)
		{
			if (!this.SearchForVectorFunction(ref Node, null))
				return true;

			return !(Node?.ForAllChildNodes(this.SearchForVectorFunction, null, false) ?? true);
		}

		private bool SearchForVectorFunction(ref ScriptNode Node, object State)
		{
			if (Node is Function)
			{
				return !(Node is FunctionOneVectorVariable ||
					Node is Count);
			}

			return true;
		}

	}
}
