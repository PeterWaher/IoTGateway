using System;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// XMPP extension.
	/// </summary>
	public interface IXmppExtension : IDisposable
	{
		/// <summary>
		/// Implemented extensions.
		/// </summary>
		string[] Extensions
		{
			get;
		}
	}
}
