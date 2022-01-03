using System;
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
		public override bool Contains(IElement Element)
		{
			return Element is Namespace;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Namespaces;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
