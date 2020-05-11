using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waher.Networking.XMPP.Concentrator;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for AddContactForm.xaml
	/// </summary>
	public partial class SelectItemDialog : Window
	{
		private readonly LocalizedString[] items;

		public SelectItemDialog(string Title, string Header, string OkTooltip, string TypeHeader, string ValueHeader, params LocalizedString[] Items)
			: base()
		{
			this.items = Items;

			InitializeComponent();

			if (this.ItemsView.View is GridView GridView)
			{
				GridView.Columns[0].Header = TypeHeader;
				GridView.Columns[1].Header = ValueHeader;
			}

			this.Title = Title;
			this.Label.Content = Header;
			this.OkButton.ToolTip = OkTooltip;

			foreach (LocalizedString Item in Items)
				this.ItemsView.Items.Add(new Item(Item));
		}

		public class Item
		{
			private LocalizedString s;

			public Item(LocalizedString String)
			{
				this.s = String;
			}

			public LocalizedString LocalizedString => this.s;
			public string Localized => this.s.Localized;
			public string Unlocalized => this.s.Unlocalized;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		public LocalizedString? SelectedItem
		{
			get
			{
				if (this.ItemsView.SelectedItem is Item Item)
					return Item.LocalizedString;
				else
					return null;
			}
		}

		private void ItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.OkButton.IsEnabled = this.ItemsView.SelectedItem != null;
		}

		private void ItemsView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (this.ItemsView.SelectedItem != null)
				this.DialogResult = true;
		}
	}
}
