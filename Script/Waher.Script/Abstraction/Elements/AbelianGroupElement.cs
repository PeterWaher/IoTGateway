using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of abelian group elements.
	/// </summary>
	public abstract class AbelianGroupElement : GroupElement
	{
		/// <summary>
		/// Base class for all types of abelian group elements.
		/// </summary>
		public AbelianGroupElement()
		{
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override SemiGroupElement AddLeft(SemiGroupElement Element)
		{
			AbelianGroupElement E = Element as AbelianGroupElement;
			if (E == null)
				return null;
			else
				return this.Add(E);
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override SemiGroupElement AddRight(SemiGroupElement Element)
		{
			AbelianGroupElement E = Element as AbelianGroupElement;
			if (E == null)
				return null;
			else
				return this.Add(E);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract AbelianGroupElement Add(AbelianGroupElement Element);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedAbelianGroup; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedAbelianGroup; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedAbelianGroup; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public abstract AbelianGroup AssociatedAbelianGroup
		{
			get;
		}
	}
}
