using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Networking;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Maintains the set of open connections.
	/// </summary>
	public static class Connections
	{
		private const string xmlRootElement = "ClientConnections";
		private const string xmlNamespace = "http://waher.se/ClientConnections.xsd";

		private static List<TreeNode> connections = new List<TreeNode>();
		private static object synchObject = new object();
		private static bool modified = false;

		/// <summary>
		/// Adds a new connection.
		/// </summary>
		/// <param name="RootNode">Connection.</param>
		public static void Add(TreeNode RootNode)
		{
			lock (synchObject)
			{
				connections.Add(RootNode);
				modified = true;
			}
		}

		/// <summary>
		/// Removes an existing connection.
		/// </summary>
		/// <param name="RootNode">Connection.</param>
		/// <returns>If the connection was found and removed.</returns>
		public static bool Remove(TreeNode RootNode)
		{
			lock (synchObject)
			{
				if (connections.Remove(RootNode))
				{
					modified = true;
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// If the source has been changed.
		/// </summary>
		public static bool Modified
		{
			get { return modified; }
		}

		/// <summary>
		/// Saves connections to an XML file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		public static void Save(string FileName)
		{
			using (FileStream f = File.OpenWrite(FileName))
			{
				using (XmlWriter w = XmlWriter.Create(f, XML.WriterSettings(true, false)))
				{
					w.WriteStartElement(xmlRootElement, xmlNamespace);

					foreach (TreeNode RootNode in connections)
						RootNode.Write(w);

					w.WriteEndElement();
					w.Flush();
				}
			}
		}

		/// <summary>
		/// Loads the environment from an XML file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		public static void Load(string FileName)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.Load(FileName);
			XML.Validate(FileName, Doc, xmlRootElement, xmlNamespace, schema);

			New();

			foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "XmppAccount":
						connections.Add(new XmppAccountNode((XmlElement)N, null));
						break;
				}
			}
		}

		private static readonly XmlSchema schema = XML.LoadSchema("Waher.Client.WPF.Schema.ClientConnections.xsd", typeof(Connections).Assembly);

		/// <summary>
		/// Creates a new environment.
		/// </summary>
		public static void New()
		{
			// TODO
		}
	}
}
