using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event arguments for node configuration callback events.
	/// </summary>
    public class ConfigurationEventArgs : DataFormEventArgs
    {
		private readonly string nodeName;
		private readonly NodeConfiguration configuration;

		/// <summary>
		/// Event arguments for node callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="e">IQ result event arguments.</param>
		public ConfigurationEventArgs(string NodeName, NodeConfiguration Configuration, 
			DataFormEventArgs e)
			: base(e.Form, e)
		{
			this.nodeName = NodeName;
			this.configuration = Configuration;
		}

		/// <summary>
		/// Node name.
		/// </summary>
		public string NodeName => this.nodeName;

		/// <summary>
		/// Node configuration.
		/// </summary>
		public NodeConfiguration Configuration => this.configuration;
    }
}
