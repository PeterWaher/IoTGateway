using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of group elements.
	/// </summary>
	public interface IGroupElement : ISemiGroupElement
	{
		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		IGroupElement Negate();

		/// <summary>
		/// Associated Group.
		/// </summary>
		IGroup AssociatedGroup
		{
			get;
		}
	}
}
