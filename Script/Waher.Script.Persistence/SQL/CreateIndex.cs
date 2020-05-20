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
	/// Executes an CREATE INDEX ... ON ... (...) statement against the object database.
	/// </summary>
	public class CreateIndex : ScriptNode
	{
		private ScriptNode name;
		private SourceDefinition source;
		private readonly ScriptNode[] columns;
		private readonly bool[] ascending;
		private readonly int nrColumns;

		/// <summary>
		/// Executes an CREATE INDEX ... ON ... (...) statement against the object database.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <param name="Source">Source to create index in.</param>
		/// <param name="Columns">Columns</param>
		/// <param name="Ascending">If columns are escending (true) or descending (false).</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateIndex(ScriptNode Name, SourceDefinition Source, ScriptNode[] Columns, bool[] Ascending,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.nrColumns = Columns.Length;
			if (Ascending.Length != this.nrColumns)
				throw new ArgumentException("Number of columns does not match number of ascending argument values.", nameof(Ascending));

			this.name = Name;
			this.source = Source;
			this.columns = Columns;
			this.ascending = Ascending;
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
			string Name = InsertValues.GetName(this.name, Variables);
			string[] Fields = new string[this.nrColumns];
			int i;

			for (i = 0; i < this.nrColumns; i++)
			{
				string s = InsertValues.GetName(this.columns[i], Variables);
				Fields[i] = this.ascending[i] ? s : "-" + s;
			}

			await Source.CreateIndex(Name, Fields);

			return new StringValue(Name);
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
				if (!(this.name?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
				
				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst))
					return false;
			}

			if (!(this.name is null) && !Callback(ref this.name, State))
				return false;

			ScriptNode Node = this.source;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node is SourceDefinition Source2)
				this.source = Source2;
			else
				return false;

			int i;

			for (i = 0; i < this.nrColumns; i++)
			{
				if (!(this.columns[i] is null) && !Callback(ref this.columns[i], State))
					return false;
			}

			if (!DepthFirst)
			{
				if (!(this.name?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is CreateIndex O &&
				AreEqual(this.name, O.name) &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.ascending, O.ascending) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.name);
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.ascending);
			return Result;
		}

	}
}
