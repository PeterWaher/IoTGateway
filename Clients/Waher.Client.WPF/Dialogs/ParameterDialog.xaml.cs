using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Waher.Client.WPF.Dialogs.AvalonExtensions;
using Waher.Content;
using Waher.Content.Text;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Runtime.Inventory;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for ParameterDialog.xaml
	/// </summary>
	public partial class ParameterDialog : Window
	{
		private readonly DataForm form;
		private FrameworkElement makeVisible = null;
		private bool empty = true;

		private ParameterDialog(DataForm Form)
		{
			this.form = Form;
			this.InitializeComponent();
		}

		/// <summary>
		/// If the dialog is empty.
		/// </summary>
		public bool Empty => this.empty;

		/// <summary>
		/// Interaction logic for ParameterDialog.xaml
		/// </summary>
		public static async Task<ParameterDialog> CreateAsync(DataForm Form)
		{
			ParameterDialog Result = new(Form)
			{
				Title = Form.Title
			};

			Panel Container = Result.DialogPanel;
			TabControl TabControl = null;
			TabItem TabItem;
			StackPanel StackPanel;
			ScrollViewer ScrollViewer;
			Control First = null;
			Control Control;

			if (Form.HasPages)
			{
				TabControl = new TabControl();
				Result.DialogPanel.Children.Add(TabControl);
				DockPanel.SetDock(TabControl, Dock.Top);
			}
			else
			{
				ScrollViewer = new ScrollViewer()
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto
				};

				Result.DialogPanel.Children.Add(ScrollViewer);
				DockPanel.SetDock(ScrollViewer, Dock.Top);

				StackPanel = new StackPanel()
				{
					Margin = new Thickness(10, 10, 10, 10),
				};

				ScrollViewer.Content = StackPanel;
				Container = StackPanel;
			}

			foreach (Networking.XMPP.DataForms.Layout.Page Page in Form.Pages)
			{
				if (TabControl is not null)
				{
					TabItem = new TabItem()
					{
						Header = Page.Label
					};

					TabControl.Items.Add(TabItem);

					ScrollViewer = new ScrollViewer()
					{
						VerticalScrollBarVisibility = ScrollBarVisibility.Auto
					};

					TabItem.Content = ScrollViewer;

					StackPanel = new StackPanel()
					{
						Margin = new Thickness(10, 10, 10, 10)
					};

					ScrollViewer.Content = StackPanel;
					Container = StackPanel;
				}
				else
					TabItem = null;

				if (Form.Instructions is not null && Form.Instructions.Length > 0)
				{
					foreach (string Row in Form.Instructions)
					{
						TextBlock TextBlock = new()
						{
							TextWrapping = TextWrapping.Wrap,
							Margin = new Thickness(0, 5, 0, 5),
							Text = Row
						};

						Container.Children.Add(TextBlock);
						Result.empty = false;
					}
				}

				foreach (LayoutElement Element in Page.Elements)
				{
					Control = await Result.Layout(Container, Element, Form);
					First ??= Control;
				}

				if (TabControl is not null && TabControl.Items.Count == 1)
					TabItem.Focus();
			}

			First?.Focus();

			Result.CheckOkButtonEnabled();

			return Result;
		}

		private async Task<Control> Layout(Panel Container, LayoutElement Element, DataForm Form)
		{
			if (Element is FieldReference FieldReference)
			{
				this.empty = false;
				return await this.Layout(Container, FieldReference, Form);
			}
			else if (Element is Networking.XMPP.DataForms.Layout.TextElement TextElement)
			{
				this.empty = false;
				Layout(Container, TextElement, Form);
			}
			else if (Element is Networking.XMPP.DataForms.Layout.Section Section)
			{
				this.empty = false;
				return await this.Layout(Container, Section, Form);
			}
			else if (Element is ReportedReference ReportedReference)
			{
				if (Layout(Container, ReportedReference, Form))
					this.empty = false;
			}

			return null;
		}

		private async Task<Control> Layout(Panel Container, Networking.XMPP.DataForms.Layout.Section Section, DataForm Form)
		{
			GroupBox GroupBox = new();
			Container.Children.Add(GroupBox);
			GroupBox.Header = Section.Label;
			GroupBox.Margin = new Thickness(5, 5, 5, 5);

			StackPanel StackPanel = new();
			GroupBox.Content = StackPanel;
			StackPanel.Margin = new Thickness(5, 5, 5, 5);

			Control First = null;
			Control Control;

			foreach (LayoutElement Element in Section.Elements)
			{
				Control = await this.Layout(StackPanel, Element, Form);
				First ??= Control;
			}

			return First;
		}

		private static void Layout(Panel Container, Networking.XMPP.DataForms.Layout.TextElement TextElement, DataForm _)
		{
			TextBlock TextBlock = new()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 0, 0, 5),
				Text = TextElement.Text
			};

			Container.Children.Add(TextBlock);
		}

		private async Task<Control> Layout(Panel Container, FieldReference FieldReference, DataForm Form)
		{
			Field Field = Form[FieldReference.Var];
			if (Field is null)
				return null;

			Control Result = null;
			bool MakeVisible = false;

			if (Field.HasError)
				MakeVisible = true;
			else
				Field.Validate(Field.ValueStrings);

			if (Field is TextSingleField TextSingleField)
				Result = this.Layout(Container, TextSingleField, Form);
			else if (Field is TextMultiField TextMultiField)
				Result = this.Layout(Container, TextMultiField, Form);
			else if (Field is TextPrivateField TextPrivateField)
				Result = this.Layout(Container, TextPrivateField, Form);
			else if (Field is BooleanField BooleanField)
				Result = this.Layout(Container, BooleanField, Form);
			else if (Field is ListSingleField ListSingleField)
				Result = this.Layout(Container, ListSingleField, Form);
			else if (Field is ListMultiField ListMultiField)
				Result = this.Layout(Container, ListMultiField, Form);
			else if (Field is FixedField FixedField)
				Layout(Container, FixedField, Form);
			else if (Field is JidMultiField JidMultiField)
				Result = this.Layout(Container, JidMultiField, Form);
			else if (Field is JidSingleField JidSingleField)
				Result = this.Layout(Container, JidSingleField, Form);
			else if (Field is MediaField MediaField)
				await this.Layout(Container, MediaField, Form);

			if (MakeVisible && this.makeVisible is null)
				this.makeVisible = Result;

			return Result;
		}

		private CheckBox Layout(Panel Container, BooleanField Field, DataForm _)
		{
			TextBlock TextBlock = new()
			{
				TextWrapping = TextWrapping.Wrap,
				Text = Field.Label
			};

			if (Field.Required)
			{
				Run Run = new("*");
				TextBlock.Inlines.Add(Run);
				Run.Foreground = new SolidColorBrush(Colors.Red);
			}

			CheckBox CheckBox;

			CheckBox = new CheckBox()
			{
				Name = VarToName(Field.Var),
				Content = TextBlock,
				Margin = new Thickness(0, 3, 0, 3),
				IsEnabled = !Field.ReadOnly,
				ToolTip = Field.Description
			};

			if (!CommonTypes.TryParse(Field.ValueString, out bool IsChecked))
				CheckBox.IsChecked = null;
			else
				CheckBox.IsChecked = IsChecked;

			if (Field.HasError)
				CheckBox.Background = new SolidColorBrush(Colors.PeachPuff);
			else if (Field.NotSame)
				CheckBox.Background = new SolidColorBrush(Colors.LightGray);

			CheckBox.Click += this.CheckBox_Click;

			Container.Children.Add(CheckBox);

			return CheckBox;
		}

		private async void CheckBox_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				if (Sender is not CheckBox CheckBox)
					return;

				string Var = NameToVar(CheckBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				if (CheckBox.IsChecked.HasValue)
				{
					CheckBox.Background = null;
					await Field.SetValue(CommonTypes.Encode(CheckBox.IsChecked.Value));
					this.CheckOkButtonEnabled();
				}
				else
				{
					CheckBox.Background = new SolidColorBrush(Colors.LightGray);
					await Field.SetValue(string.Empty);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void CheckOkButtonEnabled()
		{
			foreach (Field Field in this.form.Fields)
			{
				if (!Field.ReadOnly && Field.HasError)
				{
					this.OkButton.IsEnabled = false;
					return;
				}

				if (Field.Required && string.IsNullOrEmpty(Field.ValueString))
				{
					this.OkButton.IsEnabled = false;
					return;
				}
			}

			this.OkButton.IsEnabled = true;
		}

		private static void Layout(Panel Container, FixedField Field, DataForm _)
		{
			TextBlock TextBlock = new()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 5, 0, 5),
				Text = Field.ValueString
			};

			Container.Children.Add(TextBlock);
		}

		private TextBox Layout(Panel Container, JidMultiField Field, DataForm _)
		{
			TextBox TextBox = LayoutTextBox(Container, Field);
			TextBox.TextChanged += this.TextBox_TextChanged;
			TextBox.AcceptsReturn = true;
			TextBox.AcceptsTab = true;
			TextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

			return TextBox;
		}

		private TextBox Layout(Panel Container, JidSingleField Field, DataForm _)
		{
			TextBox TextBox = LayoutTextBox(Container, Field);
			TextBox.TextChanged += this.TextBox_TextChanged;

			return TextBox;
		}

		private GroupBox Layout(Panel Container, ListMultiField Field, DataForm _)
		{
			TextBlock TextBlock = new()
			{
				TextWrapping = TextWrapping.Wrap,
				Text = Field.Label
			};

			if (Field.Required)
			{
				Run Run = new("*");
				TextBlock.Inlines.Add(Run);
				Run.Foreground = new SolidColorBrush(Colors.Red);
			}

			GroupBox GroupBox = new();
			Container.Children.Add(GroupBox);
			GroupBox.Name = VarToName(Field.Var);
			GroupBox.Header = TextBlock;
			GroupBox.ToolTip = Field.Description;
			GroupBox.Margin = new Thickness(5, 5, 5, 5);

			StackPanel StackPanel = new();
			GroupBox.Content = StackPanel;
			StackPanel.Margin = new Thickness(5, 5, 5, 5);

			string[] Values = Field.ValueStrings;
			CheckBox CheckBox;

			foreach (KeyValuePair<string, string> Option in Field.Options)
			{
				CheckBox = new CheckBox()
				{
					Content = Option.Key,
					Tag = Option.Value,
					Margin = new Thickness(0, 3, 0, 3),
					IsEnabled = !Field.ReadOnly,
					IsChecked = Array.IndexOf<string>(Values, Option.Value) >= 0
				};

				if (Field.HasError)
					CheckBox.Background = new SolidColorBrush(Colors.PeachPuff);
				else if (Field.NotSame)
					CheckBox.Background = new SolidColorBrush(Colors.LightGray);

				CheckBox.Click += this.MultiListCheckBox_Click;

				StackPanel.Children.Add(CheckBox);
			}

			GroupBox.Tag = LayoutErrorLabel(StackPanel, Field);

			return GroupBox;
		}

		private async void MultiListCheckBox_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				if (Sender is not CheckBox CheckBox)
					return;

				if (CheckBox.Parent is not StackPanel StackPanel)
					return;

				if (StackPanel.Parent is not GroupBox GroupBox)
					return;

				string Var = NameToVar(GroupBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				List<string> Values = [];

				foreach (UIElement Element in StackPanel.Children)
				{
					CheckBox = Element as CheckBox;
					if (CheckBox is null)
						continue;

					if (CheckBox.IsChecked.HasValue && CheckBox.IsChecked.Value)
						Values.Add((string)CheckBox.Tag);
				}

				await Field.SetValue([.. Values]);

				TextBlock ErrorLabel = (TextBlock)GroupBox.Tag;
				Brush Background;

				if (Field.HasError)
				{
					Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;

					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
				}
				else
				{
					Background = null;
					this.CheckOkButtonEnabled();

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;
				}

				foreach (UIElement Element in StackPanel.Children)
				{
					CheckBox = Element as CheckBox;
					if (CheckBox is null)
						continue;

					CheckBox.Background = Background;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private ComboBox Layout(Panel Container, ListSingleField Field, DataForm _)
		{
			LayoutControlLabel(Container, Field);

			ComboBox ComboBox = new()
			{
				Name = VarToName(Field.Var),
				IsReadOnly = Field.ReadOnly,
				ToolTip = Field.Description,
				Margin = new Thickness(0, 0, 0, 5)
			};

			if (Field.HasError)
				ComboBox.Background = new SolidColorBrush(Colors.PeachPuff);
			else if (Field.NotSame)
				ComboBox.Background = new SolidColorBrush(Colors.LightGray);

			ComboBoxItem Item;

			if (Field.Options is not null)
			{
				foreach (KeyValuePair<string, string> P in Field.Options)
				{
					Item = new ComboBoxItem()
					{
						Content = P.Key,
						Tag = P.Value
					};

					ComboBox.Items.Add(Item);
				}
			}

			if (Field.ValidationMethod is Networking.XMPP.DataForms.ValidationMethods.OpenValidation)
			{
				ComboBox.IsEditable = true;
				ComboBox.Text = Field.ValueString;
				ComboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
					new TextChangedEventHandler(this.ComboBox_TextChanged));
			}
			else
			{
				string s = Field.ValueString;

				ComboBox.IsEditable = false;
				ComboBox.SelectionChanged += this.ComboBox_SelectionChanged;

				if (Field.Options is not null)
					ComboBox.SelectedIndex = Array.FindIndex(Field.Options, (P) => P.Value.Equals(s));
			}

			Container.Children.Add(ComboBox);
			ComboBox.Tag = LayoutErrorLabel(Container, Field);

			return ComboBox;
		}

		private async void ComboBox_TextChanged(object Sender, TextChangedEventArgs e)
		{
			try
			{
				if (Sender is not ComboBox ComboBox)
					return;

				string Var = NameToVar(ComboBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				TextBlock ErrorLabel = (TextBlock)ComboBox.Tag;
				string s = ComboBox.Text;

				if (ComboBox.SelectedItem is ComboBoxItem ComboBoxItem && ((string)ComboBoxItem.Content) == s)
					s = (string)ComboBoxItem.Tag;

				await Field.SetValue(s);

				if (Field.HasError)
				{
					ComboBox.Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;

					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
				}
				else
				{
					ComboBox.Background = null;

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;

					this.CheckOkButtonEnabled();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async void ComboBox_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (Sender is not ComboBox ComboBox)
					return;

				string Var = NameToVar(ComboBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				TextBlock ErrorLabel = (TextBlock)ComboBox.Tag;
				string Value;

				if (ComboBox.SelectedItem is not ComboBoxItem Item)
					Value = string.Empty;
				else
					Value = (string)Item.Tag;

				await Field.SetValue(Value);

				if (Field.HasError)
				{
					ComboBox.Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;

					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
					return;
				}
				else
				{
					ComboBox.Background = null;

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;

					this.CheckOkButtonEnabled();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task Layout(Panel Container, MediaField Field, DataForm _)
		{
			MediaElement MediaElement;
			Uri Uri = null;
			Grade Best = Runtime.Inventory.Grade.NotAtAll;
			Grade Grade;
			bool IsImage = false;
			bool IsVideo = false;
			bool IsAudio = false;

			bool TopMarginLaidOut = LayoutControlLabel(Container, Field);

			foreach (KeyValuePair<string, Uri> P in Field.Media.URIs)
			{
				if (P.Key.StartsWith("image/"))
				{
					IsImage = true;
					Uri = P.Value;
					break;
				}
				else if (P.Key.StartsWith("video/"))
				{
					Grade = P.Key.ToLower() switch
					{
						"video/x-ms-asf" or "video/x-ms-wvx" or "video/x-ms-wm" or "video/x-ms-wmx" => Grade.Perfect,
						"video/mp4" => Grade.Excellent,
						"video/3gp" or "video/3gpp " or "video/3gpp2 " or "video/3gpp-tt" or "video/h263" or "video/h263-1998" or "video/h263-2000" or "video/h264" or "video/h264-rcdo" or "video/h264-svc" => Grade.Ok,
						_ => Grade.Barely,
					};
					if (Grade > Best)
					{
						Best = Grade;
						Uri = P.Value;
						IsVideo = true;
					}
				}
				else if (P.Key.StartsWith("audio/"))
				{
					Grade = P.Key.ToLower() switch
					{
						"audio/x-ms-wma" or "audio/x-ms-wax" or "audio/x-ms-wmv" => Grade.Perfect,
						"audio/mp4" or "audio/mpeg" => Grade.Excellent,
						"audio/amr" or "audio/amr-wb" or "audio/amr-wb+" or "audio/pcma" or "audio/pcma-wb" or "audio/pcmu" or "audio/pcmu-wb" => Grade.Ok,
						_ => Grade.Barely,
					};
					if (Grade > Best)
					{
						Best = Grade;
						Uri = P.Value;
						IsAudio = true;
					}
				}
			}

			if (IsImage)
			{
				BitmapImage BitmapImage = new();
				BitmapImage.BeginInit();
				try
				{
					if (Field.Media.Binary is not null)
						BitmapImage.UriSource = new Uri(await Waher.Content.Markdown.Model.Multimedia.ImageContent.GetTemporaryFile(Field.Media.Binary));
					else if (Uri is not null)
						BitmapImage.UriSource = Uri;
					else if (!string.IsNullOrEmpty(Field.Media.URL))
						BitmapImage.UriSource = new Uri(Field.Media.URL);
				}
				finally
				{
					BitmapImage.EndInit();
				}

				Image Image = new()
				{
					Source = BitmapImage,
					ToolTip = Field.Description,
					Margin = new Thickness(0, TopMarginLaidOut ? 0 : 5, 0, 5)
				};

				if (Field.Media.Width.HasValue)
					Image.Width = Field.Media.Width.Value;

				if (Field.Media.Height.HasValue)
					Image.Height = Field.Media.Height.Value;

				Container.Children.Add(Image);
			}
			else if (IsVideo || IsAudio)
			{
				MediaElement = new MediaElement()
				{
					Source = Uri,
					LoadedBehavior = MediaState.Manual,
					ToolTip = Field.Description
				};

				Container.Children.Add(MediaElement);

				if (IsVideo)
				{
					MediaElement.Margin = new Thickness(0, TopMarginLaidOut ? 0 : 5, 0, 5);

					if (Field.Media.Width.HasValue)
						MediaElement.Width = Field.Media.Width.Value;

					if (Field.Media.Height.HasValue)
						MediaElement.Height = Field.Media.Height.Value;
				}

				DockPanel ControlPanel = new()
				{
					Width = 290
				};

				Container.Children.Add(ControlPanel);

				Button Button = new()
				{
					Width = 50,
					Height = 23,
					Margin = new Thickness(0, 0, 5, 0),
					Content = "<<",
					Tag = MediaElement
				};

				ControlPanel.Children.Add(Button);
				Button.Click += this.Rewind_Click;

				Button = new Button()
				{
					Width = 50,
					Height = 23,
					Margin = new Thickness(5, 0, 5, 0),
					Content = "Play",
					Tag = MediaElement
				};

				ControlPanel.Children.Add(Button);
				Button.Click += this.Play_Click;

				Button = new Button()
				{
					Width = 50,
					Height = 23,
					Margin = new Thickness(5, 0, 5, 0),
					Content = "Pause",
					Tag = MediaElement
				};

				ControlPanel.Children.Add(Button);
				Button.Click += this.Pause_Click;

				Button = new Button()
				{
					Width = 50,
					Height = 23,
					Margin = new Thickness(5, 0, 5, 0),
					Content = "Stop",
					Tag = MediaElement
				};

				ControlPanel.Children.Add(Button);
				Button.Click += this.Stop_Click;

				Button = new Button()
				{
					Width = 50,
					Height = 23,
					Margin = new Thickness(5, 0, 0, 0),
					Content = ">>",
					Tag = MediaElement
				};

				ControlPanel.Children.Add(Button);
				Button.Click += this.Forward_Click;

				MediaElement.Play();
			}
		}

		private void Rewind_Click(object Sender, RoutedEventArgs e)
		{
			Button Button = (Button)Sender;
			MediaElement MediaElement = (MediaElement)Button.Tag;

			if (MediaElement is not null)
			{
				if (MediaElement.SpeedRatio >= 0)
					MediaElement.SpeedRatio = -1;
				else if (MediaElement.SpeedRatio > -32)
					MediaElement.SpeedRatio *= 2;
			}
		}

		private void Play_Click(object Sender, RoutedEventArgs e)
		{
			Button Button = (Button)Sender;
			MediaElement MediaElement = (MediaElement)Button.Tag;

			if (MediaElement is not null)
			{
				if (MediaElement.Position >= MediaElement.NaturalDuration.TimeSpan)
				{
					MediaElement.Stop();
					MediaElement.Position = TimeSpan.Zero;
				}

				MediaElement.Play();
			}
		}

		private void Pause_Click(object Sender, RoutedEventArgs e)
		{
			Button Button = (Button)Sender;
			MediaElement MediaElement = (MediaElement)Button.Tag;
			MediaElement?.Pause();
		}

		private void Stop_Click(object Sender, RoutedEventArgs e)
		{
			Button Button = (Button)Sender;
			MediaElement MediaElement = (MediaElement)Button.Tag;
			MediaElement?.Stop();
		}

		private void Forward_Click(object Sender, RoutedEventArgs e)
		{
			Button Button = (Button)Sender;
			MediaElement MediaElement = (MediaElement)Button.Tag;

			if (MediaElement is not null)
			{
				if (MediaElement.SpeedRatio <= 0)
					MediaElement.SpeedRatio = 1;
				else if (MediaElement.SpeedRatio < 32)
					MediaElement.SpeedRatio *= 2;
			}
		}

		private NonScrollingTextEditor Layout(Panel Container, TextMultiField Field, DataForm _)
		{
			LayoutControlLabel(Container, Field);

			NonScrollingTextEditor Editor = new()
			{
				Name = VarToName(Field.Var),
				Text = Field.ValueString,
				IsReadOnly = Field.ReadOnly,
				ToolTip = Field.Description,
				Margin = new Thickness(0, 0, 0, 5)
			};

			if (Field.HasError)
				Editor.Background = new SolidColorBrush(Colors.PeachPuff);
			else if (Field.NotSame)
				Editor.Background = new SolidColorBrush(Colors.LightGray);

			Container.Children.Add(Editor);
			Editor.Tag = LayoutErrorLabel(Container, Field);

			Editor.TextChanged += this.Editor_TextChanged;
			Editor.BorderBrush = new SolidColorBrush(SystemColors.ActiveBorderColor);
			Editor.BorderThickness = new Thickness(1);
			Editor.Padding = new Thickness(5, 2, 5, 2);
			Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
			Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			Editor.HorizontalAlignment = HorizontalAlignment.Stretch;
			Editor.Height = double.NaN;
			Editor.Width = double.NaN;
			Editor.Options.ShowSpaces = false;
			Editor.Options.ShowTabs = false;

			string ContentType = Field.ContentType?.ToLower() ?? PlainTextCodec.DefaultContentType;
			string SyntaxHighlightingResource = null;

			switch (ContentType)
			{
				case PlainTextCodec.DefaultContentType:
					Editor.WordWrap = true;
					break;

				case "text/markdown":
					Editor.WordWrap = true;
					SyntaxHighlightingResource = "Markdown.xshd";
					break;

				case "text/css":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					SyntaxHighlightingResource = "CSS.xshd";
					break;

				case "text/sgml":
				case "text/csv":
				case "text/tab-separated-values":
				case "application/x-turtle":
				case "text/turtle":
				case "application/sparql-query":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					break;

				case "application/json":
				case "text/x-json":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					SyntaxHighlightingResource = "JSON.xshd";
					break;

				case "text/xml":
				case "application/xml":
				case "text/xsl":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					SyntaxHighlightingResource = "XML.xshd";
					break;

				case "application/javascript":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					SyntaxHighlightingResource = "JavaScript.xshd";
					break;

				case "text/html":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = true;
					SyntaxHighlightingResource = "HTML.xshd";
					break;

				case "text/richtext":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					break;

				case "application/x-webscript":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					break;

				case "text/x-cssx":
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					SyntaxHighlightingResource = "CSS.xshd";
					break;

				default:
					Editor.FontFamily = new FontFamily("Courier New");
					Editor.WordWrap = false;
					Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

					if (ContentType.StartsWith("application/") && ContentType.EndsWith("+json"))
						SyntaxHighlightingResource = "JSON.xshd";
					else if (ContentType.StartsWith("application/") && ContentType.EndsWith("+xml"))
						SyntaxHighlightingResource = "XML.xshd";

					break;
			}

			if (!string.IsNullOrEmpty(SyntaxHighlightingResource))
			{
				// AvalodEdit Syntax Highlight files:
				//
				// CSS, HTML, JavaScript, JSON, XML.
				// Source: https://github.com/icsharpcode/AvalonEdit/tree/master/ICSharpCode.AvalonEdit/Highlighting/Resources
				//
				// Markdown:
				// Source: https://github.com/Trust-Anchor-Group/LegalLab/tree/main/LegalLab/Models/Design/AvalonExtensions

				Type AppType = typeof(App);

				using Stream XshdStream = AppType.Assembly.GetManifestResourceStream(AppType.Namespace + ".Dialogs.AvalonExtensions." +
					SyntaxHighlightingResource);

				using XmlReader XshdReader = new XmlTextReader(XshdStream);
				
				Editor.SyntaxHighlighting = HighlightingLoader.Load(XshdReader, HighlightingManager.Instance);
			}

			return Editor;
		}

		private PasswordBox Layout(Panel Container, TextPrivateField Field, DataForm _)
		{
			LayoutControlLabel(Container, Field);

			PasswordBox PasswordBox = new()
			{
				Name = VarToName(Field.Var),
				Password = Field.ValueString,
				IsEnabled = !Field.ReadOnly,
				ToolTip = Field.Description,
				Margin = new Thickness(0, 0, 0, 5)
			};

			if (Field.HasError)
				PasswordBox.Background = new SolidColorBrush(Colors.PeachPuff);
			else if (Field.NotSame)
				PasswordBox.Background = new SolidColorBrush(Colors.LightGray);

			PasswordBox.PasswordChanged += this.PasswordBox_PasswordChanged;

			Container.Children.Add(PasswordBox);
			PasswordBox.Tag = LayoutErrorLabel(Container, Field);

			return PasswordBox;
		}

		private async void PasswordBox_PasswordChanged(object Sender, RoutedEventArgs e)
		{
			try
			{
				if (Sender is not PasswordBox PasswordBox)
					return;

				string Var = NameToVar(PasswordBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				await Field.SetValue(PasswordBox.Password);

				TextBlock ErrorLabel = (TextBlock)PasswordBox.Tag;

				if (Field.HasError)
				{
					PasswordBox.Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;

					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
				}
				else
				{
					PasswordBox.Background = null;

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;

					this.CheckOkButtonEnabled();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private TextBox Layout(Panel Container, TextSingleField Field, DataForm _)
		{
			TextBox TextBox = LayoutTextBox(Container, Field);
			TextBox.TextChanged += this.TextBox_TextChanged;

			return TextBox;
		}

		private static TextBox LayoutTextBox(Panel Container, Field Field)
		{
			LayoutControlLabel(Container, Field);

			TextBox TextBox = new()
			{
				Name = VarToName(Field.Var),
				Text = Field.ValueString,
				IsReadOnly = Field.ReadOnly,
				ToolTip = Field.Description,
				Margin = new Thickness(0, 0, 0, 5)
			};

			if (Field.HasError)
				TextBox.Background = new SolidColorBrush(Colors.PeachPuff);
			else if (Field.NotSame)
				TextBox.Background = new SolidColorBrush(Colors.LightGray);

			Container.Children.Add(TextBox);
			TextBox.Tag = LayoutErrorLabel(Container, Field);

			return TextBox;
		}

		private static TextBlock LayoutErrorLabel(Panel Container, Field Field)
		{
			TextBlock ErrorLabel = new()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 0, 0, 5),
				Text = Field.Error,
				Foreground = new SolidColorBrush(Colors.Red),
				FontWeight = FontWeights.Bold,
				Visibility = Field.HasError ? Visibility.Visible : Visibility.Collapsed
			};

			Container.Children.Add(ErrorLabel);

			return ErrorLabel;
		}

		private static bool LayoutControlLabel(Panel Container, Field Field)
		{
			if (string.IsNullOrEmpty(Field.Label) && !Field.Required)
				return false;
			else
			{
				TextBlock TextBlock = new()
				{
					TextWrapping = TextWrapping.Wrap,
					Text = Field.Label,
					Margin = new Thickness(0, 5, 0, 0)
				};

				Container.Children.Add(TextBlock);

				if (Field.Required)
				{
					Run Run = new("*");
					TextBlock.Inlines.Add(Run);
					Run.Foreground = new SolidColorBrush(Colors.Red);
				}

				return true;
			}
		}

		private async void TextBox_TextChanged(object Sender, TextChangedEventArgs e)
		{
			try
			{
				if (Sender is not TextBox TextBox)
					return;

				string Var = NameToVar(TextBox.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				TextBlock ErrorLabel = (TextBlock)TextBox.Tag;

				await Field.SetValue(TextBox.Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'));

				if (Field.HasError)
				{
					TextBox.Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;
					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
				}
				else
				{
					TextBox.Background = null;

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;

					this.CheckOkButtonEnabled();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async void Editor_TextChanged(object Sender, EventArgs e)
		{
			try
			{
				if (Sender is not NonScrollingTextEditor Editor)
					return;

				string Var = NameToVar(Editor.Name);
				Field Field = this.form[Var];
				if (Field is null)
					return;

				TextBlock ErrorLabel = (TextBlock)Editor.Tag;

				await Field.SetValue(Editor.Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'));

				if (Field.HasError)
				{
					Editor.Background = new SolidColorBrush(Colors.PeachPuff);
					this.OkButton.IsEnabled = false;
					if (ErrorLabel is not null)
					{
						ErrorLabel.Text = Field.Error;
						ErrorLabel.Visibility = Visibility.Visible;
					}
				}
				else
				{
					Editor.Background = null;

					if (ErrorLabel is not null)
						ErrorLabel.Visibility = Visibility.Collapsed;

					this.CheckOkButtonEnabled();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static bool Layout(Panel Container, ReportedReference _, DataForm Form)
		{
			if (Form.Records.Length == 0 || Form.Header.Length == 0)
				return false;

			Dictionary<string, int> VarIndex = [];
			ColumnDefinition ColumnDefinition;
			RowDefinition RowDefinition;
			TextBlock TextBlock;
			int i, j;

			Brush BorderBrush = new SolidColorBrush(Colors.Gray);
			Brush Bg1 = new SolidColorBrush(Color.FromArgb(0x20, 0x40, 0x40, 0x40));
			Brush Bg2 = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80));
			Border Border;
			Grid Grid = new();
			Container.Children.Add(Grid);

			i = 0;
			foreach (Field Field in Form.Header)
			{
				ColumnDefinition = new ColumnDefinition()
				{
					Width = GridLength.Auto
				};

				Grid.ColumnDefinitions.Add(ColumnDefinition);

				VarIndex[Field.Var] = i++;
			}

			RowDefinition = new RowDefinition()
			{
				Height = GridLength.Auto
			};

			Grid.RowDefinitions.Add(RowDefinition);

			foreach (Field[] Row in Form.Records)
			{
				RowDefinition = new RowDefinition()
				{
					Height = GridLength.Auto
				};

				Grid.RowDefinitions.Add(RowDefinition);
			}

			foreach (Field Field in Form.Header)
			{
				if (!VarIndex.TryGetValue(Field.Var, out i))
					continue;

				Border = new Border();
				Grid.Children.Add(Border);

				Grid.SetColumn(Border, i);
				Grid.SetRow(Border, 0);

				Border.BorderBrush = BorderBrush;
				Border.BorderThickness = new Thickness(1);
				Border.Padding = new Thickness(5, 1, 5, 1);
				Border.Background = Bg1;

				TextBlock = new TextBlock()
				{
					FontWeight = FontWeights.Bold,
					Text = Field.Label
				};

				Border.Child = TextBlock;
			}

			j = 0;
			foreach (Field[] Row in Form.Records)
			{
				j++;

				foreach (Field Field in Row)
				{
					if (!VarIndex.TryGetValue(Field.Var, out i))
						continue;

					Border = new Border();
					Grid.Children.Add(Border);

					Grid.SetColumn(Border, i);
					Grid.SetRow(Border, j);

					Border.BorderBrush = BorderBrush;
					Border.BorderThickness = new Thickness(1);
					Border.Padding = new Thickness(5, 1, 5, 1);

					if ((j & 1) == 1)
						Border.Background = Bg2;
					else
						Border.Background = Bg1;

					TextBlock = new TextBlock()
					{
						Text = Field.ValueString
					};

					Border.Child = TextBlock;
				}
			}

			return true;
		}

		private async void OkButton_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				await this.form.Submit();

				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				await this.form.Cancel();

				this.DialogResult = false;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void Window_Activated(object Sender, EventArgs e)
		{
			if (this.makeVisible is not null)
			{
				LinkedList<FrameworkElement> List = [];

				while (this.makeVisible is not null)
				{
					List.AddFirst(this.makeVisible);
					this.makeVisible = this.makeVisible.Parent as FrameworkElement;
				}

				foreach (FrameworkElement E in List)
				{
					if (E.Focusable)
						E.Focus();
					else
						E.BringIntoView();
				}
			}
		}

		private static string VarToName(string Var)
		{
			return "Form_" + Var.Replace("#", "__GATO__");
		}

		private static string NameToVar(string Name)
		{
			return Name[5..].Replace("__GATO__", "#");
		}

		// TODO: Color picker.
		// TODO: Dynamic forms & post back

	}
}
