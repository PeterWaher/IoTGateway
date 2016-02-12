using System;
using System.Collections.Generic;
using System.Text;

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
		void Start();

		/// <summary>
		/// Stops the module.
		/// </summary>
		void Stop();
	}
}
