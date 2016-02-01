using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
	/// </summary>
	public class FailedDependencyException : HttpException
	{
		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		public FailedDependencyException()
			: base(424, "Failed Dependency")
		{
		}
	}
}
