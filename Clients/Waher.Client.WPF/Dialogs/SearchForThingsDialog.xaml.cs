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
	public enum Operator
	{
		Equality = 0,
		NonEquality = 1,
		GreaterThan = 2,
		GreaterThanOrEqualTo = 3,
		LesserThan = 4,
		LesserThanOrEqualTo = 5,
		InRange = 6,
		NotInRange = 7,
		Wildcard = 8
	}

	public class Rule
	{
		public string Tag;
		public Operator Operator;
		public string Value;
	}

	/// <summary>
	/// Interaction logic for SearchForThingsDialog.xaml
	/// </summary>
	public partial class SearchForThingsDialog : Window
	{
		private int nrRules = 1;
		private int ruleNr = 1;

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
			string Suffix = (++this.ruleNr).ToString();

			StackPanel Panel = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(0, 0, 0, 8)
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

			StackPanel Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical
			};

			Panel2.Children.Add(new Label() { Content = "Tag:" });
			Panel2.Children.Add(Field);
			Panel.Children.Add(Panel2);

			ComboBox Operator = new ComboBox()
			{
				Name = "Operator" + Suffix,
				Width = 184,
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

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0)
			};

			Panel2.Children.Add(new Label() { Content = "Operator:" });
			Panel2.Children.Add(Operator);
			Panel.Children.Add(Panel2);

			TextBox Value = new TextBox()
			{
				Name = "Value" + Suffix,
				Width = 184,
				ToolTip = "Select value to search on."
			};

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0)
			};

			Panel2.Children.Add(new Label() { Content = "Value:" });
			Panel2.Children.Add(Value);
			Panel.Children.Add(Panel2);

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0),
				VerticalAlignment = VerticalAlignment.Center
			};

			Button Delete = new Button()
			{
				Name = "Delete" + Suffix,
				Width = 80,
				Height = 25,
				Padding = new Thickness(-10),
				Content = "Delete",
				Tag = this.ruleNr,
			};

			Delete.Click += this.Delete_Click;

			Panel2.Children.Add(Delete);
			Panel.Children.Add(Panel2);

			this.SearchFields.Children.Add(Panel);
			this.nrRules++;
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			if (this.nrRules <= 1)
			{
				MessageBox.Show(this, "At least one rule must exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Button Button = (Button)sender;
			int RuleNr = int.Parse(Button.Tag.ToString());
			StackPanel Rule = (StackPanel)(((StackPanel)Button.Parent).Parent);
			((StackPanel)Rule.Parent).Children.Remove(Rule);

			this.nrRules--;
		}

		public Rule[] GetRules()
		{
			List<Rule> Result = new List<Rule>();

			foreach (StackPanel Rule in this.SearchFields.Children)
			{
				StackPanel Panel1 = (StackPanel)Rule.Children[0];
				StackPanel Panel2 = (StackPanel)Rule.Children[1];
				StackPanel Panel3 = (StackPanel)Rule.Children[2];
				ComboBox Field = (ComboBox)Panel1.Children[1];
				ComboBox Operator = (ComboBox)Panel2.Children[1];
				TextBox Value = (TextBox)Panel3.Children[1];
				string TagName;

				TagName = Field.Text;
				foreach (ComboBoxItem Item in Field.Items)
				{
					if (TagName == Item.Content.ToString())
					{
						TagName = Item.Tag.ToString();
						break;
					}
				}

				Result.Add(new Dialogs.Rule()
				{
					Tag = TagName,
					Operator = (Operator)Operator.SelectedIndex,
					Value = Value.Text
				});
			}

			return Result.ToArray();
		}

	}
}
