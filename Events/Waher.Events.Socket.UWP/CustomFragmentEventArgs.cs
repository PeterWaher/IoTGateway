using System;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Events.Socket
{
	/// <summary>
	/// Delegate for Custom Fragment event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CustomFragmentEventHandler(object Sender, CustomFragmentEventArgs e);

	/// <summary>
	/// Event arguments for custom fragment events.
	/// </summary>
	public class CustomFragmentEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for custom fragment events.
		/// </summary>
		/// <param name="Fragment">XML Fragment.</param>
		public CustomFragmentEventArgs(XmlDocument Fragment)
		{
			this.Fragment = Fragment;
		}

		/// <summary>
		/// XML Fragment.
		/// </summary>
		public XmlDocument Fragment { get; }
	}
}
