using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of semigroups.
	/// </summary>
	public abstract class SemiGroup : Set
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
		public virtual SemiGroupElement Add(SemiGroupElement Left, SemiGroupElement Right)
		{
			SemiGroupElement Result;

			Result = Left.AddRight(Right);
			if (Result != null)
				return Result;

			Result = Right.AddLeft(Left);
			if (Result != null)
				return Result;

			return null;
		}
	}
}
