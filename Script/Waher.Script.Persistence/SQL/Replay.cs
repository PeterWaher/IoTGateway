using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Persistence.XmlLedger;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
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
			Dictionary<string, int> ColumnIndices = new Dictionary<string, int>();
			List<KeyValuePair<string, ScriptNode>> AdditionalFields = null;
			ScriptNode[] Columns2 = this.columns;

			if (!(this.columns is null))
			{
				c = this.columns.Length;
				for (i = 0; i < c; i++)
				{
					if (this.columns[i] is VariableReference Ref)
						ColumnIndices[Ref.VariableName] = i;
					else if (this.columnNames[i] is VariableReference Ref2)
					{
						ColumnIndices[Ref2.VariableName] = i;

						if (AdditionalFields is null)
						{
							AdditionalFields = new List<KeyValuePair<string, ScriptNode>>();
							Columns2 = (ScriptNode[])Columns2.Clone();
						}

						AdditionalFields.Add(new KeyValuePair<string, ScriptNode>(Ref2.VariableName, this.columns[i]));
						Columns2[i] = new VariableReference(Ref2.VariableName, Ref2.Start, Ref2.Length, Ref2.Expression);
					}
				}
			}
			else
				c = 0;

			ILedgerExport Destination = null;
			ExportCounter Counter = null;
			ExportToScriptObject ObjectsExported = null;
			StringBuilder XmlOutput = null;

			if (!(this.to is null))
			{
				if (this.to is VariableReference Ref)
				{
					switch (Ref.VariableName.ToUpper())
					{
						case "JSON":
							Destination = ObjectsExported = new ExportToScriptObject();
							break;

						case "XML":
							XmlOutput = new StringBuilder();
							StringWriter Writer = new StringWriter(XmlOutput);
							Destination = new XmlFileLedger(Writer);
							break;

						case "COUNTERS":
							Destination = Counter = new ExportCounter(null);
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
						Destination = new XmlFileLedger(FileName, null, int.MaxValue);
					}
					else if (E.AssociatedObjectValue is null)
						Destination = null;
					else
						throw new ScriptRuntimeException("Unable to export to objects of type " + E.AssociatedObjectValue.GetType().FullName, this.to);
				}
			}

			if (Destination is null)
				Destination = ObjectsExported = new ExportToScriptObject();
			else if (Counter is null)
				Destination = Counter = new ExportCounter(Destination);

			if (Offset > 0)
				Destination = new ExportEntryOffset(Destination, Offset);

			if (Top != int.MaxValue)
				Destination = new ExportEntryMaxCount(Destination, Top);

			if (!(this.where is null))
				Destination = new ExportCondition(Destination, this.where, Variables);

			await Ledger.Export(Destination, new string[] { Source.CollectionName });

			// TODO:
			//
			// Table results
			//
			// Optimize on:
			//		Timestamp,
			//		Block,
			//		Collection,
			//		Creator
			//		ObjectId,
			//		TypeName,
			//		EntryType

			if (!(ObjectsExported is null))
				return ObjectsExported.ToVector();
			else if (!(XmlOutput is null))
			{
				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(XmlOutput.ToString());
				return new ObjectValue(Doc);
			}
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
