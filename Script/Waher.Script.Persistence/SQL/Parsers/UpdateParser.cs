using System;
using System.Collections.Generic;
using Waher.Script.Model;
using Waher.Script.Operators;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Comparisons;

namespace Waher.Script.Persistence.SQL.Parsers
{
	/// <summary>
	/// Parses an UPDATE statement
	/// </summary>
	public class UpdateParser : IKeyWord
	{
		/// <summary>
		/// Parses an UPDATE statement
		/// </summary>
		public UpdateParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "UPDATE";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "LAZY", "SET", "WHERE" };

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
				string s = Parser.PeekNextToken().ToUpper();
				bool Lazy = s == "LAZY";

				if (Lazy)
					Parser.NextToken();

				if (!SelectParser.TryParseSources(Parser, out SourceDefinition Source))
					return false;

				s = Parser.NextToken().ToUpper();
				if (s != "SET")
					return false;

				List<Assignment> SetOperations = new List<Assignment>();
				ScriptNode Node = Parser.ParseList();

				if (!(Node is ElementList List))
					List = new ElementList(new ScriptNode[] { Node }, Node.Start, Node.Length, Node.Expression);

				foreach (ScriptNode Operation in List.Elements)
				{
					if (Operation is EqualTo EqualTo)
					{
						if (EqualTo.LeftOperand is VariableReference Ref)
							SetOperations.Add(new Assignment(Ref.VariableName, EqualTo.RightOperand, EqualTo.Start, EqualTo.Length, EqualTo.Expression));
						else
							return false;
					}
					else if (Operation is Assignment Assignment)
						SetOperations.Add(Assignment);
					else
						return false;
				}

				ScriptNode Where = null;

				s = Parser.PeekNextToken().ToUpper();
				if (s == "WHERE")
				{
					Parser.NextToken();
					Where = Parser.ParseOrs();
				}

				Result = new Update(Source, SetOperations.ToArray(), Where, Lazy, Parser.Start, Parser.Length, Parser.Expression);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
