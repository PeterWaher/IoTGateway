using System;
using System.Collections.Generic;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Comparer for ordering a SPARQL result set.
	/// </summary>
	public class OrderResultSet : IComparer<ISparqlResultRecord>
	{
		private readonly string[] variables;
		private readonly ScriptNode[] expression;
		private readonly bool[] ascending;
		private readonly bool[] calculated;
		private readonly int count;

		/// <summary>
		/// Comparer for ordering a SPARQL result set.
		/// </summary>
		/// <param name="OrderBy">Order by-statement, containing a vector
		/// of variable names (or expressions), and corresponding ascending (true) or
		/// descending (false) direction.</param>
		public OrderResultSet(KeyValuePair<ScriptNode, bool>[] OrderBy)
		{
			ScriptNode Node;
			int i;

			this.count = OrderBy.Length;
			this.variables = new string[this.count];
			this.expression = new ScriptNode[this.count];
			this.ascending = new bool[this.count];
			this.calculated = new bool[this.count];

			for (i = 0; i < this.count; i++)
			{
				this.expression[i] = Node = OrderBy[i].Key;
				this.ascending[i] = OrderBy[i].Value;

				if (Node is VariableReference Ref)
				{
					this.variables[i] = Ref.VariableName;
					this.calculated[i] = false;
				}
				else
					this.calculated[i] = true;
			}
		}

		/// <summary>
		/// Compares two records.
		/// </summary>
		/// <param name="x">Record 1</param>
		/// <param name="y">Record 2</param>
		/// <returns>
		/// Negative, if Record 1 is less than Record 2
		/// Zero, if records are equal
		/// Positive, if Record 1 is greated than Record 2
		/// </returns>
		public int Compare(ISparqlResultRecord x, ISparqlResultRecord y)
		{
			ISemanticElement e1, e2;
			int i, j;

			for (i = 0; i < this.count; i++)
			{
				if (this.calculated[i])
				{
					Variables v = new Variables();
					ObjectProperties v1 = new ObjectProperties(x, v);
					ObjectProperties v2 = new ObjectProperties(y, v);

					e1 = EvaluateElement(this.expression[i], v1);
					e2 = EvaluateElement(this.expression[i], v2);
				}
				else
				{
					e1 = x[this.variables[i]];
					e2 = y[this.variables[i]];
				}

				if (e1 is null)
				{
					if (!(e2 is null))
						return -1;
				}
				else if (e2 is null)
					return 1;
				else
				{
					j = e1.CompareTo(e2);
					if (j != 0)
						return this.ascending[i] ? j : -j;
				}
			}

			return 0;
		}

		internal static ISemanticElement EvaluateElement(ScriptNode Expression, Variables Variables)
		{
			object Obj;

			try
			{
				Obj = Expression.Evaluate(Variables)?.AssociatedObjectValue;    // TODO: Async
			}
			catch (ScriptReturnValueException ex)
			{
				Obj = ex.ReturnValue.AssociatedObjectValue;
				//ScriptReturnValueException.Reuse(ex);
			}
			catch (ScriptBreakLoopException ex)
			{
				Obj = ex.LoopValue?.AssociatedObjectValue;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				Obj = ex.LoopValue?.AssociatedObjectValue;
				//ScriptContinueLoopException.Reuse(ex);
			}
			catch (Exception ex)
			{
				return new StringLiteral(ex.Message);
			}

			return SemanticElements.Encapsulate(Obj);
		}
	}
}
