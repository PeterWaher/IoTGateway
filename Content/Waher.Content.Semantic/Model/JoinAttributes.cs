using System.Collections;
using System.Xml;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Joins attributes from two XML elements.
	/// </summary>
	public class JoinAttributes : IEnumerable
	{
		private readonly XmlAttributeCollection attributes1;
		private readonly XmlAttributeCollection attributes2;

		/// <summary>
		/// Joins attributes from two XML elements.
		/// </summary>
		/// <param name="Attributes1">First set of attributes.</param>
		/// <param name="Attributes2">Second set of attributes.</param>
		public JoinAttributes(XmlAttributeCollection Attributes1, XmlAttributeCollection Attributes2)
		{
			this.attributes1 = Attributes1;
			this.attributes2 = Attributes2;
		}

		/// <summary>
		/// Gets an enumerator over the concatentation of attributes from the two elements.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator GetEnumerator()
		{
			return new JoinedAttributesEnumerator(this.attributes1.GetEnumerator(),
				this.attributes2.GetEnumerator());
		}

		/// <summary>
		/// Enumerator over joined set of attributes.
		/// </summary>
		private class JoinedAttributesEnumerator : IEnumerator
		{
			private readonly IEnumerator enumerator1;
			private readonly IEnumerator enumerator2;
			private bool first;

			/// <summary>
			/// Enumerator over joined set of attributes.
			/// </summary>
			/// <param name="e1">Enumerator over first set of attributes.</param>
			/// <param name="e2">Enumerator over second set of attributes.</param>
			public JoinedAttributesEnumerator(IEnumerator e1, IEnumerator e2)
			{
				this.enumerator1 = e1;
				this.enumerator2 = e2;
				this.first = true;
			}

			/// <summary>
			/// Current attribute.
			/// </summary>
			public object Current
			{
				get
				{
					if (this.first)
						return (XmlAttribute)this.enumerator1.Current;
					else
						return (XmlAttribute)this.enumerator2.Current;
				}
			}

			/// <summary>
			/// Moves to next attribute.
			/// </summary>
			/// <returns>If a new attribute is available.</returns>
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
			/// <exception cref="System.NotImplementedException"></exception>
			public void Reset()
			{
				this.enumerator1.Reset();
				this.enumerator2.Reset();

				this.first = true;
			}
		}
	}
}
