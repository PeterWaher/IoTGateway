using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Constants
{
	/// <summary>
	/// Time since the operating system was started.
	/// </summary>
	public class OsTime : IConstant
	{
		/// <summary>
		/// Time since the operating system was started.
		/// </summary>
		public OsTime()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(OsTime);

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
			return new ObjectValue(TimeSpan.FromMilliseconds(Environment.TickCount64));
		}
	}
}
