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
using Waher.Script.Operators.Assignments;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an UPDATE statement against the object database.
	/// </summary>
	public class Update : ScriptNode
	{
		private Assignment[] setOperations;
		private ScriptNode source;
		private ScriptNode where;

		/// <summary>
		/// Executes an UPDATE statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="SetOperations">Set operations to perform.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Update(ScriptNode Source, Assignment[] SetOperations, ScriptNode Where, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.setOperations = SetOperations;
			this.where = Where;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E = this.source.Evaluate(Variables);
			if (!(E.AssociatedObjectValue is Type T))
				throw new ScriptRuntimeException("Type expected.", this.source);

			IEnumerator e = Select.Find(T, 0, int.MaxValue, this.where, Variables, new string[0], this);
			LinkedList<object> ToUpdate = new LinkedList<object>();
			int Count = 0;

			while (e.MoveNext())
			{
				ObjectProperties Properties = new ObjectProperties(e.Current, Variables, false);

				foreach (Assignment SetOperation in this.setOperations)
					SetOperation.Evaluate(Properties);

				ToUpdate.AddLast(e.Current);
				Count++;
			}

			Database.Update(ToUpdate);

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

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!SetOperation.ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

				if (this.where != null)
				{
					if (!this.where.ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}
			}

			if (!Callback(ref this.source, State))
				return false;

			int i, c = this.setOperations.Length;
			for (i = 0; i < c; i++)
			{
				ScriptNode SetOperation = this.setOperations[i];

				if (!Callback(ref SetOperation, State))
					return false;

				if (SetOperation is Assignment Assignment)
					this.setOperations[i] = Assignment;
				else
					return false;
			}

			if (this.where != null)
			{
				if (!Callback(ref this.where, State))
					return false;
			}

			if (!DepthFirst)
			{
				if (!this.source.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!SetOperation.ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

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
			int i, c;

			if (!(obj is Update O &&
				AreEqual(this.source, O.source) &&
				(c = this.setOperations.Length) == O.setOperations.Length &&
				AreEqual(this.where, O.where) &&
				base.Equals(obj)))
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!AreEqual(this.setOperations[i], O.setOperations[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);

			foreach (Assignment SetOperation in this.setOperations)
				Result ^= Result << 5 ^ GetHashCode(SetOperation);

			Result ^= Result << 5 ^ GetHashCode(this.where);
			return Result;
		}


	}
}
