using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Manages a Microsoft SQL Server connection
	/// </summary>
	public class MsSqlDatabase : IDatabaseConnection
	{
		private SqlConnection connection;

		/// <summary>
		/// Manages a Microsoft SQL Server connection
		/// </summary>
		/// <param name="Connection">Connection</param>
		public MsSqlDatabase(SqlConnection Connection)
		{
			this.connection = Connection;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.connection?.Close();
			this.connection?.Dispose();
			this.connection = null;
		}

		/// <summary>
		/// Executes an SQL Statement on the database.
		/// </summary>
		/// <param name="Statement">SQL Statement.</param>
		/// <returns>Result</returns>
		public async Task<IElement> ExecuteSqlStatement(string Statement)
		{
			using (SqlCommand Command = this.connection.CreateCommand())
			{
				Command.CommandType = CommandType.Text;
				Command.CommandText = Statement;
				SqlDataReader Reader = await Command.ExecuteReaderAsync();

				return await Reader.ParseAndClose();
			}
		}

	}
}
