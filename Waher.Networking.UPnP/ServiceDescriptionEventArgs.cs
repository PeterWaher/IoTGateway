using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.UPnP
{
	public delegate void ServiceDescriptionEventHandler(object Sender, ServiceDescriptionEventArgs e);

	/// <summary>
	/// Event arguments for completion events when downloading service description documents.
	/// </summary>
	public class ServiceDescriptionEventArgs
	{
		private ServiceDescriptionDocument doc;
		private Exception ex;
		private UPnPClient client;
		private object state;

		internal ServiceDescriptionEventArgs(ServiceDescriptionDocument Doc, UPnPClient Client, object State)
		{
			this.client = Client;
			this.doc = Doc;
			this.ex = null;
			this.state = State;
		}

		internal ServiceDescriptionEventArgs(Exception Ex, UPnPClient Client, object State)
		{
			this.client = Client;
			this.doc = null;
			this.ex = Ex;
			this.state = State;
		}

		/// <summary>
		/// UPnP Client.
		/// </summary>
		public UPnPClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Underlying XML Document.
		/// </summary>
		public ServiceDescriptionDocument ServiceDescriptionDocument
		{
			get { return this.doc; }
		}

		/// <summary>
		/// Exception object, if an error occurred.
		/// </summary>
		public Exception Exception
		{
			get { return this.ex; }
		}

		/// <summary>
		/// State object.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}
	}
}
