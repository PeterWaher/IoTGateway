using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server detected an infinite loop while processing the request.
	/// </summary>
	public class LoopDetectedException : HttpException
	{
		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		public LoopDetectedException()
			: base(508, "Loop Detected")
		{
		}
	}
}
