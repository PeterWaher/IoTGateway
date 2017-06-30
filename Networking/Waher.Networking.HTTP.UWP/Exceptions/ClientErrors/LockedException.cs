using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The resource that is being accessed is locked.
	/// </summary>
	public class LockedException : HttpException
	{
		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		public LockedException()
			: base(423, "Locked")
		{
		}
	}
}
