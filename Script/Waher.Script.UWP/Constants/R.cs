using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Constants
{
    /// <summary>
    /// Real numbers.
    /// </summary>
    public class R : IConstant
	{
        /// <summary>
        /// Real numbers.
        /// </summary>
        public R()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "R"; }
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
		public IElement ValueElement
		{
			get { return set; }
		}

		private static readonly RealNumbers set = new RealNumbers();
	}
}
