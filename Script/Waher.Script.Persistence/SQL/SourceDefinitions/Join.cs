using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// Abstract base class for joins of two source definitions.
	/// </summary>
	public abstract class Join : SourceDefinition, IDataSource
	{
		private SourceDefinition left;
		private SourceDefinition right;
		private ScriptNode conditions;

		/// <summary>
		/// Abstract base class for joins of two source definitions.
		/// </summary>
		/// <param name="Left">Left source definition.</param>
		/// <param name="Right">Right source definition.</param>
		/// <param name="Conditions">Join conditions.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(SourceDefinition Left, SourceDefinition Right, ScriptNode Conditions, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.left = Left;
			this.right = Right;
			this.conditions = Conditions;
		}

		/// <summary>
		/// Left source
		/// </summary>
		public SourceDefinition Left => this.left;

		/// <summary>
		/// Right source
		/// </summary>
		public SourceDefinition Right => this.right;

		/// <summary>
		/// Conditions linking the two sources.
		/// </summary>
		public ScriptNode Conditions => this.conditions;

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
				if (!(this.left?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			ScriptNode Node = this.left;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.left)
			{
				if (Node is SourceDefinition Left2)
					this.left = Left2;
				else
					return false;
			}

			Node = this.right;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.right)
			{
				if (Node is SourceDefinition Right2)
					this.right = Right2;
				else
					return false;
			}

			if (!(this.conditions is null) && !Callback(ref this.conditions, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.left?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
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
			return (obj is Join O &&
				AreEqual(this.left, O.left) &&
				AreEqual(this.right, O.right) &&
				AreEqual(this.conditions, O.conditions) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.left);
			Result ^= Result << 5 ^ GetHashCode(this.right);
			Result ^= Result << 5 ^ GetHashCode(this.conditions);

			return Result;
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override sealed IDataSource GetSource(Variables Variables)
		{
			return this;
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public abstract Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node);

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		public Task Update(IEnumerable<object> Objects)
		{
			throw InvalidOperation();
		}

		private static Exception InvalidOperation()
		{
			return new InvalidOperationException("Operation not permitted on joined sources.");
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(object Object)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get => throw InvalidOperation();
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get => throw InvalidOperation();
		}

	}
}
