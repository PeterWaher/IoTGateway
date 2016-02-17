using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of semigroups.
	/// </summary>
	public interface ISemiGroup : ISet
	{
		/// <summary>
		/// Adds two semigroup elements, if possible.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ISemiGroupElement Add(ISemiGroupElement Left, ISemiGroupElement Right);
	}
}
