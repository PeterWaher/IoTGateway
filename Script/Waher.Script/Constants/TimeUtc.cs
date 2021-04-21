using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// Current time, UTC Coordinates.
	/// </summary>
	public class TimeUtc : IConstant
	{
		/// <summary>
		/// Current time, UTC Coordinates.
		/// </summary>
		public TimeUtc()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "TimeUtc"; }
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
			return new ObjectValue(DateTime.UtcNow.TimeOfDay);
		}

	}
}
