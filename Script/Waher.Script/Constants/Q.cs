using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// Rational Numbers.
	/// </summary>
	public class Q : IConstant
	{
		/// <summary>
		/// Rational Numbers.
		/// </summary>
		public Q()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Q"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return new string[] { "ℚ" }; }
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			return set;
		}

		private static readonly RationalNumbers set = new RationalNumbers();
	}
}
