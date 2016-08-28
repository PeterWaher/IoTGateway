using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Waher.Script
{
	/// <summary>
	/// Interface for late-bound modules loaded at runtime.
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// Starts the module.
		/// </summary>
		/// <returns>If an asynchronous start operation has been started, a wait handle is returned. This
		/// wait handle can be used to wait for the asynchronous process to finish. If no such asynchronous
		/// operation has been started, null can be returned.</returns>
		WaitHandle Start();

		/// <summary>
		/// Stops the module.
		/// </summary>
		void Stop();
	}
}
