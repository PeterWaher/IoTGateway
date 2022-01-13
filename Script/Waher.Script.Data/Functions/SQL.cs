using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Data.Functions
{
	public class SQL : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Executes an SQL statement given by the string <paramref name="Statement"/> on the database connection provided 
		/// in <paramref name="Database"/>.
		/// </summary>
		/// <param name="Database">Database connection</param>
		/// <param name="Statement">SQL Statement</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public SQL(ScriptNode Database, ScriptNode Statement, int Start, int Length, Expression Expression)
			: base(Database, Statement, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "SQL";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Database", "Statement" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument1, Argument2, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			if (!(Argument1?.AssociatedObjectValue is IDatabaseConnection DatabaseConnection))
				throw new ScriptRuntimeException("First argument must be a database connection.", this);

			if (!(Argument2?.AssociatedObjectValue is string SqlStatement))
				throw new ScriptRuntimeException("Second argument must be a string.", this);

			return DatabaseConnection.ExecuteSqlStatement(SqlStatement);
		}
	}
}
