using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of elements.
	/// </summary>
	public abstract class Element
	{
		/// <summary>
		/// Base class for all types of elements.
		/// </summary>
		public Element()
		{
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override abstract bool Equals(object obj);

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override abstract int GetHashCode();

		/// <summary>
		/// Associated Set.
		/// </summary>
		public abstract Set AssociatedSet
		{
			get;
		}
	}
}
