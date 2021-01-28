using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Constants
{
	/// <summary>
	/// Domain constant, contains the value of the gateway domain.
	/// </summary>
	public class Domain : IConstant
	{
		/// <summary>
		/// Domain constant, contains the value of the gateway domain.
		/// </summary>
		public Domain()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Domain"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			return new StringValue(Gateway.Domain);
		}

	}
}
