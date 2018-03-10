using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for node configuration callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ConfigurationEventHandler(object Sender, ConfigurationEventArgs e);

	/// <summary>
	/// Event arguments for node configuration callback events.
	/// </summary>
    public class ConfigurationEventArgs : DataFormEventArgs
    {
		private string nodeName;
		private NodeConfiguration configuration;

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
		public string NodeName
		{
			get { return this.nodeName; }
		}

		/// <summary>
		/// Node configuration.
		/// </summary>
		public NodeConfiguration Configuration
		{
			get { return this.configuration; }
		}
    }
}
