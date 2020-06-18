using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.SoftwareVersion
{
	/// <summary>
	/// Delegate for software version events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task SoftwareVersionEventHandler(object Sender, SoftwareVersionEventArgs e);

	/// <summary>
	/// Event arguments for software version responses.
	/// </summary>
	public class SoftwareVersionEventArgs : IqResultEventArgs
	{
		private readonly string name;
		private readonly string version;
		private readonly string os;

		internal SoftwareVersionEventArgs(IqResultEventArgs e, string Name, string Version, string OS)
			: base(e)
		{
			this.name = Name;
			this.version = Version;
			this.os = OS;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// Version
		/// </summary>
		public string Version { get { return this.version; } }

		/// <summary>
		/// OS
		/// </summary>
		public string OS { get { return this.os; } }
	}
}
