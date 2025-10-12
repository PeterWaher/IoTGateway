using System;
using System.Diagnostics;
using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Event arguments for the <see cref="Assert.UnauthorizedAccess"/> event.
	/// </summary>
	public class UnauthorizedAccessEventArgs : EventArgs
	{
		private readonly MethodBase method;
		private readonly Type type;
		private readonly Assembly assembly;
		private readonly StackTrace trace;

		/// <summary>
		/// Event arguments for the <see cref="Assert.UnauthorizedAccess"/> event.
		/// </summary>
		/// <param name="Frame">Frame being accessed.</param>
		/// <param name="Trace">StackTrace Trace</param>
		public UnauthorizedAccessEventArgs(FrameInformation Frame, StackTrace Trace)
			: base()
		{
			this.method = Frame.Method;
			this.type = Frame.Type;
			this.assembly = Frame.Assembly;
			this.trace = Trace;
		}

		/// <summary>
		/// Method being accessed.
		/// </summary>
		public MethodBase Method => this.method;

		/// <summary>
		/// Type on which the method is defined.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Assembly in which the type is defined.
		/// </summary>
		public Assembly Assembly => this.assembly;

		/// <summary>
		/// StackTrace Trace
		/// </summary>
		public StackTrace Trace => this.trace;
	}
}
