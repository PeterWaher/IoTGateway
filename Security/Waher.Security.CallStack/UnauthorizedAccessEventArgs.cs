using System;
using System.Diagnostics;
using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Delegate for unauthorized access event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event. Might be null.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void UnauthorizedAccessEventHandler(object Sender, UnauthorizedAccessEventArgs e);

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
		/// <param name="Method">Method being accessed.</param>
		/// <param name="Type">Type on which the method is defined.</param>
		/// <param name="Assembly">Assembly in which the type is defined.</param>
		/// <param name="Trace">StackTrace Trace</param>
		public UnauthorizedAccessEventArgs(MethodBase Method, Type Type, Assembly Assembly, StackTrace Trace)
			: base()
		{
			this.method = Method;
			this.type = Type;
			this.assembly = Assembly;
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
