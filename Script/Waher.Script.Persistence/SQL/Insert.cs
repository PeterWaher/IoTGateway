using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an INSERT statement against the object database.
	/// </summary>
	public class Insert : ScriptNode
	{
		private SourceDefinition source;
		private ElementList columns;
		private ElementList values;
		private readonly int nrFields;

		/// <summary>
		/// Executes an INSERT statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="Columns">Columns</param>
		/// <param name="Values">Values</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Insert(SourceDefinition Source, ElementList Columns, ElementList Values, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			if ((this.nrFields = Columns.Elements.Length) != Values.Elements.Length)
				throw new ArgumentException("Column and Value lists must have the same lengths.", nameof(Values));

			this.source = Source;
			this.columns = Columns;
			this.values = Values;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IDataSource Source = this.source.GetSource(Variables);
			GenericObject Obj = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);
			ScriptNode Node;
			IElement E;
			string Column;
			int i;

			for (i = 0; i < this.nrFields; i++)
			{
				Node = this.columns.Elements[i];

				if (Node is VariableReference Ref)
					Column = Ref.VariableName;
				else
				{
					E = Node.Evaluate(Variables);
					if (E.AssociatedObjectValue is string s)
						Column = s;
					else
						throw new ScriptRuntimeException("Column names must be strings.", Node);
				}

				Node = this.values.Elements[i];
				E = Node.Evaluate(Variables);

				Obj[Column] = E.AssociatedObjectValue;
			}

			Source.Insert(Obj);

			return new ObjectValue(Obj);
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

				if (!(this.columns?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.values?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
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

			ScriptNode Temp;

			Temp = this.columns;
			if (!(Temp is null) && !Callback(ref Temp, State))
				return false;

			if (Temp is ElementList List1)
				this.columns = List1;
			else
				return false;

			Temp = this.values;
			if (!(Temp is null) && !Callback(ref Temp, State))
				return false;

			if (Temp is ElementList List2)
				this.values = List2;
			else
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.columns?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.values?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is Insert O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.values, O.values) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.values);
			return Result;
		}


	}
}
