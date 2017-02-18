using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using System.Windows;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Maintains the set of open connections.
	/// </summary>
	public class Connections
	{
		private const string xmlRootElement = "ClientConnections";
		private const string xmlNamespace = "http://waher.se/ClientConnections.xsd";

		private MainWindow owner;
		private List<TreeNode> connections = new List<TreeNode>();
		private bool modified = false;

		/// <summary>
		/// Maintains the set of open connections.
		/// </summary>
		/// <param name="Owner">Owner of connections.</param>
		public Connections(MainWindow Owner)
		{
			this.owner = Owner;
		}

		/// <summary>
		/// Owner of connections.
		/// </summary>
		public MainWindow Owner
		{
			get { return this.owner; }
		}

		/// <summary>
		/// Adds a new connection.
		/// </summary>
		/// <param name="RootNode">Connection.</param>
		public void Add(TreeNode RootNode)
		{
			lock (this.connections)
			{
				this.connections.Add(RootNode);
				this.modified = true;
			}
		}

		/// <summary>
		/// Deletes a new connection.
		/// </summary>
		/// <param name="RootNode">Connection.</param>
		/// <returns>If the node was found and removed</returns>
		public bool Delete(TreeNode RootNode)
		{
			lock (this.connections)
			{
				if (this.connections.Remove(RootNode))
				{
					this.modified = true;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes an existing connection.
		/// </summary>
		/// <param name="RootNode">Connection.</param>
		/// <returns>If the connection was found and removed.</returns>
		public bool Remove(TreeNode RootNode)
		{
			lock (this.connections)
			{
				if (this.connections.Remove(RootNode))
				{
					this.modified = true;
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// If the source has been changed.
		/// </summary>
		public bool Modified
		{
			get { return this.modified; }
		}

		/// <summary>
		/// Saves connections to an XML file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		public void Save(string FileName)
		{
			lock (this.connections)
			{
				using (FileStream f = File.Create(FileName))
				{
					using (XmlWriter w = XmlWriter.Create(f, XML.WriterSettings(true, false)))
					{
						w.WriteStartElement(xmlRootElement, xmlNamespace);

						foreach (TreeNode RootNode in this.connections)
							RootNode.Write(w);

						w.WriteEndElement();
						w.Flush();
					}
				}
			}
		}

		/// <summary>
		/// Loads the environment from an XML file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		public void Load(string FileName)
		{
			XmlDocument Xml = new XmlDocument();
			Xml.Load(FileName);

			this.Load(FileName, Xml);
		}

		/// <summary>
		/// Loads the environment from an XML file.
		/// </summary>
		/// <param name="Xml">XML document.</param>
		public void Load(string FileName, XmlDocument Xml)
		{
			XML.Validate(FileName, Xml, xmlRootElement, xmlNamespace, schema);

			lock (this.connections)
			{
				this.connections.Clear();

				foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "XmppAccount":
							this.connections.Add(new XmppAccountNode((XmlElement)N, this, null));
							break;
					}
				}
			}
		}

		private static readonly XmlSchema schema = Resources.LoadSchema("Waher.Client.WPF.Schema.ClientConnections.xsd");

		/// <summary>
		/// Creates a new environment.
		/// </summary>
		public void New()
		{
			TreeNode[] ToDispose;

			lock (this.connections)
			{
				ToDispose = this.connections.ToArray();
				this.connections.Clear();
			}

			foreach (TreeNode Node in ToDispose)
				Node.Dispose();
		}

		/// <summary>
		/// Available root nodes.
		/// </summary>
		public TreeNode[] RootNodes
		{
			get
			{
				lock (this.connections)
				{
					return this.connections.ToArray();
				}
			}
		}

	}
}
