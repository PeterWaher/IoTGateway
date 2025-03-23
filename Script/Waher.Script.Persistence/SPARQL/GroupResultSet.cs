using System;
using System.Collections.Generic;
using Waher.Content.Semantic;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Comparer for grouping a SPARQL result set.
	/// </summary>
	public class GroupResultSet : IComparer<ISparqlResultRecord>
	{
		private readonly string[] variables;
		private readonly string[] alias;
		private readonly ScriptNode[] expression;
		private readonly ScriptNode[] aliasExpression;
		private readonly bool[] calculated;
		private readonly bool[] hasAlias;
		private readonly bool[] hasCalculatedAlias;
		private readonly int count;

		/// <summary>
		/// Comparer for grouping a SPARQL result set.
		/// </summary>
		/// <param name="GroupBy">Group by-statement, containing a vector
		/// of variable names (or expressions), and corresponding names (if provided).</param>
		/// <param name="Names">Group alias names, if provided.</param>
		public GroupResultSet(ScriptNode[] GroupBy, ScriptNode[] Names)
		{
			int i;

			this.count = GroupBy.Length;
			if (Names.Length != this.count)
				throw new ArgumentException("Array size mismatch.", nameof(Names));

			this.expression = GroupBy;
			this.aliasExpression = Names;

			this.variables = new string[this.count];
			this.alias = new string[this.count];
			this.calculated = new bool[this.count];
			this.hasAlias = new bool[this.count];
			this.hasCalculatedAlias = new bool[this.count];

			for (i = 0; i < this.count; i++)
			{
				if (this.expression[i] is VariableReference Ref)
				{
					this.variables[i] = Ref.VariableName;
					this.calculated[i] = false;
				}
				else
					this.calculated[i] = true;

				if (this.aliasExpression[i] is null)
					this.hasAlias[i] = false;
				else if (this.aliasExpression[i] is VariableReference Ref2)
				{
					this.alias[i] = Ref2.VariableName;
					this.hasCalculatedAlias[i] = false;
				}
				else
					this.hasCalculatedAlias[i] = true;
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
				e1 = this.GetValue(x, i);
				e2 = this.GetValue(y, i);

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
						return j;
				}
			}

			return 0;
		}

		private ISemanticElement GetValue(ISparqlResultRecord Record, int Index)
		{
			if (!this.calculated[Index])
				return Record[this.variables[Index]];

			ObjectProperties ObjectProperties = null;
			string Alias = null;
			ISemanticElement Result;

			if (this.hasAlias[Index])
			{
				if (this.hasCalculatedAlias[Index])
				{
					if (ObjectProperties is null)
						ObjectProperties = new ObjectProperties(Record, new Variables());

					Alias = EvaluateValue(this.aliasExpression[Index], ObjectProperties)?.ToString();
				}
				else
					Alias = this.alias[Index];

				if (!(Alias is null))
				{
					Result = Record[Alias];
					if (!(Result is null))
						return Result;
				}
			}

			if (ObjectProperties is null)
				ObjectProperties = new ObjectProperties(Record, new Variables());

			Result = OrderResultSet.EvaluateElement(this.expression[Index], ObjectProperties);
			Record[Alias] = Result;

			return Result;
		}

		private static object EvaluateValue(ScriptNode Expression, Variables Variables)
		{
			try
			{
				return Expression.Evaluate(Variables)?.AssociatedObjectValue;
			}
			catch (ScriptReturnValueException ex)
			{
				return ex.ReturnValue.AssociatedObjectValue;
				//object ReturnValue = ex.ReturnValue.AssociatedObjectValue;
				//ScriptReturnValueException.Reuse(ex);
				//return ReturnValue;
			}
			catch (ScriptBreakLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptContinueLoopException.Reuse(ex);
			}
		}
	}
}
