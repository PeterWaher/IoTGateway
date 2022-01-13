using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Interface for database connections
	/// </summary>
	public interface IDatabaseConnection : IDisposable
	{
		/// <summary>
		/// Executes an SQL Statement on the database.
		/// </summary>
		/// <param name="Statement">SQL Statement.</param>
		/// <returns>Result</returns>
		Task<IElement> ExecuteSqlStatement(string Statement);
	}
}
