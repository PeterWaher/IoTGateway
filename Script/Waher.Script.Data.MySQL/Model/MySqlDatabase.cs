using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;
using Waher.Script.Model;

namespace Waher.Script.Data.MySQL.Model
{
	/// <summary>
	/// Manages a MySQL Server connection
	/// </summary>
	public class MySqlDatabase : IDatabaseConnection
	{
		private readonly Dictionary<string, StoredProcedure> procedures = new Dictionary<string, StoredProcedure>();
		private readonly SemaphoreSlim synchObject = new SemaphoreSlim(1);
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
				Command.CommandType = CommandType.Text;
				Command.CommandText = Statement;
				MySqlDataReader Reader = await Command.ExecuteReaderAsync();

				return await Reader.ParseAndClose();
			}
		}

		/// <summary>
		/// Gets a Schema table, given its collection name. 
		/// For a list of collections: https://mysqlconnector.net/overview/schema-collections/
		/// </summary>
		/// <param name="Name">Schema collection</param>
		/// <returns>Schema table, as a matrix</returns>
		public async Task<IElement> GetSchema(string Name)
		{
			DataTable Table = await this.connection.GetSchemaAsync(Name);
			return Table.ToMatrix();
		}

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		public async Task<ILambdaExpression> GetProcedure(string Name)
		{
			await this.synchObject.WaitAsync();
			try
			{
				if (this.procedures.TryGetValue(Name, out StoredProcedure Result))
					return Result;

				MySqlCommand Command = this.connection.CreateCommand();
				Command.CommandType = CommandType.StoredProcedure;
				Command.CommandText = Name;

				await MySqlCommandBuilder.DeriveParametersAsync(Command);

				Result = new StoredProcedure(Command);
				this.procedures[Name] = Result;

				return new StoredProcedure(Command);
			}
			finally
			{
				this.synchObject.Release();
			}
		}

		/// <summary>
		/// Creates a lambda expression for accessing a stored procedure.
		/// </summary>
		/// <param name="Name">Name of stored procedure.</param>
		/// <returns>Lambda expression.</returns>
		public Task<ILambdaExpression> this[string Name] => this.GetProcedure(Name);
	}
}
