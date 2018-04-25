using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Search;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSearchTests : CommunicationTests
	{
		[TestMethod]
		public void Search_Test_01_FindSearchForms()
		{
			this.ConnectClients();
			string[] JIDs = SearchJIDs();

			foreach (string JID in JIDs)
				Console.Out.WriteLine(JID);
		}

		private string[] SearchJIDs()
		{
			this.ConnectClients();
			List<string> SupportsSearch = new List<string>();

			ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client1.Domain, 10000);
			if (e.Features.ContainsKey(XmppClient.NamespaceSearch))
				SupportsSearch.Add(this.client1.Domain);

			ServiceItemsDiscoveryEventArgs e2 = this.client1.ServiceItemsDiscovery(this.client1.Domain, 10000);
			foreach (Item Item in e2.Items)
			{
				e = this.client1.ServiceDiscovery(Item.JID, 10000);
				if (e.Features.ContainsKey(XmppClient.NamespaceSearch))
					SupportsSearch.Add(Item.JID);
			}

			return SupportsSearch.ToArray();
		}

		[TestMethod]
		public void Search_Test_02_GetSearchForms()
		{
			this.ConnectClients();
			string[] JIDs = SearchJIDs();

			foreach (string JID in JIDs)
			{
				SearchFormEventArgs e = this.client1.SearchForm(JID, 10000);
				this.Print(e);
			}
		}

		private void Print(SearchFormEventArgs e)
		{
			Console.Out.WriteLine("Instructions:" + e.Instructions);
			Console.Out.WriteLine("First Name:" + e.FirstName);
			Console.Out.WriteLine("Last Name:" + e.LastName);
			Console.Out.WriteLine("Nick Name:" + e.NickName);
			Console.Out.WriteLine("EMail:" + e.EMail);

			if (e.SearchForm != null)
			{
				foreach (Field Field in e.SearchForm.Fields)
					Console.Out.WriteLine(Field.Var + ": " + Field.ValueString + " (" + Field.GetType().Name + ")");
			}
		}

		[TestMethod]
		public void Search_Test_03_DoSearch()
		{
			this.ConnectClients();
			string[] JIDs = SearchJIDs();

			foreach (string JID in JIDs)
			{
				SearchFormEventArgs e = this.client1.SearchForm(JID, 10000);
				SearchResultEventArgs e2 = e.Search(10000);
				this.Print(e2);
			}
		}

		private void Print(SearchResultEventArgs e)
		{
			foreach (Dictionary<string, string> Record in e.Records)
			{
				foreach (KeyValuePair<string, string> P in Record)
					Console.Out.WriteLine(P.Key + "=" + P.Value);
			}
		}

	}
}
