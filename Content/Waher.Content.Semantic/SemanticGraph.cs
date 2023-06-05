using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Semantic.Model;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains triples that form a graph.
	/// </summary>
	public class SemanticGraph : ISemanticModel
	{
		private readonly LinkedList<ISemanticTriple> triples = new LinkedList<ISemanticTriple>();
		private readonly Dictionary<ISemanticElement, bool> nodes = new Dictionary<ISemanticElement, bool>();
		private ISemanticElement lastSubject = null;
		private ISemanticElement[] nodesStatic = null;

		/// <summary>
		/// Contains triples that form a graph.
		/// </summary>
		public SemanticGraph()
		{
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Triple">Triple</param>
		public void Add(ISemanticTriple Triple)
		{
			this.triples.AddLast(Triple);

			if (this.lastSubject is null || !this.lastSubject.Equals(Triple.Subject))
			{
				this.nodes[Triple.Subject] = true;
				this.lastSubject = Triple.Subject;
				this.nodesStatic = null;
			}

			if (!Triple.Object.IsLiteral &&
				Triple.Predicate is UriNode Predicate &&
				Predicate.Uri.AbsoluteUri != RdfDocument.UriRdfType.AbsoluteUri)
			{
				this.nodes[Triple.Object] = true;
				this.nodesStatic = null;
			}
		}

		/// <summary>
		/// Nodes in graph.
		/// </summary>
		public ISemanticElement[] Nodes
		{
			get
			{
				if (this.nodesStatic is null)
				{
					ISemanticElement[] Result = new ISemanticElement[this.nodes.Count];
					this.nodes.Keys.CopyTo(Result, 0);
					this.nodesStatic = Result;
				}

				return this.nodesStatic;
			}
		}

		/// <summary>
		/// Exports graph to PlantUML.
		/// </summary>
		/// <returns>PlantUML string</returns>
		public async Task<string> ExportPlantUml()
		{
			StringBuilder Output = new StringBuilder();
			await this.ExportPlantUml(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Exports graph to PlantUML.
		/// </summary>
		/// <param name="Output">PlantUML text will be output here.</param>
		public async Task ExportPlantUml(StringBuilder Output)
		{
			Output.AppendLine("@startuml");

			Dictionary<ISemanticElement, string> NodeIds = new Dictionary<ISemanticElement, string>();
			InMemorySemanticCube Cube = await InMemorySemanticCube.Create(this);
			Dictionary<string, LinkedList<LinkInfo>> LinksByNodeId = new Dictionary<string, LinkedList<LinkInfo>>();
			int i = 0;

			foreach (ISemanticElement Node in this.Nodes)
			{
				if (!NodeIds.TryGetValue(Node, out string NodeId))
				{
					NodeId = "n" + (++i).ToString();
					NodeIds[Node] = NodeId;
				}
			}

			foreach (ISemanticElement Node in this.Nodes)
			{
				string NodeId = NodeIds[Node];
				ISemanticPlane Plane = await Cube.GetTriplesBySubject(Node);
				LinkedList<KeyValuePair<string, object>> Properties = null;
				LinkedList<LinkInfo> Links = null;
				string StereoType = null;
				bool IsRdfType;

				if (!(Plane is null))
				{
					IEnumerator<ISemanticElement> Predicates = await Plane.GetXAxisEnumerator();

					while (Predicates.MoveNext())
					{
						ISemanticLine Line = await Plane.GetTriplesByX(Predicates.Current);
						string PropertyName;

						if (Predicates.Current is UriNode UriNode)
						{
							PropertyName = UriNode.ShortName;
							IsRdfType = UriNode.Uri.AbsoluteUri == RdfDocument.UriRdfType.AbsoluteUri;
						}
						else
						{
							PropertyName = Predicates.Current.ToString();
							IsRdfType = false;
						}

						if (!(Line is null))
						{
							IEnumerator<ISemanticElement> Values = await Line.GetValueEnumerator();

							while (Values.MoveNext())
							{
								if (IsRdfType && StereoType is null)
								{
									if (Values.Current is UriNode UriNode2)
										StereoType = UriNode2.ShortName;
									else
										StereoType = Values.Current.ToString();
								}
								else if (Values.Current is ISemanticLiteral Literal)
								{
									if (Properties is null)
										Properties = new LinkedList<KeyValuePair<string, object>>();

									Properties.AddLast(new KeyValuePair<string, object>(PropertyName, Literal.StringValue));
								}
								else if (NodeIds.TryGetValue(Values.Current, out string ObjectId))
								{
									if (Links is null)
										Links = new LinkedList<LinkInfo>();

									Links.AddLast(new LinkInfo()
									{
										From = NodeId,
										To = ObjectId,
										Type = Node is BlankNode ? "*--" : "-->",
										Label = PropertyName
									});
								}
								else
								{
									if (Properties is null)
										Properties = new LinkedList<KeyValuePair<string, object>>();

									string ValueString;

									if (Values.Current is UriNode UriNode2)
										ValueString = UriNode2.ShortName;
									else
										ValueString = Values.Current.ToString();

									Properties.AddLast(new KeyValuePair<string, object>(PropertyName, ValueString));
								}
							}
						}
					}
				}

				if (Properties is null)
				{
					Output.Append("object \"");

					if (Node is UriNode UriNode)
						Output.Append(JSON.Encode(UriNode.ShortName));
					else
						Output.Append(JSON.Encode(Node.ToString()));

					Output.Append("\" as ");
					Output.Append(NodeId);

					if (!string.IsNullOrEmpty(StereoType))
					{
						Output.Append("<<");
						Output.Append(StereoType);
						Output.Append(">>");
					}

					Output.AppendLine();
				}
				else
				{
					Output.Append("map \"");

					if (Node is UriNode UriNode)
						Output.Append(JSON.Encode(UriNode.ShortName));
					else
						Output.Append(JSON.Encode(Node.ToString()));

					Output.Append("\" as ");
					Output.Append(NodeId);

					if (!string.IsNullOrEmpty(StereoType))
					{
						Output.Append("<<");
						Output.Append(StereoType);
						Output.Append(">>");
					}

					Output.AppendLine(" {");

					foreach (KeyValuePair<string, object> P in Properties)
					{
						Output.Append('\t');
						Output.Append(P.Key);
						Output.Append(" => ");
						Output.AppendLine(P.Value?.ToString());
					}

					Output.AppendLine("}");
				}

				if (!(Links is null))
					LinksByNodeId[NodeId] = Links;
			}

			foreach (LinkedList<LinkInfo> Links in LinksByNodeId.Values)
			{
				foreach (LinkInfo Link in Links)
				{
					Output.Append(Link.From);
					Output.Append(' ');
					Output.Append(Link.Type);
					Output.Append(' ');
					Output.Append(Link.To);
					Output.Append(" : ");
					Output.AppendLine(Link.Label);
				}
			}

			Output.AppendLine("@enduml");
		}

		private class LinkInfo
		{
			public string From;
			public string To;
			public string Type;
			public string Label;
		}

	}
}
