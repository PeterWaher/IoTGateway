using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
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
		private bool graphIsVariableRef;
		private string graphVariableRef;

		/// <summary>
		/// A pattern referencing a named source.
		/// </summary>
		/// <param name="Graph">Graph reference</param>
		/// <param name="Pattern">Pattern</param>
		public GraphPattern(ScriptNode Graph, ISparqlPattern Pattern)
		{
			this.graph = Graph;
			this.pattern = Pattern;

			this.CheckGraphVariableReference();
		}

		private void CheckGraphVariableReference()
		{
			if (this.graph is VariableReference Ref)
			{
				this.graphIsVariableRef = true;
				this.graphVariableRef = Ref.VariableName;
			}
			else
			{
				this.graphIsVariableRef = false;
				this.graphVariableRef = null;
			}
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
			ObjectProperties ObjectProperties = null;
			UriNode Graph;

			if (ExistingMatches is null)
			{
				if (this.graphIsVariableRef &&
					!Variables.ContainsVariable(this.graphVariableRef))
				{
					await Query.LoadUnloadedNamedGraphs(Variables);

					ChunkedList<Possibility> Result = new ChunkedList<Possibility>();

					foreach (UriNode GraphName in Query.NamedGraphNames)
					{
						Cube = await Query.GetNamedGraph(GraphName, Variables);

						if (!(Cube is null))
						{
							Possibility P = new Possibility(this.graphVariableRef, GraphName);

							if (ObjectProperties is null)
								ObjectProperties = new ObjectProperties(P, Variables);
							else
								ObjectProperties.Object = P;

							IEnumerable<Possibility> PartResult = await this.pattern.Search(
								Cube, ObjectProperties, new Possibility[] { P }, Query);

							if (!(PartResult is null))
							{
								foreach (Possibility P2 in PartResult)
									Result.Add(P2);
							}
						}
					}

					return Result;
				}
				else
				{
					Graph = Query.GetGraphName((await this.graph.EvaluateAsync(Variables)).AssociatedObjectValue);
					Cube = await Query.GetNamedGraph(Graph.AssociatedObjectValue, Variables);
					return await this.pattern.Search(Cube, Variables, ExistingMatches, Query);
				}
			}
			else
			{
				ChunkedList<KeyValuePair<Possibility, UriNode>> ToProcess = new ChunkedList<KeyValuePair<Possibility, UriNode>>();
				Dictionary<UriNode, bool> ReferencedGraphs = new Dictionary<UriNode, bool>();

				foreach (Possibility P in ExistingMatches)
				{
					if (ObjectProperties is null)
						ObjectProperties = new ObjectProperties(P, Variables);
					else
						ObjectProperties.Object = P;

					if (!this.graphIsVariableRef ||
						ObjectProperties.ContainsVariable(this.graphVariableRef))
					{
						Graph = Query.GetGraphName(await this.graph.EvaluateAsync(ObjectProperties));
						if (Graph is null)
							continue;
					}
					else
					{
						ToProcess = null;
						ReferencedGraphs = null;
						break;
					}

					ToProcess.Add(new KeyValuePair<Possibility, UriNode>(P, Graph));
					ReferencedGraphs[Graph] = true;
				}

				if (!(ReferencedGraphs is null))
				{
					UriNode[] Referenced = new UriNode[ReferencedGraphs.Count];
					ReferencedGraphs.Keys.CopyTo(Referenced, 0);

					await Query.LoadUnloadedNamedGraphs(Variables, Referenced);
				}
				else
					await Query.LoadUnloadedNamedGraphs(Variables);

				ChunkedList<Possibility> Result = new ChunkedList<Possibility>();

				if (!(ToProcess is null))
				{
					foreach (KeyValuePair<Possibility, UriNode> P in ToProcess)
					{
						if (ObjectProperties is null)
							ObjectProperties = new ObjectProperties(P.Key, Variables);
						else
							ObjectProperties.Object = P.Key;

						Cube = await Query.GetNamedGraph(P.Value, Variables);

						if (!(Cube is null))
						{
							IEnumerable<Possibility> PartResult = await this.pattern.Search(
								Cube, ObjectProperties, new Possibility[] { P.Key }, Query);

							if (!(PartResult is null))
								Result.AddRange(PartResult);
						}
					}
				}
				else
				{
					foreach (Possibility P in ExistingMatches)
					{
						if (ObjectProperties is null)
							ObjectProperties = new ObjectProperties(P, Variables);
						else
							ObjectProperties.Object = P;

						if (!this.graphIsVariableRef ||
							ObjectProperties.ContainsVariable(this.graphVariableRef))
						{
							Graph = Query.GetGraphName(await this.graph.EvaluateAsync(ObjectProperties));
							if (Graph is null)
								continue;

							Cube = await Query.GetNamedGraph(Graph, Variables);

							if (!(Cube is null))
							{
								IEnumerable<Possibility> PartResult = await this.pattern.Search(
									Cube, ObjectProperties, new Possibility[] { P }, Query);

								if (!(PartResult is null))
									Result.AddRange(PartResult);
							}
						}
						else
						{
							foreach (UriNode GraphName in Query.NamedGraphNames)
							{
								Cube = await Query.GetNamedGraph(GraphName, Variables);

								if (!(Cube is null))
								{
									IEnumerable<Possibility> PartResult = await this.pattern.Search(
										Cube, ObjectProperties, new Possibility[]
										{
											new Possibility(this.graphVariableRef, GraphName, P)
										}, Query);

									if (!(PartResult is null))
										Result.AddRange(PartResult);
								}
							}
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
			{
				this.graph = NewNode;
				this.CheckGraphVariableReference();
			}

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
