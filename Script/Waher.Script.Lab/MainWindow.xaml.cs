using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;

namespace Waher.Script.Lab
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static readonly string registryKey = Registry.CurrentUser + @"\Software\Waher Data AB\Waher.Script.Lab";

		private Variables variables;

		public MainWindow()
		{
			InitializeComponent();

			Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
			Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

			Initialize();

			this.variables = new Variables()
			{
				ConsoleOut = new PrintOutput(this)
			};

			this.Input.Focus();
		}

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <paramref name="Assemblies"/>.
		/// </summary>
		/// <param name="Folder">Name of folder containing assemblies to load, if they are not already loaded.</param>
		private static void Initialize()
		{
			string Folder = Path.GetDirectoryName(typeof(App).GetTypeInfo().Assembly.Location);
			string[] DllFiles = Directory.GetFiles(Folder, "*.dll", SearchOption.TopDirectoryOnly);
			Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);
			Dictionary<string, AssemblyName> ReferencedAssemblies = new Dictionary<string, AssemblyName>(StringComparer.CurrentCultureIgnoreCase);

			foreach (string DllFile in DllFiles)
			{
				try
				{
					Assembly A = Assembly.LoadFile(DllFile);
					LoadedAssemblies[A.GetName().FullName] = A;

					foreach (AssemblyName AN in A.GetReferencedAssemblies())
						ReferencedAssemblies[AN.FullName] = AN;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			do
			{
				AssemblyName[] References = new AssemblyName[ReferencedAssemblies.Count];
				ReferencedAssemblies.Values.CopyTo(References, 0);
				ReferencedAssemblies.Clear();

				foreach (AssemblyName AN in References)
				{
					if (LoadedAssemblies.ContainsKey(AN.FullName))
						continue;

					try
					{
						Assembly A = Assembly.Load(AN);
						LoadedAssemblies[A.GetName().FullName] = A;

						foreach (AssemblyName AN2 in A.GetReferencedAssemblies())
							ReferencedAssemblies[AN2.FullName] = AN2;
					}
					catch (Exception)
					{
						Log.Error("Unable to load assembly " + AN.ToString() + ".");
					}
				}
			}
			while (ReferencedAssemblies.Count > 0);

			Assembly[] Assemblies = new Assembly[LoadedAssemblies.Count];
			LoadedAssemblies.Values.CopyTo(Assemblies, 0);

			Types.Initialize(Assemblies);
		}

		private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					this.ExecuteButton_Click(sender, e);
					e.Handled = true;
				}
			}
		}

		private void ExecuteButton_Click(object sender, RoutedEventArgs e)
		{
			Expression Exp;
			TextBlock ScriptBlock;

			try
			{
				string s = this.Input.Text.Trim();
				if (string.IsNullOrEmpty(s))
					return;

				Exp = new Expression(s);

				ScriptBlock = new TextBlock()
				{
					Text = this.Input.Text,
					FontFamily = new FontFamily("Courier New"),
					TextWrapping = TextWrapping.Wrap
				};

				ScriptBlock.PreviewMouseDown += TextBlock_PreviewMouseDown;

				this.HistoryPanel.Children.Add(ScriptBlock);
				this.HistoryScrollViewer.ScrollToBottom();

				this.Input.Text = string.Empty;
				this.Input.Focus();
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MessageBox.Show(this, ex.Message, "Unable to parse script.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Task.Run(() =>
			{
				try
				{
					IElement Ans;

					try
					{
						Ans = Exp.Root.Evaluate(this.variables);
					}
					catch (ScriptReturnValueException ex)
					{
						Ans = ex.ReturnValue;
					}
					catch (Exception ex)
					{
						Ans = new ObjectValue(ex);
					}

					this.variables["Ans"] = Ans;

					this.Dispatcher.Invoke(() =>
					{
						SKImage Img;
						object Obj;

						if (Ans is Graph G)
						{
							GraphSettings Settings = new GraphSettings();
							Tuple<int, int> Size;
							double d;

							if ((Size = G.RecommendedBitmapSize) != null)
							{
								Settings.Width = Size.Item1;
								Settings.Height = Size.Item2;

								Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
								Settings.MarginRight = Settings.MarginLeft;

								Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
								Settings.MarginBottom = Settings.MarginTop;
								Settings.LabelFontSize = 12.0 * Settings.Height / 480;
							}
							else
							{
								if (this.variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
								{
									Settings.Width = (int)Math.Round(d);
									Settings.MarginLeft = (int)Math.Round(15 * d / 640);
									Settings.MarginRight = Settings.MarginLeft;
								}
								else if (!this.variables.ContainsVariable("GraphWidth"))
									this.variables["GraphWidth"] = (double)Settings.Width;

								if (this.variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
								{
									Settings.Height = (int)Math.Round(d);
									Settings.MarginTop = (int)Math.Round(15 * d / 480);
									Settings.MarginBottom = Settings.MarginTop;
									Settings.LabelFontSize = 12 * d / 480;
								}
								else if (!this.variables.ContainsVariable("GraphHeight"))
									this.variables["GraphHeight"] = (double)Settings.Height;
							}

							using (SKImage Bmp = G.CreateBitmap(Settings, out object[] States))
							{
								this.AddImageBlock(ScriptBlock, Bmp, G, States);
							}
						}
						else if ((Img = Ans.AssociatedObjectValue as SKImage) != null)
							this.AddImageBlock(ScriptBlock, Img, null, null);
						else if (Ans.AssociatedObjectValue is Exception ex)
						{
							ex = Log.UnnestException(ex);

							if (ex is AggregateException ex2)
							{
								foreach (Exception ex3 in ex2.InnerExceptions)
									ScriptBlock = this.AddTextBlock(ScriptBlock, ex3.Message, Colors.Red, FontWeights.Bold);
							}
							else
								this.AddTextBlock(ScriptBlock, ex.Message, Colors.Red, FontWeights.Bold);
						}
						else
							this.AddTextBlock(ScriptBlock, Ans.ToString(), Colors.Red, FontWeights.Normal);
					});
				}
				catch (Exception ex)
				{
					this.Dispatcher.Invoke(() =>
					{
						ex = Log.UnnestException(ex);
						MessageBox.Show(this, ex.Message, "Unable to parse script.", MessageBoxButton.OK, MessageBoxImage.Error);
					});
				}
			});
		}

		private TextBlock AddTextBlock(TextBlock ScriptBlock, string s, Color cl, FontWeight FontWeight)
		{
			TextBlock ResultBlock = new TextBlock()
			{
				Text = s,
				FontFamily = new FontFamily("Courier New"),
				Foreground = new SolidColorBrush(cl),
				TextWrapping = TextWrapping.Wrap,
				FontWeight = FontWeight
			};

			ResultBlock.PreviewMouseDown += TextBlock_PreviewMouseDown;

			if (ScriptBlock == null)
				this.HistoryPanel.Children.Add(ResultBlock);
			else
				this.HistoryPanel.Children.Insert(this.HistoryPanel.Children.IndexOf(ScriptBlock) + 1, ResultBlock);

			return ResultBlock;
		}

		private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.Input.Text = ((TextBlock)sender).Text;
			this.Input.SelectAll();
			this.Input.Focus();
			e.Handled = true;
		}

		private void AddImageBlock(TextBlock ScriptBlock, SKImage Image, Graph Graph, object[] States)
		{
			BitmapImage BitmapImage;
			byte[] Bin;

			using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
			{
				Bin = Data.ToArray();
				MemoryStream ms = new MemoryStream(Bin);

				BitmapImage = new BitmapImage();
				BitmapImage.BeginInit();
				BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				BitmapImage.StreamSource = ms;
				BitmapImage.EndInit();

				ms.Dispose();
			}

			Image ImageBlock = new Image()
			{
				Source = BitmapImage,
				Width = Image.Width,
				Height = Image.Height,
				Tag = new Tuple<byte[], int, int, Graph, object[]>(Bin, Image.Width, Image.Height, Graph, States)
			};

			ImageBlock.PreviewMouseDown += ImageBlock_PreviewMouseDown;

			this.HistoryPanel.Children.Insert(this.HistoryPanel.Children.IndexOf(ScriptBlock) + 1, ImageBlock);
		}

		private void ImageBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Image ImageBlock = (Image)sender;

			if (e.ChangedButton == MouseButton.Left)
			{
				Point P = e.GetPosition(ImageBlock);
				string Script;

				if (ImageBlock.Tag is Tuple<byte[], int, int, Graph, object[]> Image && Image.Item4 != null && Image.Item5 != null)
				{
					double X = ((double)P.X) * Image.Item2 / ImageBlock.ActualWidth;
					double Y = ((double)P.Y) * Image.Item3 / ImageBlock.ActualHeight;

					Script = Image.Item4.GetBitmapClickScript(X, Y, Image.Item5);
				}
				else
					Script = "[" + P.X.ToString() + "," + P.Y.ToString() + "]";

				this.Input.Text = Script;
				this.ExecuteButton_Click(this, e);
			}
			else if (e.ChangedButton == MouseButton.Right)
			{
				BitmapImage Image = (BitmapImage)ImageBlock.Source;

				SaveFileDialog Dialog = new SaveFileDialog()
				{
					Title = "Save Image",
					DefaultExt = "png",
					Filter = "PNG files (*.png)|*.png|All Image files (*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.tif, *.tiff)|*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.tif, *.tiff|All files (*.*)|*.*",
					OverwritePrompt = true
				};

				bool? Result = Dialog.ShowDialog();
				if (Result.HasValue && Result.Value)
				{
					BitmapEncoder Encoder;

					switch (System.IO.Path.GetExtension(Dialog.FileName).ToLower())
					{
						case ".jpg":
						case ".jpeg":
							Encoder = new JpegBitmapEncoder();
							break;

						case ".bmp":
							Encoder = new BmpBitmapEncoder();
							break;

						case ".gif":
							Encoder = new GifBitmapEncoder();
							break;

						case ".tif":
						case ".tiff":
							Encoder = new TiffBitmapEncoder();
							break;

						case ".png":
						default:
							Encoder = new PngBitmapEncoder();
							break;
					}

					try
					{
						Encoder.Frames.Add(BitmapFrame.Create(Image));

						using (FileStream File = new FileStream(Dialog.FileName, System.IO.FileMode.Create))
						{
							Encoder.Save(File);
						}
					}
					catch (Exception ex)
					{
						ex = Log.UnnestException(ex);
						MessageBox.Show(this, ex.Message, "Unable to save image.", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}

			e.Handled = true;
		}

		internal void Print(string Output)
		{
			this.Dispatcher.Invoke(() => this.AddTextBlock(null, Output, Colors.Blue, FontWeights.Normal));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			object Value;

			try
			{
				Value = Registry.GetValue(registryKey, "WindowLeft", (int)this.Left);
				if (Value != null && Value is int)
					this.Left = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value != null && Value is int)
					this.Top = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value != null && Value is int)
					this.Width = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value != null && Value is int)
					this.Height = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value != null && Value is string)
					this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), (string)Value);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MessageBox.Show(this, ex.Message, "Unable to load values from registry.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Registry.SetValue(registryKey, "WindowLeft", (int)this.Left, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowTop", (int)this.Top, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowWidth", (int)this.Width, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowHeight", (int)this.Height, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowState", this.WindowState.ToString(), RegistryValueKind.String);

			Log.Terminate();
		}
	}
}
