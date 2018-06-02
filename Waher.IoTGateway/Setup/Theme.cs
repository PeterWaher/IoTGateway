using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Theme constant.
	/// </summary>
    public class Theme : IConstant
    {
		/// <summary>
		/// Theme constant.
		/// </summary>
		public Theme()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Theme"; }
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
			get { return new StringValue("/Themes/CactusRose/CactusRose.css"); }
		}

	}
}
