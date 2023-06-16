using System.Collections;
using System.Xml;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Joins two sets of XML nodes.
	/// </summary>
	public class JoinNodes : IEnumerable
	{
		private readonly XmlNodeList nodes1;
		private readonly XmlNodeList nodes2;

		/// <summary>
		/// Joins two sets of XML nodes.
		/// </summary>
		/// <param name="Nodes1">First set of nodes</param>
		/// <param name="Nodes2">Second set of nodes</param>
		public JoinNodes(XmlNodeList Nodes1, XmlNodeList Nodes2)
		{
			this.nodes1 = Nodes1;
			this.nodes2 = Nodes2;
		}

		/// <summary>
		/// Gets an enumerator over the concatentation of nodes.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator GetEnumerator()
		{
			return new JoinedNodesEnumerator(this.nodes1.GetEnumerator(),
				this.nodes2.GetEnumerator());
		}

		/// <summary>
		/// Enumerator over joined set of nodes.
		/// </summary>
		private class JoinedNodesEnumerator : IEnumerator
		{
			private readonly IEnumerator enumerator1;
			private readonly IEnumerator enumerator2;
			private bool first;

			/// <summary>
			/// Enumerator over joined set of nodes.
			/// </summary>
			/// <param name="e1">Enumerator over first set of nodes.</param>
			/// <param name="e2">Enumerator over second set of nodes.</param>
			public JoinedNodesEnumerator(IEnumerator e1, IEnumerator e2)
			{
				this.enumerator1 = e1;
				this.enumerator2 = e2;
				this.first = true;
			}

			/// <summary>
			/// Current node.
			/// </summary>
			public object Current
			{
				get
				{
					if (this.first)
						return (XmlNode)this.enumerator1.Current;
					else
						return (XmlNode)this.enumerator2.Current;
				}
			}

			/// <summary>
			/// Moves to next node.
			/// </summary>
			/// <returns>If a new node is available.</returns>
			public bool MoveNext()
			{
				if (this.first)
				{
					if (this.enumerator1.MoveNext())
						return true;
					else
						this.first = false;
				}

				return this.enumerator2.MoveNext();
			}

			/// <summary>
			/// Resets the enumerator.
			/// </summary>
			public void Reset()
			{
				this.enumerator1.Reset();
				this.enumerator2.Reset();

				this.first = true;
			}
		}
	}
}
