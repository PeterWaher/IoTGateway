using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Static class with common data extensions.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Parses the result from a Database Reader, and returns the corresponding script object.
		/// </summary>
		/// <param name="Reader">Database reader</param>
		/// <returns>Parsed result.</returns>
		public static async Task<IElement> ParseAndClose(this DbDataReader Reader)
		{
			try
			{
				ChunkedList<IElement> Results = null;
				IElement Result = null;
				bool First = true;

				do
				{
					if (First)
						First = false;
					else
					{
						if (Results is null)
							Results = new ChunkedList<IElement>();

						Results.Add(Result);
					}

					if (!Reader.HasRows)
						Result = ObjectValue.Null;
					else
					{
						string[] ColumnNames;

						if (Reader.CanGetColumnSchema())
						{
							ReadOnlyCollection<DbColumn> Columns = Reader.GetColumnSchema();
							ChunkedList<string> Names = new ChunkedList<string>();
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
						ChunkedList<IElement> Elements = new ChunkedList<IElement>();
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
								Elements.Add(Expression.Encapsulate(Item));

							NrRows++;
						}

						if (NrRows == 1 && NrColumns == 1)
							Result = Elements.FirstItem;
						else
						{
							Result = new ObjectMatrix(NrRows, NrColumns, Elements)
							{
								ColumnNames = ColumnNames
							};
						}
					}
				}
				while (await Reader.NextResultAsync());

				if (Results is null)
					return Result;
				else
				{
					Results.Add(Result);
					return VectorDefinition.Encapsulate(Results, false, null);
				}
			}
			finally
			{
				Reader.Close();
			}
		}

		/// <summary>
		/// Converts a Data Table to a matrix.
		/// </summary>
		/// <param name="Table">Table</param>
		/// <returns>Matrix</returns>
		public static ObjectMatrix ToMatrix(this DataTable Table)
		{
			int NrColumns = Table.Columns.Count;
			int NrRows = Table.Rows.Count;
			string[] Header = new string[NrColumns];
			IElement[,] Elements = new IElement[NrRows, NrColumns];
			int i, j;

			for (i = 0; i < NrColumns; i++)
				Header[i] = Table.Columns[i].ColumnName;

			for (j = 0; j < NrRows; j++)
			{
				DataRow Row = Table.Rows[j];
				for (i = 0; i < NrColumns; i++)
					Elements[j, i] = Expression.Encapsulate(Row[i]);
			}

			return new ObjectMatrix(Elements)
			{
				ColumnNames = Header
			};
		}
	}
}
