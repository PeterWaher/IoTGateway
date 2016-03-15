using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// Imaginary unit.
	/// </summary>
	public class ImaginaryUnit : IConstant
	{
        /// <summary>
        /// Imaginary unit.
        /// </summary>
        public ImaginaryUnit()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "i"; }
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
			get { return i; }
		}

		private static readonly ComplexNumber i = new ComplexNumber(Complex.ImaginaryOne);
	}
}
