using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Constants
{
	/// <summary>
	/// Returns an array of <see cref="System.Diagnostics.PerformanceCounterCategory"/> objects.
	/// </summary>
	public class PerformanceCategories : IConstant
	{
		/// <summary>
		/// Returns an array of <see cref="System.Diagnostics.PerformanceCounterCategory"/> objects.
		/// </summary>
		public PerformanceCategories()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "PerformanceCategories"; }
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
			return new ObjectVector(PerformanceCounters.Categories);
		}
	}
}
