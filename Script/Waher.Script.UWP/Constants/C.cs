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
    /// Complex numbers.
    /// </summary>
    public class C : IConstant
	{
        /// <summary>
        /// Complex numbers.
        /// </summary>
        public C()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "C"; }
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

		private static readonly ComplexNumbers set = new ComplexNumbers();
	}
}
