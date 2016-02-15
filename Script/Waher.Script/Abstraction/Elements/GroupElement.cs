using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of group elements.
	/// </summary>
	public abstract class GroupElement : SemiGroupElement
	{
		/// <summary>
		/// Base class for all types of group elements.
		/// </summary>
		public GroupElement()
		{
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public abstract GroupElement Negate();

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedGroup; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedGroup; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public abstract Group AssociatedGroup
		{
			get;
		}
	}
}
