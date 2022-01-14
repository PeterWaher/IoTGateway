using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Abstract base class for external databases
	/// </summary>
	public abstract class ExternalDatabase : IDatabaseConnection
	{
		/// <summary>
		/// Abstract base class for external databases
		/// </summary>
		public ExternalDatabase()
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Executes an SQL Statement on the database.
		/// </summary>
		/// <param name="Statement">SQL Statement.</param>
		/// <returns>Result</returns>
		public abstract Task<IElement> ExecuteSqlStatement(string Statement);

		/// <summary>
		/// Gets a Schema table, given its collection name. 
		/// </summary>
		/// <param name="Name">Schema collection</param>
		/// <returns>Schema table, as a matrix</returns>
		public abstract Task<IElement> GetSchema(string Name);

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		public abstract Task<ILambdaExpression> GetProcedure(string Name);

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		public Task<ILambdaExpression> this[string Name] => this.GetProcedure(Name);

		/// <summary>
		/// Number of arguments.
		/// </summary>
		public virtual int NrArguments => 1;

		/// <summary>
		/// Argument Names.
		/// </summary>
		public virtual string[] ArgumentNames => new string[] { "SQL" };

		/// <summary>
		/// Argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes => argumentTypes1Parameters;

		private static readonly ArgumentType[] argumentTypes1Parameters = new ArgumentType[] { ArgumentType.Scalar };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0]?.AssociatedObjectValue is string SqlStatement))
				throw new ScriptRuntimeException("Argument must be a string.", null);

			return this.ExecuteSqlStatement(SqlStatement);
		}

	}
}
