using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an DROP INDEX ... ON ... statement against the object database.
	/// </summary>
	public class DropIndex : ScriptNode, IEvaluateAsync
	{
		private ScriptNode name;
		private SourceDefinition source;

		/// <summary>
		/// Executes an DROP INDEX ... ON ... statement against the object database.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <param name="Source">Source to create index in.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DropIndex(ScriptNode Name, SourceDefinition Source, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.name = Name;
			this.source = Source;
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
			IDataSource Source = await this .source.GetSource(Variables);
			string Name = InsertValues.GetName(this.name, Variables);

			if (!await Source.DropIndex(Name))
				throw new ScriptRuntimeException("Index not found.", this);

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

			if (!DepthFirst)
			{
				if (!(this.name?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is DropIndex O &&
				AreEqual(this.name, O.name) &&
				AreEqual(this.source, O.source) &&
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
			return Result;
		}

	}
}
