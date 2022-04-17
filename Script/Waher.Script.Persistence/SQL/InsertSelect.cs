using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an INSERT SELECT statement against the object database.
	/// </summary>
	public class InsertSelect : ScriptNode, IEvaluateAsync
	{
		private SourceDefinition source;
		private Select select;
		private readonly bool lazy;

		/// <summary>
		/// Executes an INSERT SELECT statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="Select">SELECT statement.</param>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public InsertSelect(SourceDefinition Source, Select Select, bool Lazy, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.source?.SetParent(this);

			this.select = Select;
			this.select?.SetParent(this);

			this.lazy = Lazy;
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
			IDataSource Source = await this.source.GetSource(Variables);
			IElement E = await this.select.EvaluateAsync(Variables);
			long Count = 0;

			await Database.StartBulk();
			try
			{
				if (E is ObjectMatrix M)
				{
					string[] ColumnNames = M.ColumnNames;
					int NrColumns = ColumnNames.Length;
					int NrRows = M.Rows;
					int RowIndex;
					int ColIndex;

					for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
					{
						E = M.GetRow(RowIndex);

						if (E is IVector V)
						{
							GenericObject Obj = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);

							ColIndex = 0;
							foreach (IElement E2 in V.ChildElements)
								Obj[ColumnNames[ColIndex++]] = E2.AssociatedObjectValue;

							await Source.Insert(this.lazy, Obj);
						}
						else
							await Source.Insert(this.lazy, E.AssociatedObjectValue);

						Count++;
					}
				}
				else if (E is IVector V)
				{
					foreach (IElement Obj in E.ChildElements)
					{
						object Item = Obj.AssociatedObjectValue;

						if (Item is Dictionary<string, IElement> ObjExNihilo)
						{
							GenericObject Obj2 = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);

							foreach (KeyValuePair<string, IElement> P in ObjExNihilo)
								Obj2[P.Key] = P.Value.AssociatedObjectValue;

							Item = Obj2;
						}
						else if (Item is Dictionary<string, object> ObjExNihilo2)
						{
							GenericObject Obj2 = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);

							foreach (KeyValuePair<string, object> P in ObjExNihilo2)
								Obj2[P.Key] = P.Value;

							Item = Obj2;
						}

						await Source.Insert(this.lazy, Item);
						Count++;
					}
				}
				else
					throw new ScriptRuntimeException("Unexpected response from SELECT statement.", this.select);
			}
			finally
			{
				await Database.EndBulk();
			}

			return new DoubleNumber(Count);
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
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.select?.ForAllChildNodes(Callback, State, Order) ?? true))
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

			if (!(this.select is null))
			{
				b = !Callback(this.select, out NewNode, State);
				if (!(NewNode is null) && NewNode is Select NewSelect)
				{
					this.select = NewSelect;
					this.select.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.select.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.select?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is InsertSelect O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.select, O.select) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.select);
			return Result;
		}

	}
}
