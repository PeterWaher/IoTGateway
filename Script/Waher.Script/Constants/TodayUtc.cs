using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
	/// <summary>
	/// Current date, UTC Coordinates.
	/// </summary>
	public class TodayUtc : IConstant
	{
		/// <summary>
		/// Current date, UTC Coordinates.
		/// </summary>
		public TodayUtc()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(TodayUtc);

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
			return new DateTimeValue(DateTime.UtcNow.Date);
		}

	}
}
