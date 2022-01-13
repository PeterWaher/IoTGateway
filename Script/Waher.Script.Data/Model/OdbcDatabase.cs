using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Manages an ODBC SQL connection
	/// </summary>
	public class OdbcDatabase : IDatabaseConnection
	{
		private OdbcConnection connection;

		/// <summary>
		/// Manages an ODBC SQL connection
		/// </summary>
		/// <param name="Connection">Connection</param>
		public OdbcDatabase(OdbcConnection Connection)
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
			using (OdbcCommand Command = this.connection.CreateCommand())
			{
				Command.CommandText = Statement;
				DbDataReader Reader = await Command.ExecuteReaderAsync();

				return await MsSqlDatabase.ParseAndCloseReader(Reader);
			}
		}
	}
}
