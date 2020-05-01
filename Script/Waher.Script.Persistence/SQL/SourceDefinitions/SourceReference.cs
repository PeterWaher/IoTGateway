using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// Direct reference to a data source.
	/// </summary>
	public class SourceReference : SourceDefinition
	{
		private ScriptNode source;
		private ScriptNode alias;

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, int Start, int Length, Expression Expression)
			: this(Source, null, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Alias">Alias definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, ScriptNode Alias, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.alias = Alias;
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override IDataSource GetSource(Variables Variables)
		{
			if (this.source is VariableReference Ref)
				return GetDataSource(Ref, Variables);
			else
				return GetDataSource(this.source.Evaluate(Variables), this.source);
		}

		private static IDataSource GetDataSource(IElement E, ScriptNode Source)
		{
			if (E.AssociatedObjectValue is Type T)
				return new TypeSource(T);
			else if (E is StringValue S)
				return new CollectionSource(S.Value);
			else if (E is IVector V)
				return new VectorSource(V, Source);
			else
				throw new ScriptRuntimeException("Data source type not supported.", Source);
		}

		private static IDataSource GetDataSource(VariableReference Source, Variables Variables)
		{
			string Name = Source.VariableName;

			if (Variables.TryGetVariable(Name, out Variable v))
				return GetDataSource(v.ValueElement, Source);

			if (Expression.TryGetConstant(Name, Variables, out IElement ValueElement))
				return GetDataSource(ValueElement, Source);

			if (Types.TryGetQualifiedNames(Name, out string[] QualifiedNames))
			{
				if (QualifiedNames.Length == 1)
				{
					Type T = Types.GetType(QualifiedNames[0]);

					if (!(T is null))
						return new TypeSource(T);
				}
			}

			return new CollectionSource(Name);
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
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			if (!(this.source is null) && !Callback(ref this.source, State))
				return false;

			if (!(this.alias is null) && !Callback(ref this.alias, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
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
			return (obj is SourceReference O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.alias, O.alias) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.alias);

			return Result;
		}

	}
}
