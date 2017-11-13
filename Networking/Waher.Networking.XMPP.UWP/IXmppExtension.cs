using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
