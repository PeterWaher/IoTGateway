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
    public class Eps : IConstant
	{
        /// <summary>
        /// Represents the smallest positive System.Double value that is greater than zero.
        /// </summary>
        public Eps()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "ε"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return new string[] { "eps", "epsilon" }; }
		}

		/// <summary>
		/// Constant value element.
		/// </summary>
		public IElement ValueElement
		{
			get { return epsilon; }
		}

		private static readonly DoubleNumber epsilon = new DoubleNumber(double.Epsilon);
	}
}
