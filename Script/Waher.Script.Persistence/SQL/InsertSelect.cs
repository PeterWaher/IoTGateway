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
	public class InsertSelect : ScriptNode
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
			this.select = Select;
			this.lazy = Lazy;
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
			IDataSource Source = this.source.GetSource(Variables);
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
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.select?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			ScriptNode Node = this.source;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node is SourceDefinition Source2)
				this.source = Source2;
			else
				return false;

			Node = this.select;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node is Select Select1)
				this.select = Select1;
			else
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.select?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is InsertSelect O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.select, O.select) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.select);
			return Result;
		}

	}
}
