using System;
using System.Collections.Generic;
using System.Text;
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
		/// Constant value element.
		/// </summary>
		public Element ValueElement
		{
			get { return pi; }
		}

		private static readonly DoubleNumber pi = new DoubleNumber(Math.PI);
	}
}
