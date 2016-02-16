using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// Euler's number.
	/// </summary>
	public class E : IConstant
	{
		/// <summary>
		/// Euler's number.
		/// </summary>
		public E()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "e"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return null; }
		}

		/// <summary>
		/// Constant value element.
		/// </summary>
		public Element ValueElement
		{
			get { return e; }
		}

		private static readonly DoubleNumber e = new DoubleNumber(Math.E);
	}
}
