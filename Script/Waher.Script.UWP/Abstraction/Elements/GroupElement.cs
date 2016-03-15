using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of group elements.
	/// </summary>
	public abstract class GroupElement : SemiGroupElement, IGroupElement
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
		public abstract IGroupElement Negate();

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return this.AssociatedGroup; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedGroup; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public abstract IGroup AssociatedGroup
		{
			get;
		}
	}
}
