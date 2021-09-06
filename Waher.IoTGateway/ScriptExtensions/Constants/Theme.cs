using System;
using Waher.IoTGateway.Setup;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Constants
{
	/// <summary>
	/// Theme constant.
	/// </summary>
    public class Theme : IConstant
    {
		private static ThemeDefinition currentDefinition = null;

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
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			Variables["GraphBgColor"] = currentDefinition.GraphBgColor;
			Variables["GraphFgColor"] = currentDefinition.GraphFgColor;

			return new ObjectValue(currentDefinition);
		}
		
		/// <summary>
		/// Current theme.
		/// </summary>
		public static ThemeDefinition CurrerntTheme
		{
			get => currentDefinition;
			internal set => currentDefinition = value;
		}

	}
}
