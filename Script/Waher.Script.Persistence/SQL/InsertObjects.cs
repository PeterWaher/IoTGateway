using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an INSERT ... OBJECT[S] ... statement against the object database.
	/// </summary>
	public class InsertObjects : ScriptNode
	{
		private SourceDefinition source;
		private ElementList objects;

		/// <summary>
		/// Executes an INSERT ... OBJECT[S] ... statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="Objects">Objects</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public InsertObjects(SourceDefinition Source, ElementList Objects, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.objects = Objects;
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
			IEnumerable<IElement> Objects;
			IElement E;
			long Count = 0;
			object Item;

			await Database.StartBulk();
			try
			{
				foreach (ScriptNode Object in this.objects.Elements)
				{
					E = Object.Evaluate(Variables);
					if (E is IVector V)
						Objects = V.ChildElements;
					else if (E is ISet S)
						Objects = S.ChildElements;
					else
						Objects = new IElement[] { E };

					foreach (IElement E2 in Objects)
					{
						Item = E2.AssociatedObjectValue;

						if (Item is Dictionary<string, IElement> ObjExNihilo)
						{
							GenericObject Obj2 = new GenericObject(Source.CollectionName, Source.TypeName, Guid.Empty);

							foreach (KeyValuePair<string, IElement> P in ObjExNihilo)
								Obj2[P.Key] = P.Value.AssociatedObjectValue;

							Item = Obj2;
						}

						await Source.Insert(Item);
						Count++;
					}
				}
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

				if (!(this.objects?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			ScriptNode Node = this.source;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node is SourceDefinition Source2)
				this.source = Source2;
			else
				return false;

			Node = this.objects;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node is ElementList List1)
				this.objects = List1;
			else
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.objects?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is InsertObjects O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.objects, O.objects) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.objects);
			return Result;
		}

	}
}
