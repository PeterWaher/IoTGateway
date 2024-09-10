using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Search;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Console;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSearchTests : CommunicationTests
	{
		[TestMethod]
		public void Search_Test_01_FindSearchForms()
		{
			string[] JIDs = this.SearchJIDs();

			foreach (string JID in JIDs)
				ConsoleOut.WriteLine(JID);
		}

		private string[] SearchJIDs()
		{
			this.ConnectClients();
			List<string> SupportsSearch = new();

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
			string[] JIDs = this.SearchJIDs();

			foreach (string JID in JIDs)
			{
				SearchFormEventArgs e = this.client1.SearchForm(JID, 10000);
				Print(e);
			}
		}

		private static void Print(SearchFormEventArgs e)
		{
			ConsoleOut.WriteLine("Instructions:" + e.Instructions);
			ConsoleOut.WriteLine("First Name:" + e.FirstName);
			ConsoleOut.WriteLine("Last Name:" + e.LastName);
			ConsoleOut.WriteLine("Nick Name:" + e.NickName);
			ConsoleOut.WriteLine("EMail:" + e.EMail);

			if (e.SearchForm is not null)
			{
				foreach (Field Field in e.SearchForm.Fields)
					ConsoleOut.WriteLine(Field.Var + ": " + Field.ValueString + " (" + Field.GetType().Name + ")");
			}
		}

		[TestMethod]
		public void Search_Test_03_DoSearch()
		{
			string[] JIDs = this.SearchJIDs();

			foreach (string JID in JIDs)
			{
				SearchFormEventArgs e = this.client1.SearchForm(JID, 10000);
				SearchResultEventArgs e2 = e.Search(10000);
				Print(e2);
			}
		}

		private static void Print(SearchResultEventArgs e)
		{
			foreach (Dictionary<string, string> Record in e.Records)
			{
				foreach (KeyValuePair<string, string> P in Record)
					ConsoleOut.WriteLine(P.Key + "=" + P.Value);
			}
		}

	}
}
