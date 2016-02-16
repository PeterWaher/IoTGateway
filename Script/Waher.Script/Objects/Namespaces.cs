using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Set of namespaces.
	/// </summary>
	public sealed class Namespaces : SemiGroup
	{
		private static readonly int hashCode = typeof(Namespaces).FullName.GetHashCode();

		/// <summary>
		/// Set of namespaces.
		/// </summary>
		public Namespaces()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(Element Element)
		{
			return Element is Namespace;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Namespaces;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
