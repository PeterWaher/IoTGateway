using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
    /// <summary>
    /// Represents the smallest positive System.Double value that is greater than zero.
    /// </summary>
    public class Infinity : IConstant
	{
        /// <summary>
        /// Represents the smallest positive System.Double value that is greater than zero.
        /// </summary>
        public Infinity()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "∞"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return new string[] { "infinity", "inf" }; }
		}

		/// <summary>
		/// Constant value element.
		/// </summary>
		public IElement ValueElement
		{
			get { return infinity; }
		}

		private static readonly DoubleNumber infinity = new DoubleNumber(double.PositiveInfinity);
	}
}
