using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.ServiceRegistration
{
	/// <summary>
	/// XMPP-based Service Registration Client.
	/// </summary>
	public class ServiceRegistrationClient
	{
		/// <summary>
		/// http://waher.se/Schema/ServiceRegistration.xsd
		/// </summary>
		public const string NamespaceServiceRegistration = "http://waher.se/Schema/ServiceRegistration.xsd";

		private readonly XmppClient client;
		private readonly string registryJid;

		/// <summary>
		/// XMPP-based Service Registration Client.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ServiceRegistryJID">JID of the Service Registry</param>
		public ServiceRegistrationClient(XmppClient Client, string ServiceRegistryJID)
		{
			this.client = Client;
			this.registryJid = ServiceRegistryJID;
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// JID of the Service Registry.
		/// </summary>
		public string ServiceRegistryJID => this.registryJid;

		/// <summary>
		/// Checks if the software needs to be registered.
		/// </summary>
		public async Task CheckRegistration(params Annotation[] Annotations)
		{
			if (this.client.State != XmppState.Connected)
				return;

			Registration Registration = await Database.FindFirstDeleteRest<Registration>();
			SortedDictionary<string, bool> AssembliesSorted = new SortedDictionary<string, bool>();
			Assembly[] LoadedAssemblies = Types.Assemblies;
			AssemblyName AN;

			foreach (Assembly A in LoadedAssemblies)
			{
				try
				{
					AN = A.GetName();
					if (AN.Version.Major == 0 && AN.Version.Minor == 0 && AN.Version.Revision == 0 && AN.Version.Build == 0)
						continue;

					AssembliesSorted[A.FullName] = true;
				}
				catch (Exception)
				{
					continue;
				}
			}

			string[] Assemblies = new string[AssembliesSorted.Count];
			AssembliesSorted.Keys.CopyTo(Assemblies, 0);

			string BareJid = this.client.BareJID.ToLower();
			bool New;

			if (Registration is null)
			{
				Registration = new Registration()
				{
					Created = DateTime.Now
				};

				New = true;
			}
			else if (Registration.BareJid != BareJid ||
				!Equals<string>(Registration.Assemblies, Assemblies) ||
				!Equals<string>(Registration.Features, this.client.GetFeatures()) ||
				!Equals<Annotation>(Registration.Annotations, Annotations) ||
				Registration.ClientName != this.client.ClientName ||
				Registration.ClientVersion != this.client.ClientVersion ||
				Registration.ClientOS != this.client.ClientOS ||
				Registration.Language != this.client.Language ||
				Registration.Host != this.client.Host ||
				Registration.Domain != this.client.Domain)
			{
				Registration.Updated = DateTime.Now;
				New = false;
			}
			else
				return;

			this.SetValues(Registration, Assemblies, Annotations);

			if (await this.SendRegistration(Registration))
			{
				if (New)
					await Database.Insert(Registration);
				else
					await Database.Update(Registration);
			}
		}

		private Task<bool> SendRegistration(Registration Registration)
		{
			TaskCompletionSource<bool> T = new TaskCompletionSource<bool>();
			StringBuilder Xml = new StringBuilder();
			XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(false, true));

			w.WriteStartElement("register", NamespaceServiceRegistration);
			w.WriteAttributeString("jid", Registration.BareJid);
			w.WriteAttributeString("clientName", Registration.ClientName);
			w.WriteAttributeString("clientVersion", Registration.ClientVersion);
			w.WriteAttributeString("clientOS", Registration.ClientOS);
			w.WriteAttributeString("language", Registration.Language);
			w.WriteAttributeString("host", Registration.Host);
			w.WriteAttributeString("domain", Registration.Domain);
			w.WriteAttributeString("created", XML.Encode(Registration.Created));
			w.WriteAttributeString("updated", XML.Encode(Registration.Updated));

			if (Registration.Features != null)
			{
				foreach (string Feature in Registration.Features)
					w.WriteElementString("Feature", Feature);
			}

			if (Registration.Annotations != null)
			{
				foreach (Annotation Annotation in Registration.Annotations)
				{
					w.WriteStartElement("Annotation");
					w.WriteAttributeString("tag", Annotation.Tag);
					w.WriteValue(Annotation.Value);
					w.WriteEndElement();
				}
			}

			if (Registration.Assemblies != null)
			{
				foreach (string QN in Registration.Assemblies)
					w.WriteElementString("Assembly", QN);
			}

			w.WriteEndElement();
			w.Flush();

			this.client.SendIqSet(this.registryJid, Xml.ToString(), (sender, e) =>
			{
				T.SetResult(e.Ok);
				return Task.CompletedTask;
			}, null);

			return T.Task;
		}

		private void SetValues(Registration Registration, string[] Assemblies, Annotation[] Annotations)
		{
			Registration.Assemblies = Assemblies;
			Registration.BareJid = this.client.BareJID;
			Registration.Features = this.client.GetFeatures();
			Registration.Annotations = Annotations;
			Registration.ClientName = this.client.ClientName;
			Registration.ClientVersion = this.client.ClientVersion;
			Registration.ClientOS = this.client.ClientOS;
			Registration.Language = this.client.Language;
			Registration.Host = this.client.Host;
			Registration.Domain = this.client.Domain;
		}

		/// <summary>
		/// Compares two arrays
		/// </summary>
		/// <typeparam name="T">Element type</typeparam>
		/// <param name="Array1">Array 1</param>
		/// <param name="Array2">Array 2</param>
		/// <returns></returns>
		public static bool Equals<T>(T[] Array1, T[] Array2)
		{
			if ((Array1 is null) ^ (Array2 is null))
				return false;

			if (Array1 is null)
				return true;

			int i, c = Array1.Length;

			if (Array2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (!Array1[i].Equals(Array2[i]))
					return false;
			}

			return true;
		}

	}
}
