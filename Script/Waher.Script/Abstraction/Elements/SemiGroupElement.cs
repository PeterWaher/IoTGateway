using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of semigroup elements.
	/// </summary>
	public abstract class SemiGroupElement : Element, ISemiGroupElement
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
		public abstract ISemiGroupElement AddLeft(ISemiGroupElement Element);

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract ISemiGroupElement AddRight(ISemiGroupElement Element);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return this.AssociatedSemiGroup; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public abstract ISemiGroup AssociatedSemiGroup
		{
			get;
		}
	}
}
