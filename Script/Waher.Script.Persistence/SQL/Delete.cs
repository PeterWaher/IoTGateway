using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a DELETE statement against the object database.
	/// </summary>
	public class Delete : ScriptNode
	{
		private ScriptNode source;
		private ScriptNode where;

		/// <summary>
		/// Executes a DELETE statement against the object database.
		/// </summary>
		/// <param name="Source">Source to delete from.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Delete(ScriptNode Source, ScriptNode Where, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.where = Where;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E;

			E = this.source.Evaluate(Variables);
			if (!(E.AssociatedObjectValue is Type T))
				throw new ScriptRuntimeException("Type expected.", this.source);

			IEnumerator e = Select.Find(T, 0, int.MaxValue, this.where, Variables, new string[0], this);
			LinkedList<object> ToDelete = new LinkedList<object>();
			int Count = 0;

			while (e.MoveNext())
			{
				ToDelete.AddLast(e.Current);
				Count++;
			}

			Database.Delete(ToDelete);

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
				if (!this.source.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (this.where != null)
				{
					if (!this.where.ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}
			}

			if (!Callback(ref this.source, State))
				return false;

			if (this.where != null)
			{
				if (!Callback(ref this.where, State))
					return false;
			}

			if (!DepthFirst)
			{
				if (!this.source.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (this.where != null)
				{
					if (!this.where.ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Delete O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.where, O.where) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			return Result;
		}


	}
}
