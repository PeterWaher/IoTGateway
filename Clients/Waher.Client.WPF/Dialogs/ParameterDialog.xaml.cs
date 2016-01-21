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

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for ParameterDialog.xaml
	/// </summary>
	public partial class ParameterDialog : Window
	{
		private DataForm form;

		/// <summary>
		/// Interaction logic for ParameterDialog.xaml
		/// </summary>
		public ParameterDialog(DataForm Form)
		{
			InitializeComponent();
			this.form = Form;

			Panel Container = this.DialogPanel;
			TabControl TabControl = null;
			TabItem TabItem;
			StackPanel StackPanel;
			ScrollViewer ScrollViewer;

			if (Form.HasPages)
			{
				TabControl = new TabControl();
				this.DialogPanel.Children.Add(TabControl);
				DockPanel.SetDock(TabControl, Dock.Top);
			}
			else
			{
				ScrollViewer = new ScrollViewer();
				ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
				this.DialogPanel.Children.Add(ScrollViewer);
				DockPanel.SetDock(ScrollViewer, Dock.Top);

				StackPanel = new StackPanel();
				ScrollViewer.Content = StackPanel;
				Container = StackPanel;
			}

			foreach (Networking.XMPP.DataForms.Layout.Page Page in Form.Pages)
			{
				if (TabControl != null)
				{
					TabItem = new TabItem();
					TabItem.Header = Page.Label;
					TabControl.Items.Add(TabItem);

					ScrollViewer = new ScrollViewer();
					ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					TabItem.Content = ScrollViewer;

					StackPanel = new StackPanel();
					ScrollViewer.Content = StackPanel;
					Container = StackPanel;
				}

				foreach (LayoutElement Element in Page.Elements)
					this.Layout(Container, Element, Form);
			}
		}

		private void Layout(Panel Container, LayoutElement Element, DataForm Form)
		{
			if (Element is FieldReference)
				this.Layout(Container, (FieldReference)Element, Form);
			else if (Element is Networking.XMPP.DataForms.Layout.TextElement)
				this.Layout(Container, (Networking.XMPP.DataForms.Layout.TextElement)Element, Form);
			else if (Element is Networking.XMPP.DataForms.Layout.Section)
				this.Layout(Container, (Networking.XMPP.DataForms.Layout.Section)Element, Form);
			else if (Element is ReportedReference)
				this.Layout(Container, (ReportedReference)Element, Form);
		}

		private void Layout(Panel Container, Networking.XMPP.DataForms.Layout.Section Section, DataForm Form)
		{
			GroupBox GroupBox = new GroupBox();
			Container.Children.Add(GroupBox);
			GroupBox.Header = Section.Label;
			// TODO: Margins, Padding, Color, Thickness, etc.

			StackPanel StackPanel = new StackPanel();
			GroupBox.Content = StackPanel;

			foreach (LayoutElement Element in Section.Elements)
				this.Layout(Container, Element, Form);
		}

		private void Layout(Panel Container, Networking.XMPP.DataForms.Layout.TextElement TextElement, DataForm Form)
		{
			TextBlock TextBlock = new TextBlock();
			TextBlock.TextWrapping = TextWrapping.Wrap;
			// TODO: Margins

			TextBlock.Text = TextElement.Text;
			Container.Children.Add(TextBlock);
		}

		private void Layout(Panel Container, FieldReference FieldReference, DataForm Form)
		{
			Field Field = Form[FieldReference.Var];
			if (Field == null)
				return;

			if (Field is TextSingleField)
				this.Layout(Container, (TextSingleField)Field, Form);
			else if (Field is TextMultiField)
				this.Layout(Container, (TextMultiField)Field, Form);
			else if (Field is TextPrivateField)
				this.Layout(Container, (TextPrivateField)Field, Form);
			else if (Field is BooleanField)
				this.Layout(Container, (BooleanField)Field, Form);
			else if (Field is ListSingleField)
				this.Layout(Container, (ListSingleField)Field, Form);
			else if (Field is ListMultiField)
				this.Layout(Container, (ListMultiField)Field, Form);
			else if (Field is FixedField)
				this.Layout(Container, (FixedField)Field, Form);
			else if (Field is HiddenField)
				this.Layout(Container, (HiddenField)Field, Form);
			else if (Field is JidMultiField)
				this.Layout(Container, (JidMultiField)Field, Form);
			else if (Field is JidSingleField)
				this.Layout(Container, (JidSingleField)Field, Form);
			else if (Field is MediaField)
				this.Layout(Container, (MediaField)Field, Form);
		}

		private void Layout(Panel Container, BooleanField Field, DataForm Form)
		{
			CheckBox CheckBox;
			bool IsChecked;

			CheckBox = new CheckBox();
			CheckBox.Name = "Form_" + Field.Var;
			CheckBox.Content = Field.Label;
			CheckBox.Margin = new Thickness(0, 5, 0, 0);
			CheckBox.IsEnabled = !Field.ReadOnly;
			CheckBox.ToolTip = Field.Description;

			if (CommonTypes.TryParse(Field.ValueString, out IsChecked))
				CheckBox.IsChecked = IsChecked;

			// TODO: this.Required;
			// TODO: this.PostBack;
			// TODO: this.Options;
			// TODO: this.NotSame;
			// TODO: this.HasError;
			// TODO: this.Error;
			// TODO: this.DataType;
			// TODO: this.ValidationMethod;

			Container.Children.Add(CheckBox);
		}

		private void Layout(Panel Container, FixedField Field, DataForm Form)
		{
			TextBlock TextBlock = new TextBlock();
			TextBlock.TextWrapping = TextWrapping.Wrap;
			// TODO: Margins

			TextBlock.Text = Field.ValueString;
			Container.Children.Add(TextBlock);
		}

		private void Layout(Panel Container, HiddenField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, JidMultiField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, JidSingleField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, ListMultiField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, ListSingleField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, MediaField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, TextMultiField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, TextPrivateField Field, DataForm Form)
		{
			// TODO
		}

		private void Layout(Panel Container, TextSingleField Field, DataForm Form)
		{
			Label Label = new Label();
			Label.Content = Field.Label;
			Container.Children.Add(Label);

			TextBox TextBox = new TextBox();
			TextBox.Name = "Form_" + Field.Var;
			TextBox.Text = Field.ValueString;
			TextBox.IsEnabled = !Field.ReadOnly;
			TextBox.ToolTip = Field.Description;
			Container.Children.Add(TextBox);

			// TODO: this.Required;
			// TODO: this.PostBack;
			// TODO: this.Options;
			// TODO: this.NotSame;
			// TODO: this.HasError;
			// TODO: this.Error;
			// TODO: this.DataType;
			// TODO: this.ValidationMethod;
		}

		private void Layout(Panel Container, ReportedReference ReportedReference, DataForm Form)
		{
			// TODO: Include table of results.
		}

	}
}
