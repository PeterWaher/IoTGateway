﻿using System;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an INSERT ... VALUES ... statement against the object database.
	/// </summary>
	public class InsertValues : ScriptNode, IEvaluateAsync
	{
		private SourceDefinition source;
		private ElementList columns;
		private ElementList values;
		private readonly int nrFields;
		private readonly bool lazy;

		/// <summary>
		/// Executes an INSERT ... VALUES ... statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="Columns">Columns</param>
		/// <param name="Values">Values</param>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public InsertValues(SourceDefinition Source, ElementList Columns, ElementList Values, bool Lazy, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			if ((this.nrFields = Columns.Elements.Length) != Values.Elements.Length)
				throw new ArgumentException("Column and Value lists must have the same lengths.", nameof(Values));

			this.source = Source;
			this.source?.SetParent(this);

			this.columns = Columns;
			this.columns?.SetParent(this);

			this.values = Values;
			this.values?.SetParent(this);

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
			GenericObject Obj = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);
			ScriptNode Node;
			IElement E;
			string Column;
			int i;

			for (i = 0; i < this.nrFields; i++)
			{
				Column = await GetNameAsync(this.columns.Elements[i], Variables);

				Node = this.values.Elements[i];
				E = await Node.EvaluateAsync(Variables);

				Obj[Column] = E.AssociatedObjectValue;
			}

			await Source.Insert(this.lazy, Obj);

			return new ObjectValue(Obj);
		}

		/// <summary>
		/// Gets a name from a script node, either by using the name of a variable reference, or evaluating the node to a string.
		/// </summary>
		/// <param name="Node">Node</param>
		/// <param name="Variables">Variables</param>
		/// <returns>Name</returns>
		/// <exception cref="ScriptRuntimeException">If node is not a variable reference, or does not evaluate to a string value.</exception>
		public static async Task<string> GetNameAsync(ScriptNode Node, Variables Variables)
		{
			if (Node is VariableReference Ref)
				return Ref.VariableName;
			else
			{
				IElement E = await Node.EvaluateAsync(Variables);
				if (E.AssociatedObjectValue is string s)
					return s;
				else
					throw new ScriptRuntimeException("Exepected variable reference or string value.", Node);
			}
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

				if (!(this.columns?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.values?.ForAllChildNodes(Callback, State, Order) ?? true))
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

			if (!(this.columns is null))
			{
				b = !Callback(this.columns, out NewNode, State);
				if (!(NewNode is null) && NewNode is ElementList NewColumns)
				{
					this.columns = NewColumns;
					this.columns.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.columns.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.values is null))
			{
				b = !Callback(this.values, out NewNode, State);
				if (!(NewNode is null) && NewNode is ElementList NewValues)
				{
					this.values = NewValues;
					this.values.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.values.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.columns?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.values?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is InsertValues O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.values, O.values) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
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
