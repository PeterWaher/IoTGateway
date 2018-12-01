using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Contracts;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Questions;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model.Concentrator;

namespace Waher.Client.WPF.Model.Legal
{
	public class LegalService : XmppComponent
	{
		private ContractsClient contractsClient;

		public LegalService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
			this.contractsClient = new ContractsClient(this.Account.Client, JID);
		}

		public ContractsClient ContractsClient
		{
			get { return this.contractsClient; }
		}

		public override void Dispose()
		{
			if (this.contractsClient != null)
			{
				this.contractsClient.Dispose();
				this.contractsClient = null;
			}

			base.Dispose();
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				return "Legal Services";
			}
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			MenuItem Item;

			this.GroupSeparator(ref CurrentGroup, "Database", Menu);

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "_Register Legal Identity...",
				IsEnabled = true,
				Icon = new Image()
				{
					Source = new BitmapImage(new Uri("../Graphics/Places-user-identity-icon_16.png", UriKind.Relative)),
					Width = 16,
					Height = 16
				}
			});

			Item.Click += this.RegisterLegalIdentity_Click;

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "M_y legal identities...",
				IsEnabled = true
			});

			Item.Click += this.MyLegalIdentities_Click;
		}

		private void RegisterLegalIdentity_Click(object sender, RoutedEventArgs e)
		{
			LegalIdentityForm Form = new LegalIdentityForm
			{
				Owner = MainWindow.currentInstance
			};
			bool? Result = Form.ShowDialog();
			string s;

			if (Result.HasValue && Result.Value)
			{
				List<Property> Properties = new List<Property>();

				if (!string.IsNullOrEmpty(s = Form.FirstName.Text))
					Properties.Add(new Property("FIRST", s));

				if (!string.IsNullOrEmpty(s = Form.MiddleNames.Text))
					Properties.Add(new Property("MIDDLE", s));

				if (!string.IsNullOrEmpty(s = Form.LastName.Text))
					Properties.Add(new Property("LAST", s));

				if (!string.IsNullOrEmpty(s = Form.PersonalNumber.Text))
					Properties.Add(new Property("PNR", s));

				if (!string.IsNullOrEmpty(s = Form.Address.Text))
					Properties.Add(new Property("ADDR", s));

				if (!string.IsNullOrEmpty(s = Form.Address2.Text))
					Properties.Add(new Property("ADDR2", s));

				if (!string.IsNullOrEmpty(s = Form.PostalCode.Text))
					Properties.Add(new Property("ZIP", s));

				if (!string.IsNullOrEmpty(s = Form.Area.Text))
					Properties.Add(new Property("AREA", s));

				if (!string.IsNullOrEmpty(s = Form.City.Text))
					Properties.Add(new Property("CITY", s));

				if (!string.IsNullOrEmpty(s = Form.Region.Text))
					Properties.Add(new Property("REGION", s));

				if (!string.IsNullOrEmpty(s = Form.Country.Text))
					Properties.Add(new Property("COUNTRY", s));

				this.contractsClient.Apply(Properties.ToArray(), (sender2, e2) =>
				{
					if (!e2.Ok)
						MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to register legal identity." : e2.ErrorText);
				}, null);
			}
		}

		private void MyLegalIdentities_Click(object sender, RoutedEventArgs e)
		{
			this.contractsClient.GetLegalIdentities((sender2, e2) =>
			{
			}, null);
		}

	}
}
