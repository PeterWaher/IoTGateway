using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

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
				Command.CommandText = Statement;
				SqlDataReader Reader = await Command.ExecuteReaderAsync();

				return await ParseAndCloseReader(Reader);
			}
		}

		/// <summary>
		/// Parses the result from a Database Reader, and returns the corresponding script object.
		/// </summary>
		/// <param name="Reader">Database reader</param>
		/// <returns>Parsed result.</returns>
		public static async Task<IElement> ParseAndCloseReader(DbDataReader Reader)
		{
			try
			{
				if (!Reader.HasRows)
					return ObjectValue.Null;

				string[] ColumnNames;

				if (Reader.CanGetColumnSchema())
				{
					ReadOnlyCollection<DbColumn> Columns = Reader.GetColumnSchema();
					List<string> Names = new List<string>();
					int i = 1;

					foreach (DbColumn Column in Columns)
					{
						if (string.IsNullOrEmpty(Column.ColumnName))
							Names.Add(i.ToString());
						else
							Names.Add(Column.ColumnName);

						i++;
					}

					ColumnNames = Names.ToArray();
				}
				else
					ColumnNames = null;

				int NrRows = 0;
				int NrColumns = 0;
				LinkedList<IElement> Elements = new LinkedList<IElement>();
				object[] Row = null;

				while (await Reader.ReadAsync())
				{
					if (NrColumns == 0)
					{
						NrColumns = Reader.FieldCount;
						Row = new object[NrColumns];
					}

					Reader.GetValues(Row);

					foreach (object Item in Row)
						Elements.AddLast(Expression.Encapsulate(Item));

					NrRows++;
				}

				if (NrRows == 1 && NrColumns == 1)
					return Elements.First.Value;

				return new ObjectMatrix(NrRows, NrColumns, Elements)
				{
					ColumnNames = ColumnNames
				};
				
				// TODO: Multiple result sets
			}
			finally
			{
				Reader.Close();
			}
		}

	}
}
