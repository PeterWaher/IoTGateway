using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Interface for database connections
	/// </summary>
	public interface ISqlDatabaseConnection : IDisposable
	{
		/// <summary>
		/// Executes an SQL Statement on the database.
		/// </summary>
		/// <param name="Statement">SQL Statement.</param>
		/// <returns>Result</returns>
		Task<IElement> ExecuteSqlStatement(string Statement);

		/// <summary>
		/// Gets a Schema table, given its collection name. 
		/// </summary>
		/// <param name="Name">Schema collection</param>
		/// <returns>Schema table, as a matrix</returns>
		Task<IElement> GetSchema(string Name);

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		Task<ILambdaExpression> GetProcedure(string Name);

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		Task<ILambdaExpression> this[string Name] { get; }
	}
}
