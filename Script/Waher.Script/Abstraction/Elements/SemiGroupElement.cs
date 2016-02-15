using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of semigroup elements.
	/// </summary>
	public abstract class SemiGroupElement : Element
	{
		/// <summary>
		/// Base class for all types of semigroup elements.
		/// </summary>
		public SemiGroupElement()
		{
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract SemiGroupElement AddLeft(SemiGroupElement Element);

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract SemiGroupElement AddRight(SemiGroupElement Element);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedSemiGroup; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public abstract SemiGroup AssociatedSemiGroup
		{
			get;
		}
	}
}
