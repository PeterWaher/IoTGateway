using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of semigroups.
	/// </summary>
	public abstract class SemiGroup : Set, ISemiGroup
	{
		/// <summary>
		/// Base class for all types of semigroups.
		/// </summary>
		public SemiGroup()
			: base()
		{
		}

		/// <summary>
		/// Adds two semigroup elements, if possible.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ISemiGroupElement Add(ISemiGroupElement Left, ISemiGroupElement Right)
		{
			ISemiGroupElement Result;

			Result = Left.AddRight(Right);
			if (!(Result is null))
				return Result;

			Result = Right.AddLeft(Left);
			if (!(Result is null))
				return Result;

			return null;
		}
	}
}
