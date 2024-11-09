using System.Windows;
using System.Windows.Controls;
using Waher.Client.WPF.Model.PubSub;
using Waher.Networking.XMPP.PubSub;

namespace Waher.Client.WPF.Dialogs.PubSub
{
	/// <summary>
	/// Interaction logic for Affiliations.xaml
	/// </summary>
	public partial class AffiliationsForm : Window
	{
		private readonly string node;

		public AffiliationsForm(string Node)
		{
			this.node = Node;

			this.InitializeComponent();
		}

		private void AffilationView_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			this.RemoveButton.IsEnabled = this.AffiliationView.SelectedIndex >= 0;
		}

		public void AddButton_Click(object Sender, RoutedEventArgs e)
		{
			AddAffiliateForm Form = new AddAffiliateForm()
			{
				Owner = this
			};
			bool? b = Form.ShowDialog();

			if (b.HasValue && b.Value)
			{
				Affiliation NewAffiliation = new Affiliation(this.node, Form.Jid.Text, AffiliationItem.FromIndex(Form.Affiliation.SelectedIndex));
				ListViewItem NewItem = new ListViewItem()
				{
					Content = new AffiliationItem(NewAffiliation)
				};

				this.AffiliationView.Items.Add(NewItem);
			}
		}

		public void RemoveButton_Click(object Sender, RoutedEventArgs e)
		{
			this.AffiliationView.Items.RemoveAt(this.AffiliationView.SelectedIndex);
		}

		public void ApplyButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		public void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

	}
}
