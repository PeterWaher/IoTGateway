using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// A sub-query
	/// </summary>
	public class SubQueryPattern : ISparqlPattern
	{
		private SparqlQuery subQuery;

		/// <summary>
		/// A sub-query
		/// </summary>
		/// <param name="SubQuery">Subquery</param>
		public SubQueryPattern(SparqlQuery SubQuery)
		{
			this.subQuery = SubQuery;
		}

		/// <summary>
		/// If pattern is empty.
		/// </summary>
		public bool IsEmpty => false;

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="ExistingMatches">Existing matches.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public async Task<IEnumerable<Possibility>> Search(ISemanticCube Cube,
			Variables Variables, IEnumerable<Possibility> ExistingMatches, SparqlQuery Query)
		{
			IElement E = await this.subQuery.EvaluateAsync(Variables, ExistingMatches);
			if (!(E.AssociatedObjectValue is SparqlResultSet ResultSet))
				throw new ScriptRuntimeException("Subquery did not return a SPARQL Result Set.", this.subQuery);

			if (ResultSet.BooleanResult.HasValue)
				throw new ScriptRuntimeException("Subquery only returned a Boolean value.", this.subQuery);

			ChunkedList<Possibility> Result = new ChunkedList<Possibility>();

			foreach (ISparqlResultRecord Record in ResultSet.Records)
			{
				Possibility P = null;

				foreach (ISparqlResultItem Item in Record)
				{
					if (P is null)
						P = new Possibility(Item.Name, Item.Value);
					else
						P = new Possibility(Item.Name, Item.Value, P);
				}

				Result.Add(P);
			}

			return Result;
		}

		/// <summary>
		/// Sets the parent node. Can only be used when expression is being parsed or created.
		/// </summary>
		/// <param name="Parent">Parent Node</param>
		public void SetParent(ScriptNode Parent)
		{
			this.subQuery.SetParent(Parent);
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
				this.subQuery.ForAllChildNodes(Callback, State, Order);

			this.ForAll(Callback, State, Order);

			if (Order == SearchMethod.BreadthFirst)
				this.subQuery.ForAllChildNodes(Callback, State, Order);

			return true;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAll(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (!Callback(this.subQuery, out ScriptNode NewNode, State))
				return false;

			if (!(NewNode is null))
			{
				if (!(NewNode is SparqlQuery SubQuery))
					throw new Exception("Expected sub-query node.");

				this.subQuery = SubQuery;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is SubQueryPattern Typed &&
				this.subQuery.Equals(Typed.subQuery);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = typeof(SubQueryPattern).GetHashCode();
			Result ^= Result << 5 ^ this.subQuery.GetHashCode();
			return Result;
		}

	}
}
