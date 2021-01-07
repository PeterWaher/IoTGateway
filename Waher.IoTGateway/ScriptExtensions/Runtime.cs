using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions
{
	/// <summary>
	/// Returns the time elapsed since the gateway was started.
	/// </summary>
	public class Runtime : IConstant
	{
		/// <summary>
		/// Returns the time elapsed since the gateway was started.
		/// </summary>
		public Runtime()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Runtime"; }
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
			return new ObjectValue(DateTime.Now - Gateway.StartTime);
		}

	}
}
