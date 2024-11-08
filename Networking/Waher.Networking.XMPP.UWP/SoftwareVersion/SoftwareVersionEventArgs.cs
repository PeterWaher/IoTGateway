using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.SoftwareVersion
{
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
		public string Name => this.name;

		/// <summary>
		/// Version
		/// </summary>
		public string Version => this.version;

		/// <summary>
		/// OS
		/// </summary>
		public string OS => this.os;
	}
}
