using System;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;

namespace Waher.Script.Data.MySQL.Model
{
	/// <summary>
	/// Manages a MySQL Server connection
	/// </summary>
	public class MySqlDatabase : IDatabaseConnection
	{
		private MySqlConnection connection;

		/// <summary>
		/// Manages a MySQL Server connection
		/// </summary>
		/// <param name="Connection">Connection</param>
		public MySqlDatabase(MySqlConnection Connection)
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
			using (MySqlCommand Command = this.connection.CreateCommand())
			{
				Command.CommandText = Statement;
				MySqlDataReader Reader = await Command.ExecuteReaderAsync();
				
				return await MsSqlDatabase.ParseAndCloseReader(Reader);
			}
		}
	}
}
