using Waher.Content.Asn1.Model.Types;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for list constructs.
	/// </summary>
	public abstract class Asn1List : Asn1ComplexType
	{
		private readonly Asn1Node[] nodes;
		private Asn1Node[] nodesIncludingComponents;

		/// <summary>
		/// Abstract base class for list constructs.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1List(string Name, bool TypeDef, Asn1Node[] Nodes, Asn1Document Document)
			: base(Name, TypeDef, Document)
		{
			this.nodes = Nodes;
			this.SetAsChild(this.nodes);
		}

		/// <summary>
		/// Nodes
		/// </summary>
		public Asn1Node[] Nodes
		{
			get
			{
				if (this.nodesIncludingComponents is null)
				{
					ChunkedList<Asn1Node> Nodes = new ChunkedList<Asn1Node>();

					foreach (Asn1Node Node in this.nodes)
					{
						if (Node is Asn1ComponentsOf ComponentsOf)
							Nodes.AddRange(ComponentsOf.GetComponents());
						else
							Nodes.Add(Node);
					}

					this.nodesIncludingComponents = Nodes.ToArray();
				}

				return this.nodesIncludingComponents;
			}
		}

		/// <summary>
		/// Gets the components associated with the node.
		/// </summary>
		/// <returns>Array of components.</returns>
		public override Asn1Node[] GetComponents()
		{
			return this.nodes;
		}
	}
}
