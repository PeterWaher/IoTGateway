using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// π
	/// </summary>
	public class Pi : IConstant
	{
		/// <summary>
		/// π
		/// </summary>
		public Pi()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "π"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return new string[] { "pi" }; }
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			return pi;
		}

		private static readonly DoubleNumber pi = new DoubleNumber(Math.PI);
	}
}
