using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Things.Queries;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
	/// <summary>
	/// Represents a query on a script node.
	/// </summary>
	public class ScriptQuery : ICommand, IEditableObject
	{
		private readonly Variables values;
		private readonly ScriptParameterNode[] parameters;
		private readonly VirtualNode node;
		private readonly ScriptQueryNode queryNode;

		/// <summary>
		/// Represents a query on a script node.
		/// </summary>
		/// <param name="Node">Script node publishing the command.</param>
		/// <param name="QueryNode">Script query node, defining the script for executing the query.</param>
		/// <param name="Parameters">Command parameter definitions.</param>
		public ScriptQuery(VirtualNode Node, ScriptQueryNode QueryNode, ScriptParameterNode[] Parameters)
		{
			this.node = Node;
			this.queryNode = QueryNode;
			this.parameters = Parameters;

			this.values = new Variables()
			{
				["this"] = this.node
			};
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => this.queryNode.CommandId;

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Query;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => this.queryNode.SortCategory;

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => this.queryNode.SortKey;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language) => Task.FromResult(this.queryNode.CommandName);

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language) => Task.FromResult(this.queryNode.Confirmation);

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language) => Task.FromResult(this.queryNode.Failure);

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language) => Task.FromResult(this.queryNode.Success);

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller) => Task.FromResult(true);   // TODO: Configure access rights.

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			throw new Exception("Script query is not a command.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public async Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			try
			{
				Namespace Namespace = await Language.GetNamespaceAsync(typeof(ScriptQuery).Namespace);

				this.values["Query"] = Query;
				this.values["Language"] = Namespace;

				if (!Query.IsStarted)
					await Query.Start();

				await Query.SetStatus(await Namespace.GetStringAsync(95, "Executing Query..."));

				object Result = await this.queryNode.ParsedScript.EvaluateAsync(this.values);

				if (!Query.HasTitle)
					await Query.SetTitle(await this.GetNameAsync(Language));

				if (!Query.HasReported && !(Result is null))
				{
					await Query.SetStatus(await Namespace.GetStringAsync(96, "Processing Result..."));
					await Query.BeginSection(await Namespace.GetStringAsync(94, "Query Result"));

					if (Result is ObjectMatrix ResultSet)
					{
						int NrColumns = ResultSet.Columns;
						int Column;
						Column[] Columns = new Column[NrColumns];
						string[] ColumnNames = ResultSet.ColumnNames;
						int NrColumnNames = ColumnNames?.Length ?? 0;

						for (Column = 0; Column < NrColumns; Column++)
						{
							IVector ColumnVector = ResultSet.GetColumn(Column);
							ColumnAlignment Alignment = ColumnAlignment.Left;
							string Header = Column < NrColumnNames ? ColumnNames[Column] : Column.ToString();
							byte? NrDec = null;

							if (ColumnVector is DoubleVector dv)
							{
								Alignment = ColumnAlignment.Right;
								NrDec = 0;

								foreach (double d in dv.Values)
								{
									byte NrDec2 = CommonTypes.GetNrDecimals(d);
									if (NrDec2 > NrDec)
										NrDec = NrDec2;
								}
							}
							else if (!(ColumnVector is ObjectVector))
								Alignment = ColumnAlignment.Center;

							Columns[Column] = new Column("C" + Column.ToString(), Header, null, null, null, null, Alignment, NrDec);
						}

						await Query.NewTable("ResultSet", await Namespace.GetStringAsync(97, "Result Set"), Columns);

						int NrRows = ResultSet.Rows;
						List<Record> Records = new List<Record>();
						List<object> RecordElements = new List<object>();

						foreach (IElement RowElement in ResultSet.VectorElements)
						{
							if (!(RowElement is IVector RowVector))
								continue;

							RecordElements.Clear();
							foreach (IElement Element in RowVector.VectorElements)
								RecordElements.Add(Element.AssociatedObjectValue);

							Records.Add(new Record(RecordElements.ToArray()));
						}

						await Query.NewRecords("ResultSet", Records.ToArray());
						await Query.TableDone("ResultSet");
					}
					else
						await Query.NewObject(Result);

					await Query.EndSection();
				}

				await Query.SetStatus(string.Empty);
			}
			catch (Exception ex)
			{
				await Query.LogMessage(ex);
			}
			finally
			{
				if (!Query.IsDone && !Query.IsAborted)
					await Query.Done();
			}
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
		{
			return new ScriptQuery(this.node, this.queryNode, this.parameters);
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		public async Task PopulateForm(DataForm Parameters, Language Language)
		{
			object Value;

			foreach (ScriptParameterNode Parameter in this.parameters)
			{
				lock (this.values)
				{
					if (this.values.TryGetVariable(Parameter.ParameterName, out Variable v))
						Value = v.ValueObject;
					else
						Value = null;
				}

				await Parameter.PopulateForm(Parameters, Language, Value);
			}
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public async Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language, bool OnlySetChanged)
		{
			SetEditableFormResult Result = new SetEditableFormResult();

			foreach (ScriptParameterNode Parameter in this.parameters)
				await Parameter.SetParameter(Parameters, Language, OnlySetChanged, this.values, Result);

			return Result;
		}

	}
}