using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Persistence.XmlLedger;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Operators.Membership;
using Waher.Script.Persistence.SQL.LedgerExports;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a REPLAY statement against the ledger.
	/// </summary>
	public class Replay : ScriptNode, IEvaluateAsync
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private SourceDefinition source;
		private ScriptNode top;
		private ScriptNode where;
		private ScriptNode offset;
		private ScriptNode to;

		/// <summary>
		/// Executes a REPLAY statement against the ledger.
		/// </summary>
		/// <param name="Columns">Columns to select. If null, all columns are selected.</param>
		/// <param name="ColumnNames">Optional renaming of columns.</param>
		/// <param name="Source">Source(s) to select from.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Top">Optional limit on number of records to return</param>
		/// <param name="Offset">Optional offset into result set where reporting begins</param>
		/// <param name="To">Destination of export.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Replay(ScriptNode[] Columns, ScriptNode[] ColumnNames, SourceDefinition Source, ScriptNode Where,
			ScriptNode Top, ScriptNode Offset, ScriptNode To, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.columns = Columns;
			this.columns?.SetParent(this);

			this.columnNames = ColumnNames;
			this.columnNames?.SetParent(this);

			this.top = Top;
			this.top?.SetParent(this);

			this.source = Source;
			this.source?.SetParent(this);

			this.where = Where;
			this.where?.SetParent(this);

			this.offset = Offset;
			this.offset?.SetParent(this);

			this.to = To;
			this.to?.SetParent(this);

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("Columns and ColumnNames must both be null or not null.", nameof(ColumnNames));

			if (!(this.columns is null) && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("Columns and ColumnNames must be of equal length.", nameof(ColumnNames));
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node asynchronously, using the variables provided in 
		/// the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E;
			int Top;
			int Offset;
			int i, c;

			if (!(this.top is null))
			{
				E = await this.top.EvaluateAsync(Variables);
				Top = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Top <= 0)
					throw new ScriptRuntimeException("TOP must evaluate to a positive integer.", this.top);
			}
			else
				Top = int.MaxValue;

			if (!(this.offset is null))
			{
				E = await this.offset.EvaluateAsync(Variables);
				Offset = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Offset < 0)
					throw new ScriptRuntimeException("OFFSET must evaluate to a non-negative integer.", this.offset);
			}
			else
				Offset = 0;

			IDataSource Source = await this.source.GetSource(Variables);
			ChunkedList<KeyValuePair<string, ScriptNode>> AdditionalFields = null;
			ScriptNode[] Columns2 = this.columns;
			bool Columns2Cloned = false;
			Dictionary<string, int> ColumnIndices = null;

			if (!(this.columns is null))
			{
				ColumnIndices = new Dictionary<string, int>();

				c = this.columns.Length;
				for (i = 0; i < c; i++)
				{
					if (this.columnNames[i] is VariableReference Ref2)
					{
						ColumnIndices[Ref2.VariableName] = i;

						if (!Columns2Cloned)
						{
							Columns2Cloned = true;
							Columns2 = (ScriptNode[])this.columns.Clone();
						}

						Columns2[i] = Ref2;

						if (AdditionalFields is null)
							AdditionalFields = new ChunkedList<KeyValuePair<string, ScriptNode>>();

						AdditionalFields.Add(new KeyValuePair<string, ScriptNode>(Ref2.VariableName, this.columns[i]));
					}
					else if (this.columns[i] is VariableReference Ref)
						ColumnIndices[Ref.VariableName] = i;
				}
			}

			ILedgerExport Destination = null;
			ExportCounter Counter = null;
			ExportToJson ObjectsExported = null;
			ExportToTable TableExported = null;
			ExportToVariableTable VariableTableExported = null;
			ExportToLambda LambdaExported = null;
			StringBuilder XmlOutput = null;
			bool FilterColumns = true;

			if (!(this.to is null))
			{
				if (this.to is VariableReference Ref)
				{
					switch (Ref.VariableName.ToUpper())
					{
						case "JSON":
							Destination = ObjectsExported = new ExportToJson();
							break;

						case "XML":
							XmlOutput = new StringBuilder();
							StringWriter Writer = new StringWriter(XmlOutput);
							Destination = new XmlFileLedger(Writer);
							break;

						case "COUNTERS":
							Destination = Counter = new ExportCounter(null);
							break;

						case "TABLE":
							if (Columns2 is null)
								Destination = VariableTableExported = new ExportToVariableTable();
							else
								Destination = TableExported = new ExportToTable(Columns2);
							FilterColumns = false;
							break;
					}
				}

				if (Destination is null)
				{
					E = await this.to.EvaluateAsync(Variables);
					if (E.AssociatedObjectValue is ILedgerExport Export)
						Destination = Export;
					else if (E.AssociatedObjectValue is string FileName)
					{
						switch (FileName.ToUpper())
						{
							case "JSON":
								Destination = ObjectsExported = new ExportToJson();
								break;

							case "XML":
								XmlOutput = new StringBuilder();
								StringWriter Writer = new StringWriter(XmlOutput);
								Destination = new XmlFileLedger(Writer);
								break;

							case "COUNTERS":
								Destination = Counter = new ExportCounter(null);
								break;

							case "TABLE":
								if (Columns2 is null)
									Destination = VariableTableExported = new ExportToVariableTable();
								else
									Destination = TableExported = new ExportToTable(Columns2);
								FilterColumns = false;
								break;

							default:
								Destination = new XmlFileLedger(FileName, null, int.MaxValue);
								break;
						}
					}
					else if (E.AssociatedObjectValue is ILambdaExpression Lambda)
					{
						if (Lambda.NrArguments != 1)
							throw new ScriptRuntimeException("Lambda expression used in REPLAY must have one argument.", this);

						Destination = LambdaExported = new ExportToLambda(Lambda);
					}
					else if (E.AssociatedObjectValue is null)
						Destination = null;
					else
						throw new ScriptRuntimeException("Unable to export to objects of type " + E.AssociatedObjectValue.GetType().FullName, this.to);
				}
			}

			if (Destination is null)
			{
				if (!(this.columns is null))
				{
					Destination = TableExported = new ExportToTable(Columns2);
					FilterColumns = false;
				}
				else
					Destination = ObjectsExported = new ExportToJson();
			}
			else if (Counter is null)
				Destination = Counter = new ExportCounter(Destination);

			if (!(ColumnIndices is null) && FilterColumns)
				Destination = new ExportColumnFilter(Destination, ColumnIndices);

			if (Offset > 0)
				Destination = new ExportEntryOffset(Destination, Offset);

			if (Top != int.MaxValue)
				Destination = new ExportEntryMaxCount(Destination, Top);

			LedgerExportRestriction Restriction = new LedgerExportRestriction()
			{
				CollectionNames = new string[] { Source.CollectionName }
			};

			if (!(this.where is null) ||
				!(TableExported is null) ||
				!(VariableTableExported is null) ||
				!(LambdaExported is null) ||
				!(AdditionalFields is null))
			{
				ExportCondition Conditions = new ExportCondition(Destination, this.where, Variables, AdditionalFields);
				Destination = Conditions;

				if (!(TableExported is null))
					TableExported.Variables = Conditions.EntryVariables;

				if (!(VariableTableExported is null))
					VariableTableExported.Variables = Conditions.EntryVariables;

				if (!(LambdaExported is null))
					LambdaExported.Variables = Conditions.EntryVariables;

				if (!(this.where is null))
					await FindRestrictions(Restriction, this.where, Variables);
			}

			await Ledger.Export(Destination, Restriction);

			if (!(ObjectsExported is null))
				return ObjectsExported.ToVector();
			else if (!(XmlOutput is null))
			{
				XmlDocument Doc = XML.ParseXml(XmlOutput.ToString());
				return new ObjectValue(Doc);
			}
			else if (!(TableExported is null))
				return TableExported.ToMatrix();
			else if (!(VariableTableExported is null))
				return VariableTableExported.ToMatrix();
			else if (!(Counter is null))
			{
				return new ObjectValue(new Dictionary<string, IElement>()
				{
					{ "NrCollections", new DoubleNumber(Counter.NrCollections) },
					{ "NrBlocks", new DoubleNumber(Counter.NrBlocks) },
					{ "NrEntries", new DoubleNumber(Counter.NrEntries) },
					{ "NrProperties", new DoubleNumber(Counter.NrProperties) }
				});
			}
			else
				return ObjectValue.Null;
		}

		private static async Task FindRestrictions(LedgerExportRestriction Restriction,
			ScriptNode Where, Variables Variables)
		{
			if (Where is BinaryOperator BinOp)
			{
				if (BinOp is Operators.Logical.And ||
					BinOp is Operators.Binary.And ||
					BinOp is Operators.Dual.And)
				{
					await FindRestrictions(Restriction, BinOp.LeftOperand, Variables);
					await FindRestrictions(Restriction, BinOp.RightOperand, Variables);
				}
				else if (BinOp is Operators.Logical.Or ||
					BinOp is Operators.Binary.Or ||
					BinOp is Operators.Dual.Or)
				{
					LedgerExportRestriction Restriction1 = new LedgerExportRestriction();
					LedgerExportRestriction Restriction2 = new LedgerExportRestriction();

					await FindRestrictions(Restriction1, BinOp.LeftOperand, Variables);
					await FindRestrictions(Restriction2, BinOp.RightOperand, Variables);

					if (!(Restriction1.BlockIds is null) && !(Restriction2.BlockIds is null))
						Restriction.BlockIds = Restriction1.BlockIds.Join(Restriction2.BlockIds);
					else if (!(Restriction1.Creators is null) && !(Restriction2.Creators is null))
						Restriction.Creators = Restriction1.Creators.Join(Restriction2.Creators);
				}
				else if (BinOp is Range Range)
				{
					if (!(Range.MiddleOperand is VariableReference MidVar))
						return;

					if (MidVar.VariableName != "Created")
						return;

					object LeftValue = await Eval(Range.LeftOperand, Variables);
					if (LeftValue is null)
						return;

					object RightValue = await Eval(Range.RightOperand, Variables);
					if (RightValue is null)
						return;

					if (LeftValue is DateTime LeftTP)
						LeftTP = LeftTP.ToUniversalTime();
					else if (LeftValue is DateTimeOffset LeftTPO)
						LeftTP = LeftTPO.UtcDateTime;
					else
						LeftTP = DateTime.MinValue;

					if (LeftTP != DateTime.MinValue)
					{
						Restriction.MinCreated = LeftTP;
						Restriction.MinCreatedIncluded = Range.LeftInclusive;
					}

					if (RightValue is DateTime RightTP)
						RightTP = RightTP.ToUniversalTime();
					else if (RightValue is DateTimeOffset RightTPO)
						RightTP = RightTPO.UtcDateTime;
					else
						RightTP = DateTime.MaxValue;

					if (RightTP != DateTime.MaxValue)
					{
						Restriction.MaxCreated = RightTP;
						Restriction.MaxCreatedIncluded = Range.RightInclusive;
					}
				}
				else
				{
					object Value;
					string Name;
					bool Invert;
					bool LeftIsVariable;

					if (BinOp.LeftOperand is VariableReference LeftVar)
					{
						LeftIsVariable = true;
						Name = LeftVar.VariableName;
						Invert = false;

						Value = await Eval(BinOp.RightOperand, Variables);
						if (Value is null)
							return;
					}
					else if (BinOp.RightOperand is VariableReference RightVar)
					{
						LeftIsVariable = false;
						Name = RightVar.VariableName;
						Invert = true;

						Value = await Eval(BinOp.LeftOperand, Variables);
						if (Value is null)
							return;
					}
					else
						return;

					if (BinOp is EqualTo || BinOp is EqualToElementWise ||
						BinOp is IdenticalTo || BinOp is IdenticalToElementWise)
					{
						switch (Name)
						{
							case "BlockId":
								if (Value is string BlockId)
									Restriction.BlockIds = new string[] { BlockId };
								break;

							case "Creator":
								if (Value is string Creator)
									Restriction.Creators = new string[] { Creator };
								break;

							case "Created":
								if (Value is DateTime Created)
									Created = Created.ToUniversalTime();
								else if (Value is DateTimeOffset Created2)
									Created = Created2.UtcDateTime;
								else
									break;

								Restriction.MinCreated = Created;
								Restriction.MaxCreated = Created;
								Restriction.MinCreatedIncluded = true;
								Restriction.MaxCreatedIncluded = true;
								break;
						}
					}
					else if (BinOp is GreaterThan)
					{
						switch (Name)
						{
							case "Created":
								if (Value is DateTime Created)
									Created = Created.ToUniversalTime();
								else if (Value is DateTimeOffset Created2)
									Created = Created2.UtcDateTime;
								else
									break;

								if (Invert)
								{
									Restriction.MaxCreated = Created;
									Restriction.MaxCreatedIncluded = false;
								}
								else
								{
									Restriction.MinCreated = Created;
									Restriction.MinCreatedIncluded = false;
								}
								break;
						}
					}
					else if (BinOp is GreaterThanOrEqualTo)
					{
						switch (Name)
						{
							case "Created":
								if (Value is DateTime Created)
									Created = Created.ToUniversalTime();
								else if (Value is DateTimeOffset Created2)
									Created = Created2.UtcDateTime;
								else
									break;

								if (Invert)
								{
									Restriction.MaxCreated = Created;
									Restriction.MaxCreatedIncluded = true;
								}
								else
								{
									Restriction.MinCreated = Created;
									Restriction.MinCreatedIncluded = true;
								}
								break;
						}
					}
					else if (BinOp is LesserThan)
					{
						switch (Name)
						{
							case "Created":
								if (Value is DateTime Created)
									Created = Created.ToUniversalTime();
								else if (Value is DateTimeOffset Created2)
									Created = Created2.UtcDateTime;
								else
									break;

								if (Invert)
								{
									Restriction.MinCreated = Created;
									Restriction.MinCreatedIncluded = false;
								}
								else
								{
									Restriction.MaxCreated = Created;
									Restriction.MaxCreatedIncluded = false;
								}
								break;
						}
					}
					else if (BinOp is LesserThanOrEqualTo)
					{
						switch (Name)
						{
							case "Created":
								if (Value is DateTime Created)
									Created = Created.ToUniversalTime();
								else if (Value is DateTimeOffset Created2)
									Created = Created2.UtcDateTime;
								else
									break;

								if (Invert)
								{
									Restriction.MinCreated = Created;
									Restriction.MinCreatedIncluded = true;
								}
								else
								{
									Restriction.MaxCreated = Created;
									Restriction.MaxCreatedIncluded = true;
								}
								break;
						}
					}
					else if (BinOp is In)
					{
						if (!LeftIsVariable || !IsStringArray(Value, out string[] Options))
							return;

						switch (Name)
						{
							case "BlockId":
								Restriction.BlockIds = Options;
								break;

							case "Creator":
								Restriction.Creators = Options;
								break;
						}
					}
				}
			}
		}

		private static bool IsStringArray(object Value, out string[] Options)
		{
			if (Value is string[] A)
			{
				Options = A;
				return true;
			}
			else if (Value is object[] B)
			{
				int i, c = B.Length;
				Options = new string[c];

				for (i = 0; i < c; i++)
				{
					if (B[i] is string s)
						Options[i] = s;
					else
						return false;
				}

				return true;
			}
			else
			{
				Options = null;
				return false;
			}
		}

		private static async Task<object> Eval(ScriptNode Node, Variables Variables)
		{
			IElement Element;

			try
			{
				if (Node.IsAsynchronous)
					Element = await Node.EvaluateAsync(Variables);
				else
					Element = Node.Evaluate(Variables);
			}
			catch (Exception)
			{
				return null;
			}

			return Element.AssociatedObjectValue;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order) ||
					!this.columnNames.ForAllChildNodes(Callback, State, Order) ||
					!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.to?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			if (!this.columns.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder) ||
				!this.columnNames.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder))
			{
				return false;
			}

			ScriptNode NewNode;
			bool b;

			if (!(this.source is null))
			{
				b = !Callback(this.source, out NewNode, State);
				if (!(NewNode is null) && NewNode is SourceDefinition Source2)
				{
					this.source = Source2;
					this.source.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.source.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.top is null))
			{
				b = !Callback(this.top, out NewNode, State);
				if (!(NewNode is null))
				{
					this.top = NewNode;
					this.top.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.top.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.where is null))
			{
				b = !Callback(this.where, out NewNode, State);
				if (!(NewNode is null))
				{
					this.where = NewNode;
					this.where.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.where.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.offset is null))
			{
				b = !Callback(this.offset, out NewNode, State);
				if (!(NewNode is null))
				{
					this.offset = NewNode;
					this.offset.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.offset.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.to is null))
			{
				b = !Callback(this.to, out NewNode, State);
				if (!(NewNode is null))
				{
					this.to = NewNode;
					this.to.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.to.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order) ||
					!this.columnNames.ForAllChildNodes(Callback, State, Order) ||
					!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.to?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return !(obj is Replay O &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.columnNames, O.columnNames) &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.top, O.top) &&
				AreEqual(this.where, O.where) &&
				AreEqual(this.offset, O.offset) &&
				AreEqual(this.to, O.to) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.columnNames);
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.top);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			Result ^= Result << 5 ^ GetHashCode(this.offset);
			Result ^= Result << 5 ^ GetHashCode(this.to);

			return Result;
		}

	}
}
