using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waher.Content;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Runtime.Inventory;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for SearchForThingsDialog.xaml
	/// </summary>
	public partial class SearchForThingsDialog : Window
	{
		/// <summary>
		/// Interaction logic for SearchForThingsDialog.xaml
		/// </summary>
		public SearchForThingsDialog()
		{
			InitializeComponent();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			int i = this.SearchFields.Children.Count + 1;
			string Suffix = i.ToString();

			StackPanel Panel = new StackPanel()
			{
				Orientation = Orientation.Horizontal
			};

			ComboBox Field = new ComboBox()
			{
				Name = "Field" + Suffix,
				Width = 150,
				IsEditable = true,
				ToolTip = "Select field to searh on."
			};

			Field.Items.Add(new ComboBoxItem() { Tag = "ALT", Content = "Altitude (ALT)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "APT", Content = "Apartment (APT)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "AREA", Content = "Area (AREA)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "BLD", Content = "Building (BLD)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "CITY", Content = "City (CITY)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "CLASS", Content = "Class (CLASS)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "COUNTRY", Content = "Country (COUNTRY)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "LAT", Content = "Latitude (LAT)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "LONG", Content = "Longitude (LONG)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "MAN", Content = "Manufacturer (MAN)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "MLOC", Content = "Meter Location (MLOC)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "MNR", Content = "Meter Number (MNR)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "MODEL", Content = "Model (MODEL)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "NAME", Content = "Name (NAME)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "PURL", Content = "Product URL (PURL)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "REGION", Content = "Region (REGION)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "ROOM", Content = "Room (ROOM)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "SN", Content = "Serial Number (SN)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "STREET", Content = "Street (STREET)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "STREETNR", Content = "Street Number (STREETNR)" });
			Field.Items.Add(new ComboBoxItem() { Tag = "V", Content = "Version (V)" });

			Panel.Children.Add(Field);

			ComboBox Operator = new ComboBox()
			{
				Name = "Operator" + Suffix,
				Width = 200,
				IsEditable = true,
				ToolTip = "Select search operator.",
				SelectedIndex = 0
			};

			Operator.Items.Add(new ComboBoxItem() { Tag = "=", Content = "Equality (=)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "&lt;&gt;", Content = "Non-equality (&lt;&gt;)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "&gt;", Content = "Greater than (&gt;)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "&gt;=", Content = "Greater than or equal to (&gt;=)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "&lt;", Content = "Lesser than (&lt;)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "&lt;=", Content = "Lesser than or equal to (&lt;=)" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "InRange", Content = "In range" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "NotInRange", Content = "Not in range" });
			Operator.Items.Add(new ComboBoxItem() { Tag = "Wildcard", Content = "Wildcard" });

			Panel.Children.Add(Operator);

			TextBox Value = new TextBox()
			{
				Name = "Value" + Suffix,
				Width = 200,
				ToolTip = "Select value to search on."
			};

			Panel.Children.Add(Value);
		}
	}
}
