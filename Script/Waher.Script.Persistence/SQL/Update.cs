using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
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
		private readonly Assignment[] setOperations;
		private SourceDefinition source;
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
		public Update(SourceDefinition Source, Assignment[] SetOperations, ScriptNode Where, int Start, int Length, Expression Expression)
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
			IDataSource Source = this.source.GetSource(Variables);
			IEnumerator e = Source.Find(0, int.MaxValue, this.where, Variables, new KeyValuePair<VariableReference, bool>[0], this);
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

			Source.Update(ToUpdate);

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

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!(SetOperation?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;
				}

				if (!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			ScriptNode Node = this.source;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.source)
			{
				if (Node is SourceDefinition Source2)
					this.source = Source2;
				else
					return false;
			}

			int i, c = this.setOperations.Length;
			for (i = 0; i < c; i++)
			{
				ScriptNode SetOperation = this.setOperations[i];

				if (!(SetOperation is null))
				{
					if (!Callback(ref SetOperation, State))
						return false;

					if (SetOperation is Assignment Assignment)
						this.setOperations[i] = Assignment;
					else
						return false;
				}
			}

			if (!(this.where is null) && !Callback(ref this.where, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!(SetOperation?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;
				}

				if (!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
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
