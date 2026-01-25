using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Persistence.SQL.Processors;

namespace Waher.Script.Persistence.SQL
{
    /// <summary>
    /// Executes a SELECT statement against the object database.
    /// </summary>
    public class Select : ScriptNode, IEvaluateAsync
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupByNames;
		private readonly KeyValuePair<ScriptNode, bool>[] orderBy;
		private readonly bool distinct;
		private readonly bool generic;
		private readonly bool selectOneObject;
		private SourceDefinition source;
		private ScriptNode top;
		private ScriptNode where;
		private ScriptNode having;
		private ScriptNode offset;

		/// <summary>
		/// Executes a SELECT statement against the object database.
		/// </summary>
		/// <param name="Columns">Columns to select. If null, all columns are selected.</param>
		/// <param name="ColumnNames">Optional renaming of columns.</param>
		/// <param name="Source">Source(s) to select from.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="GroupBy">Optional grouping</param>
		/// <param name="GroupByNames">Optional renaming of groups</param>
		/// <param name="Having">Optional Having clause</param>
		/// <param name="OrderBy">Optional ordering</param>
		/// <param name="Top">Optional limit on number of records to return</param>
		/// <param name="Offset">Optional offset into result set where reporting begins</param>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Generic">If objects of type <see cref="GenericObject"/> should be returned.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(ScriptNode[] Columns, ScriptNode[] ColumnNames, SourceDefinition Source, 
			ScriptNode Where, ScriptNode[] GroupBy, ScriptNode[] GroupByNames, ScriptNode Having, 
			KeyValuePair<ScriptNode, bool>[] OrderBy, ScriptNode Top, ScriptNode Offset,
			bool Distinct, bool Generic, int Start, int Length, Expression Expression)
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

			this.groupBy = GroupBy;
			this.groupBy?.SetParent(this);

			this.groupByNames = GroupByNames;
			this.groupByNames?.SetParent(this);

			this.having = Having;
			this.having?.SetParent(this);

			this.orderBy = OrderBy;
			if (!(this.orderBy is null))
			{
				foreach (KeyValuePair<ScriptNode, bool> P in this.orderBy)
					P.Key?.SetParent(this);
			}

			this.offset = Offset;
			this.offset?.SetParent(this);

			this.distinct = Distinct;
			this.generic = Generic;

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("Columns and ColumnNames must both be null or not null.", nameof(ColumnNames));

			if (!(this.columns is null) && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("Columns and ColumnNames must be of equal length.", nameof(ColumnNames));

			if (this.groupByNames is null ^ this.groupBy is null)
				throw new ArgumentException("GroupBy and GroupByNames must both be null or not null.", nameof(GroupByNames));

			if (!(this.groupBy is null) && this.groupBy.Length != this.groupByNames.Length)
				throw new ArgumentException("GroupBy and GroupByNames must be of equal length.", nameof(GroupByNames));

			this.selectOneObject = this.CalcSelectOneObject();
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		private bool CalcSelectOneObject()
		{
			if ((this.columns is null || this.columns.Length == 1) &&
				!(this.top is null) &&
				this.top is ConstantElement TopConstant &&
				TopConstant.Constant.AssociatedObjectValue is double D &&
				D == 1)
			{
				return true;    // select top 1 * ...
			}
			else
			{
				if (!(this.columns is null))
				{
					bool AllXPath = true;

					foreach (ScriptNode Column in this.columns)
					{
						if (!(Column is Functions.XPath))
						{
							AllXPath = false;
							break;
						}
					}

					if (AllXPath)
						return true;
				}

				return false;
			}
		}

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

			ChunkedList<KeyValuePair<VariableReference, bool>> OrderBy = new ChunkedList<KeyValuePair<VariableReference, bool>>();
			bool CalculatedOrder = false;

			if (!(this.groupBy is null))
			{
				foreach (ScriptNode Node in this.groupBy)
				{
					if (Node is VariableReference Ref)
						this.AddIfNotFound(Ref, true, OrderBy);
					else
					{
						CalculatedOrder = true;
						break;
					}
				}
			}

			if (!(this.orderBy is null) && !CalculatedOrder)
			{
				foreach (KeyValuePair<ScriptNode, bool> Node in this.orderBy)
				{
					if (Node.Key is VariableReference Ref)
					{
						if (!(this.columnNames is null))
						{
							foreach (ScriptNode N in this.columnNames)
							{
								if (!(N is null) && N is VariableReference Ref2 && Ref2.VariableName == Ref.VariableName)
								{
									CalculatedOrder = true;
									break;
								}
							}
						}

						if (!CalculatedOrder)
							this.AddIfNotFound(Ref, Node.Value, OrderBy);
					}
					else
					{
						CalculatedOrder = true;
						break;
					}
				}
			}

			ChunkedList<KeyValuePair<string, int>> AdditionalFields = null;
			Dictionary<string, int> ColumnIndices = new Dictionary<string, int>();
			ScriptNode[] Columns2 = this.columns;
			bool Columns2Cloned = false;

			if (!(this.columns is null))
			{
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
							AdditionalFields = new ChunkedList<KeyValuePair<string, int>>();

						AdditionalFields.Add(new KeyValuePair<string, int>(Ref2.VariableName, i));
					}
					else if (this.columns[i] is VariableReference Ref)
						ColumnIndices[Ref.VariableName] = i;
				}
			}
			else
				c = 0;

			IDataSource Source = await this.source.GetSource(Variables);
			
			IProcessor<object> Processor;
			RecordProcessor Records;

			if (this.distinct)
				Records = new DistinctRecordProcessor(Columns2, Variables);
			else
				Records = new RecordProcessor(Columns2, Variables);

			Processor = Records;

			if (!(this.groupBy is null))
			{
				if (Top != int.MaxValue)
					Processor = new MaxCountProcessor(Processor, Top);

				if (Offset > 0)
					Processor = new OffsetProcessor(Processor, Offset);
			}

			if (CalculatedOrder)
			{
				ChunkedList<KeyValuePair<ScriptNode, bool>> Order = new ChunkedList<KeyValuePair<ScriptNode, bool>>();

				if (!(this.orderBy is null))
					Order.AddRange(this.orderBy);

				if (!(this.groupByNames is null))
				{
					foreach (ScriptNode Group in this.groupByNames)
					{
						if (!(Group is null))
							Order.Add(new KeyValuePair<ScriptNode, bool>(Group, true));
					}
				}

				Processor = new CustomOrderProcessor(Processor, Variables, Order.ToArray());
			}

			if (!(this.having is null))
				Processor = new ConditionalProcessor(Processor, Variables, this.having);

			if (!(AdditionalFields is null))
			{
				Processor = new FieldAggregatorProcessor(Processor, Variables, AdditionalFields.ToArray(),
					this.columns);
			}

			if (this.groupBy is null)
				await Source.Process(Processor, Offset, Top, this.generic, this.where, Variables, OrderBy.ToArray(), this);
			else
			{
				Processor = new GroupProcessor(Processor, Variables, this.groupBy, this.groupByNames,
					this.columns, ref this.having);

				await Source.Process(Processor, 0, int.MaxValue, this.generic, this.where, Variables, OrderBy.ToArray(), this);
			}

			if (this.selectOneObject && Records.TryGetSingleElement(out E))
				return E;

			int NrRecords = Records.Count;
			IElement[] Elements = new IElement[Columns2 is null ? NrRecords : NrRecords * c];

			i = 0;
			foreach (IElement[] Record in Records.GetRecordSet())
			{
				foreach (IElement Item in Record)
					Elements[i++] = Item;

				if (c > 0)
				{
					while (i % c != 0)
						Elements[i++] = ObjectValue.Null;
				}
			}

			if (Columns2 is null || c == 1)
			{
				c = Elements.Length;
				if (c <= 1 && !(this.columns is null) && (
					this.columns[0] is Functions.XPath ||
					(!(this.groupBy is null) && this.groupBy.Length == 0)))
				{
					return c == 0 ? ObjectValue.Null : Elements[0];
				}

				return Operators.Vectors.VectorDefinition.Encapsulate(Elements, false, this);
			}

			string[] Names = new string[c];

			foreach (KeyValuePair<string, int> P in ColumnIndices)
				Names[P.Value] = P.Key;

			i = 0;
			if (!(this.columnNames is null))
			{
				while (i < c)
				{
					ScriptNode Node = this.columnNames[i];

					if (Node is VariableReference Ref)
						Names[i++] = Ref.VariableName;
					else if (Node is Operators.Membership.NamedMember N)
						Names[i++] = N.Name;
					else
					{
						if (Node is null)
							E = null;
						else
						{
							try
							{
								E = await Node.EvaluateAsync(Variables);
							}
							catch (Exception)
							{
								E = null;
							}
						}

						if (E is null)
						{
							if (Names[i] is null)
								Names[i] = (i + 1).ToString();
						}
						else if (E.AssociatedObjectValue is string s)
							Names[i] = s;
						else
							Names[i] = Expression.ToString(E.AssociatedObjectValue);

						i++;
					}
				}
			}

			while (i < c)
			{
				if (Names[i] is null)
					Names[i] = (i + 1).ToString();

				i++;
			}

			if (this.selectOneObject && NrRecords == 1)
			{
				Dictionary<string, IElement> ObjExNihilo = new Dictionary<string, IElement>();
				bool AllNamed = true;

				for (i = 0; i < c; i++)
				{
					if (Names[i] is null)
						AllNamed = false;
					else
						ObjExNihilo[Names[i]] = Elements[i];
				}

				if (AllNamed)
					return new ObjectValue(ObjExNihilo);
			}

			ObjectMatrix Result = new ObjectMatrix(NrRecords, c, Elements)
			{
				ColumnNames = Names
			};

			return Result;
		}

		private void AddIfNotFound(VariableReference Ref, bool Ascending,
			ChunkedList<KeyValuePair<VariableReference, bool>> OrderBy)
		{
			string Name = Ref.VariableName;

			foreach (KeyValuePair<VariableReference, bool> P in OrderBy)
			{
				if (P.Key.VariableName == Name)
					return;
			}

			OrderBy.Add(new KeyValuePair<VariableReference, bool>(Ref, Ascending));
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
			int i, c;

			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order) ||
					!this.columnNames.ForAllChildNodes(Callback, State, Order) ||
					!this.groupBy.ForAllChildNodes(Callback, State, Order) ||
					!this.groupByNames.ForAllChildNodes(Callback, State, Order) ||
					!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}

				if (!(this.orderBy is null))
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, Order) ?? true))
							return false;
					}
				}
			}

			if (!this.columns.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder) ||
				!this.columnNames.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder) ||
				!this.groupBy.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder) ||
				!this.groupByNames.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder))
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

			if (!(this.having is null))
			{
				b = !Callback(this.having, out NewNode, State);
				if (!(NewNode is null))
				{
					this.having = NewNode;
					this.having.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.having.ForAllChildNodes(Callback, State, Order)))
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

			if (!(this.orderBy is null))
			{
				c = this.orderBy.Length;
				for (i = 0; i < c; i++)
				{
					ScriptNode Node = this.orderBy[i].Key;
					if (!(Node is null))
					{
						b = !Callback(Node, out NewNode, State);
						if (!(NewNode is null))
						{
							this.orderBy[i] = new KeyValuePair<ScriptNode, bool>(NewNode, this.orderBy[i].Value);
							NewNode.SetParent(this);
							Node = NewNode;
						}

						if (b || (Order == SearchMethod.TreeOrder && !Node.ForAllChildNodes(Callback, State, Order)))
							return false;
					}
				}
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order) ||
					!this.columnNames.ForAllChildNodes(Callback, State, Order) ||
					!this.groupBy.ForAllChildNodes(Callback, State, Order) ||
					!this.groupByNames.ForAllChildNodes(Callback, State, Order) ||
					!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}

				if (!(this.orderBy is null))
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, Order) ?? true))
							return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Select O &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.columnNames, O.columnNames) &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.groupBy, O.groupBy) &&
				AreEqual(this.groupByNames, O.groupByNames) &&
				AreEqual(this.top, O.top) &&
				AreEqual(this.where, O.where) &&
				AreEqual(this.having, O.having) &&
				AreEqual(this.offset, O.offset) &&
				this.distinct == O.distinct &&
				base.Equals(obj)))
			{
				return false;
			}

			if ((this.orderBy is null) ^ (O.orderBy is null))
				return false;

			if (this.orderBy is null)
				return true;

			IEnumerator e1 = this.orderBy.GetEnumerator();
			IEnumerator e2 = O.orderBy.GetEnumerator();

			while (true)
			{
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();

				if (b1 ^ b2)
					return false;

				if (!b1)
					return true;

				KeyValuePair<ScriptNode, bool> Item1 = (KeyValuePair<ScriptNode, bool>)e1.Current;
				KeyValuePair<ScriptNode, bool> Item2 = (KeyValuePair<ScriptNode, bool>)e2.Current;

				if (!Item1.Key.Equals(Item2.Key) ||
					!Item1.Value.Equals(Item2.Value))
				{
					return false;
				}
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.columnNames);
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.groupBy);
			Result ^= Result << 5 ^ GetHashCode(this.groupByNames);
			Result ^= Result << 5 ^ GetHashCode(this.top);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			Result ^= Result << 5 ^ GetHashCode(this.having);
			Result ^= Result << 5 ^ GetHashCode(this.offset);
			Result ^= Result << 5 ^ this.distinct.GetHashCode();

			if (!(this.orderBy is null))
			{
				foreach (KeyValuePair<ScriptNode, bool> P in this.orderBy)
				{
					Result ^= Result << 5 ^ P.Key.GetHashCode();
					Result ^= Result << 5 ^ P.Value.GetHashCode();
				}
			}

			return Result;
		}

	}
}
