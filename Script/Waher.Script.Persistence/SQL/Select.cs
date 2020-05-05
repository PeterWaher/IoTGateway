using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a SELECT statement against the object database.
	/// </summary>
	public class Select : ScriptNode
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupByNames;
		private readonly KeyValuePair<ScriptNode, bool>[] orderBy;
		private readonly bool distinct;
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
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(ScriptNode[] Columns, ScriptNode[] ColumnNames, SourceDefinition Source, ScriptNode Where, ScriptNode[] GroupBy,
			ScriptNode[] GroupByNames, ScriptNode Having, KeyValuePair<ScriptNode, bool>[] OrderBy, ScriptNode Top, ScriptNode Offset,
			bool Distinct, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.columns = Columns;
			this.columnNames = ColumnNames;
			this.top = Top;
			this.source = Source;
			this.where = Where;
			this.groupBy = GroupBy;
			this.groupByNames = GroupByNames;
			this.having = Having;
			this.orderBy = OrderBy;
			this.offset = Offset;
			this.distinct = Distinct;

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("Columns and ColumnNames must both be null or not null.", nameof(ColumnNames));

			if (this.columns != null && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("Columns and ColumnNames must be of equal length.", nameof(ColumnNames));

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("GroupBy and GroupByNames must both be null or not null.", nameof(GroupByNames));

			if (this.columns != null && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("GroupBy and GroupByNames must be of equal length.", nameof(GroupByNames));
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
		public async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E;
			int Top;
			int Offset;
			int i, c;

			if (this.top != null)
			{
				E = this.top.Evaluate(Variables);
				Top = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Top <= 0)
					throw new ScriptRuntimeException("TOP must evaluate to a positive integer.", this.top);
			}
			else
				Top = int.MaxValue;

			if (this.offset != null)
			{
				E = this.offset.Evaluate(Variables);
				Offset = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Offset < 0)
					throw new ScriptRuntimeException("OFFSET must evaluate to a non-negative integer.", this.offset);
			}
			else
				Offset = 0;

			IDataSource Source = this.source.GetSource(Variables);

			List<KeyValuePair<VariableReference, bool>> OrderBy = new List<KeyValuePair<VariableReference, bool>>();
			bool CalculatedOrder = false;

			if (this.orderBy != null)
			{
				foreach (KeyValuePair<ScriptNode, bool> Node in this.orderBy)
				{
					if (Node.Key is VariableReference Ref)
						OrderBy.Add(new KeyValuePair<VariableReference, bool>(Ref, Node.Value));
					else
					{
						CalculatedOrder = true;
						break;
					}
				}
			}

			if (this.groupBy != null && !CalculatedOrder)
			{
				foreach (ScriptNode Node in this.groupBy)
				{
					if (Node is VariableReference Ref)
					{
						bool Found = false;

						foreach (KeyValuePair<VariableReference, bool> P in OrderBy)
						{
							if (P.Key.VariableName == Ref.VariableName)
							{
								Found = true;
								break;
							}
						}

						if (!Found)
							OrderBy.Add(new KeyValuePair<VariableReference, bool>(Ref, true));
					}
					else
					{
						CalculatedOrder = true;
						break;
					}
				}
			}

			LinkedList<IElement[]> Items = new LinkedList<IElement[]>();
			Dictionary<string, int> Columns = new Dictionary<string, int>();
			IResultSetEnumerator e;
			RecordEnumerator e2;
			int NrRecords = 0;

			if (this.columns != null)
			{
				c = this.columns.Length;
				for (i = 0; i < c; i++)
				{
					if (this.columns[i] is VariableReference Ref)
						Columns[Ref.VariableName] = i;
				}
			}
			else
				c = 0;

			if (this.groupBy is null)
			{
				e = await Source.Find(Offset, Top, this.where, Variables, OrderBy.ToArray(), this);
				Offset = 0;
				Top = int.MaxValue;
			}
			else
				e = await Source.Find(0, int.MaxValue, this.where, Variables, OrderBy.ToArray(), this);

			if (this.groupBy != null)
			{
				e = new GroupEnumerator(e, Variables, this.groupBy, this.groupByNames);

				if (this.having != null)
					e = new ConditionalEnumerator(e, Variables, this.having);
			}

			if (CalculatedOrder)
			{
				List<KeyValuePair<ScriptNode, bool>> Order = new List<KeyValuePair<ScriptNode, bool>>();

				if (this.orderBy != null)
					Order.AddRange(this.orderBy);

				if (this.groupByNames != null)
				{
					foreach (ScriptNode Group in this.groupByNames)
					{
						if (Group != null)
							Order.Add(new KeyValuePair<ScriptNode, bool>(Group, true));
					}
				}

				e = new CustomOrderEnumerator(e, Variables, Order.ToArray());
			}

			if (Offset > 0)
				e = new OffsetEnumerator(e, Offset);

			if (Top != int.MaxValue)
				e = new MaxCountEnumerator(e, Top);

			if (this.distinct)
				e2 = new DistinctEnumerator(e, this.columns, Variables);
			else
				e2 = new RecordEnumerator(e, this.columns, Variables);

			while (await e2.MoveNextAsync())
			{
				Items.AddLast(e2.CurrentRecord);
				NrRecords++;
			}

			IElement[] Elements = new IElement[this.columns is null ? NrRecords : NrRecords * c];

			i = 0;
			foreach (IElement[] Record in Items)
			{
				foreach (IElement Item in Record)
					Elements[i++] = Item;

				if (c > 0)
				{
					while (i % c != 0)
						Elements[i++] = new ObjectValue(null);
				}
			}

			if (Elements.Length == 1)
				return Elements[0];
			else if (this.columns is null)
				return Operators.Vectors.VectorDefinition.Encapsulate(Elements, false, this);

			ObjectMatrix Result = new ObjectMatrix(NrRecords, c, Elements);
			string[] Names = new string[c];

			foreach (KeyValuePair<string, int> P in Columns)
				Names[P.Value] = P.Key;

			i = 0;
			if (this.columnNames != null)
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
						try
						{
							E = this.columnNames[i]?.Evaluate(Variables);
						}
						catch (Exception)
						{
							E = null;
						}

						if (E is null)
						{
							if (Names[i] is null)
								Names[i] = (i + 1).ToString();
						}
						else if (E is StringValue S)
							Names[i] = S.Value;
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

			Result.ColumnNames = Names;

			return Result;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			int i, c;

			if (DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst) ||
					!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}

				if (this.orderBy != null)
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
							return false;
					}
				}
			}

			if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst))
			{
				return false;
			}

			ScriptNode Node = this.source;
			if (!(this.source is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.source)
			{
				if (Node is SourceDefinition SourceDef2)
					this.source = SourceDef2;
				else
					return false;
			}

			if (!(this.top is null) && !Callback(ref this.top, State))
				return false;

			if (!(this.where is null) && !Callback(ref this.where, State))
				return false;

			if (!(this.having is null) && !Callback(ref this.having, State))
				return false;

			if (!(this.offset is null) && !Callback(ref this.offset, State))
				return false;

			if (this.orderBy != null)
			{
				c = this.orderBy.Length;
				for (i = 0; i < c; i++)
				{
					Node = this.orderBy[i].Key;
					if (!(Node is null))
					{
						ScriptNode Node0 = Node;

						if (!Callback(ref Node, State))
							return false;

						if (Node != Node0)
							this.orderBy[i] = new KeyValuePair<ScriptNode, bool>(Node, this.orderBy[i].Value);
					}
				}
			}

			if (!DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst) ||
					!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.top?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}

				if (this.orderBy != null)
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
							return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
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

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
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

			foreach (KeyValuePair<ScriptNode, bool> P in this.orderBy)
			{
				Result ^= Result << 5 ^ P.Key.GetHashCode();
				Result ^= Result << 5 ^ P.Value.GetHashCode();
			}

			return Result;
		}

	}
}
