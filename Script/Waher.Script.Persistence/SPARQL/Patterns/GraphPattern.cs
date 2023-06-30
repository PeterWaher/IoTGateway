using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Functions.ComplexNumbers;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// A pattern referencing a named source.
	/// </summary>
	public class GraphPattern : ISparqlPattern
	{
		private readonly ISparqlPattern pattern;
		private ScriptNode graph;

		/// <summary>
		/// A pattern referencing a named source.
		/// </summary>
		/// <param name="Graph">Graph reference</param>
		/// <param name="Pattern">Pattern</param>
		public GraphPattern(ScriptNode Graph, ISparqlPattern Pattern)
		{
			this.graph = Graph;
			this.pattern = Pattern;
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
			IElement Graph;

			if (ExistingMatches is null)
			{
				Graph = await this.graph.EvaluateAsync(Variables);
				Cube = await Query.GetNamedGraph(Graph.AssociatedObjectValue?.ToString() ?? string.Empty, Variables);
				return await this.pattern.Search(Cube, Variables, ExistingMatches, Query);
			}
			else
			{
				ObjectProperties ObjectProperties = null;
				LinkedList<Possibility> Result = new LinkedList<Possibility>();

				foreach (Possibility P in ExistingMatches)
				{
					if (ObjectProperties is null)
						ObjectProperties = new ObjectProperties(P, Variables);
					else
						ObjectProperties.Object = P;

					Graph = await this.graph.EvaluateAsync(ObjectProperties);
					Cube = await Query.GetNamedGraph(Graph.AssociatedObjectValue?.ToString() ?? string.Empty, Variables);

					if (!(Cube is null))
					{
						IEnumerable<Possibility> PartResult = await this.pattern.Search(
							Cube, ObjectProperties, new Possibility[] { P }, Query);

						if (!(PartResult is null))
						{
							foreach (Possibility P2 in PartResult)
								Result.AddLast(P2);
						}
					}
				}

				return Result;
			}
		}

		/// <summary>
		/// Sets the parent node. Can only be used when expression is being parsed.
		/// </summary>
		/// <param name="Parent">Parent Node</param>
		public void SetParent(ScriptNode Parent)
		{
			this.graph.SetParent(Parent);
			this.pattern.SetParent(Parent);
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
			{
				this.graph.ForAllChildNodes(Callback, State, Order);
				this.pattern.ForAllChildNodes(Callback, State, Order);
			}

			this.ForAll(Callback, State, Order);

			if (Order == SearchMethod.BreadthFirst)
			{
				this.graph.ForAllChildNodes(Callback, State, Order);
				this.pattern.ForAllChildNodes(Callback, State, Order);
			}

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
			if (!Callback(this.graph, out ScriptNode NewNode, State))
				return false;

			if (!(NewNode is null))
				this.graph = NewNode;

			if (!this.pattern.ForAll(Callback, State, Order))
				return false;

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is GraphPattern Typed &&
				this.graph.Equals(Typed.graph) &&
				this.pattern.Equals(Typed.pattern);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = typeof(GraphPattern).GetHashCode();
			Result ^= Result << 5 ^ this.graph.GetHashCode();
			Result ^= Result << 5 ^ this.pattern.GetHashCode();
			return Result;
		}
	}
}
