using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Waher.Client.WPF.Dialogs.IoT
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
		Wildcard = 8,
		RegularExpression = 9
	}

	public class Rule
	{
		public string Tag;
		public Operator Operator;
		public string Value1;
		public string Value2;
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

			this.Field1.Focus();
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
			Operator.Items.Add(new ComboBoxItem() { Tag = "RegularExpression", Content = "Regular Expression" });

			Operator.SelectionChanged += this.Operator_SelectionChanged;

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0)
			};

			Panel2.Children.Add(new Label() { Content = "Operator:" });
			Panel2.Children.Add(Operator);
			Panel.Children.Add(Panel2);

			TextBox Value1 = new TextBox()
			{
				Name = "Value" + Suffix + "_1",
				Width = 184,
				ToolTip = "Select value to search on.",
				Height = this.Operator1.ActualHeight
			};

			TextBox Value2 = new TextBox()
			{
				Name = "Value" + Suffix + "_2",
				Width = 84,
				ToolTip = "Select value to search to.",
				Height = this.Operator1.ActualHeight
			};

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0)
			};

			Panel.Children.Add(Panel2);

			StackPanel Panel3 = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				Width = 184
			};

			Panel2.Children.Add(Panel3);

			StackPanel Panel4 = new StackPanel()
			{
				Orientation = Orientation.Vertical
			};

			Panel3.Children.Add(Panel4);

			Panel4.Children.Add(new Label() { Content = "Value:" });
			Panel4.Children.Add(Value1);

			Panel4 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Visibility = Visibility.Hidden,
				Margin = new Thickness(16, 0, 0, 0)
			};

			Panel3.Children.Add(Panel4);

			Panel4.Children.Add(new Label() { Content = "To:" });
			Panel4.Children.Add(Value2);

			Panel2 = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				Margin = new Thickness(16, 0, 0, 0),
				VerticalAlignment = VerticalAlignment.Bottom
			};

			Button Delete = new Button()
			{
				Name = "Delete" + Suffix,
				Width = 80,
				Height = this.Operator1.ActualHeight,
				Padding = new Thickness(-10),
				Content = "Delete",
				Tag = this.ruleNr
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
				StackPanel Panel3_1 = (StackPanel)Panel3.Children[0];
				StackPanel Panel3_1_1 = (StackPanel)Panel3_1.Children[0];
				StackPanel Panel3_1_2 = (StackPanel)Panel3_1.Children[1];
				ComboBox Field = (ComboBox)Panel1.Children[1];
				ComboBox Operator = (ComboBox)Panel2.Children[1];
				TextBox Value1 = (TextBox)Panel3_1_1.Children[1];
				TextBox Value2 = (TextBox)Panel3_1_2.Children[1];
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

				Result.Add(new Dialogs.IoT.Rule()
				{
					Tag = TagName,
					Operator = (Operator)Operator.SelectedIndex,
					Value1 = Value1.Text,
					Value2 = Value2.Text
				});
			}

			return Result.ToArray();
		}

		private void Operator_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox Op = (ComboBox)sender;
			Operator Op2 = (Operator)Op.SelectedIndex;
			StackPanel Panel = (StackPanel)(Op.Parent);
			StackPanel Rule = (StackPanel)(Panel.Parent);
			if (Rule.Children.Count < 3)
				return;	// Building form.

			StackPanel Value = (StackPanel)(Rule.Children[2]);
			StackPanel Values = (StackPanel)(Value.Children[0]);
			StackPanel Value1Panel = (StackPanel)(Values.Children[0]);
			StackPanel Value2Panel = (StackPanel)(Values.Children[1]);
			Label Value1Label = (Label)(Value1Panel.Children[0]);
			TextBox Value1 = (TextBox)(Value1Panel.Children[1]);

			if (Op2 == Operator.InRange || Op2 == Operator.NotInRange)
			{
				Value1Label.Content = "From:";
				Value1.Width = 84;
				Value1.ToolTip = "Select value to search from.";
				Value2Panel.Visibility = Visibility.Visible;
			}
			else
			{
				Value1Label.Content = "Value:";
				Value1.Width = 184;
				Value2Panel.Visibility = Visibility.Hidden;

				switch (Op2)
				{
					case Operator.Wildcard:
						Value1.ToolTip = "Select value to search on. Use asterisks (*) as wildcards.";
						break;

					case Operator.RegularExpression:
						Value1.ToolTip = "Select regular expression to search on.";
						break;

					default:
						Value1.ToolTip = "Select value to search on.";
						break;
				}
			}
		}
	}
}
