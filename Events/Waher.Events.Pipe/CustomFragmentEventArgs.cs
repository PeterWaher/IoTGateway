using System;
using System.Xml;

namespace Waher.Events.Pipe
{
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
