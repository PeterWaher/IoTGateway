using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Files;
using Waher.Networking;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private const int NrFilesExpected = 1586;

		private static MainWindow? instance = null;
		private readonly Dictionary<string, bool> instancesFound = [];
		private readonly Command installDefault;
		private readonly Command installNewInstance;
		private readonly Command quit;
		private readonly ParametrizedCommand openLink;
		private readonly ParametrizedCommand uninstallInstance;
		private readonly ParametrizedCommand repairInstance;
		private bool installing = false;
		private int nrInstanceControlsFound = 0;

		public MainWindow()
		{
			instance = this;

			this.installDefault = new Command(this.IsIdle, this.ExecuteInstallDefaultInstance);
			this.installNewInstance = new Command(this.IsIdle, this.ExecuteInstallNewInstance);
			this.quit = new Command(this.IsIdle, this.ExecuteQuit);
			this.openLink = new ParametrizedCommand(null, ExecuteOpenLink);
			this.uninstallInstance = new(this.IsIdle, this.ExecuteUninstallInstance);
			this.repairInstance = new(this.IsIdle, this.ExecuteUpdateOrRepairInstance);

			Type ThisType = typeof(MainWindow);
			Assembly ThisAssembly = ThisType.Assembly;
			Types.Initialize(ThisAssembly);

			byte[] XsltBin = Runtime.IO.Resources.LoadResource(ThisType.Namespace + ".EventXmlToHtml.xslt");
			string Transform = "data:text/xsl;base64," + Convert.ToBase64String(XsltBin);

			string TempFolder = Path.GetTempPath();
			string LogFileName = "Installation " + XML.Encode(DateTime.Now).Replace(':', '_') + ".xml";
			string LogFilePath = Path.Combine(TempFolder, LogFileName);
			MainWindowEventSink MainWindowEventSink = new(this);

			Log.Register(new XmlFileEventSink("Installation Log", LogFilePath, Transform, int.MaxValue));
			Log.Register(MainWindowEventSink);

			this.InitializeComponent();
			MainWindowEventSink.Start();

			this.DataContext = this;
			this.InstallationLogFile.Inlines.Add(new Run(LogFileName));
			this.InstallationLogFile.Tag = LogFilePath;
		}

		internal static MainWindow? Instance => instance;

		/// <summary>
		/// Command for installing default instance.
		/// </summary>
		public Command InstallDefault => this.installDefault;

		/// <summary>
		/// Command for installing new instance.
		/// </summary>
		public Command InstallNewInstance => this.installNewInstance;

		/// <summary>
		/// Command for quitting application
		/// </summary>
		public Command Quit => this.quit;

		internal void AddStatus(EventType Type, string Message)
		{
			Label Label = new()
			{
				FontSize = 15,
				FontFamily = new FontFamily("Courier New"),
				Margin = new Thickness(0, 0, 0, 0),
				Content = new TextBlock()
				{
					TextWrapping = TextWrapping.Wrap,
					Text = Message
				},
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Foreground = new SolidColorBrush(CalcForegroundColor(Type)),
				Background = new SolidColorBrush(CalcBackgroundColor(Type))
			};

			this.LogEntries.Children.Add(Label);
		}

		private static Color CalcForegroundColor(EventType Type)
		{
			return Type switch
			{
				EventType.Debug => Colors.White,
				EventType.Informational => Colors.Black,
				EventType.Notice => Colors.Black,
				EventType.Warning => Colors.Black,
				EventType.Error => Colors.Yellow,
				EventType.Critical => Colors.White,
				EventType.Alert => Colors.White,
				EventType.Emergency => Colors.White,
				_ => Colors.Black,
			};
		}

		private static Color CalcBackgroundColor(EventType Type)
		{
			return Type switch
			{
				EventType.Debug => Colors.DarkBlue,
				EventType.Informational => Colors.White,
				EventType.Notice => Colors.LightYellow,
				EventType.Warning => Colors.Yellow,
				EventType.Error => Colors.Red,
				EventType.Critical => Colors.DarkRed,
				EventType.Alert => Colors.Purple,
				EventType.Emergency => Colors.Black,
				_ => Colors.White,
			};
		}

		private void Window_Initialized(object Sender, EventArgs e)
		{
			this.InstallationsFound.Visibility = Visibility.Collapsed;
			this.NoInstallationsFound.Visibility = Visibility.Visible;

			Log.Informational("Looking for existing installations...");

			string AppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string Programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			string[] AppDataFolders = Directory.GetDirectories(AppData);

			foreach (string AppDataFolder in AppDataFolders)
			{
				string LocalName = Path.GetFileName(AppDataFolder);
				if (!(LocalName.StartsWith("IoT Gateway")))
					continue;

				this.InstallationsFound.Visibility = Visibility.Visible;
				this.NoInstallationsFound.Visibility = Visibility.Collapsed;

				string Instance = LocalName[11..].TrimStart();
				string InstallationFolder = Path.Combine(Programs, App.AppName + Instance);
				bool InstallationExists = Directory.Exists(InstallationFolder);

				this.instancesFound[Instance] = InstallationExists;

				if (string.IsNullOrEmpty(Instance))
				{
					if (InstallationExists)
					{
						Log.Notice("Default installation found.");
						this.AddUpdateOrRepairButton(AppDataFolder, Instance);
						this.AddUninstallButton(AppDataFolder, Instance);
					}
					else
					{
						string Msg = "Default installation found, but it is managed by another installation tool.";
						Label Label = new()
						{
							FontSize = 15,
							Margin = new Thickness(10, 10, 0, 10),
							Padding = new Thickness(20, 0, 20, 0),
							Content = new TextBlock()
							{
								TextWrapping = TextWrapping.Wrap,
								Text = Msg
							}
						};

						this.InstallationsFoundCommand.Children.Insert(this.nrInstanceControlsFound++, Label);

						Log.Warning(Msg);
					}
				}
				else
				{
					if (InstallationExists)
					{
						Log.Notice("Installation instance \"" + Instance + "\" found.");
						this.AddUpdateOrRepairButton(AppDataFolder, Instance);
						this.AddUninstallButton(AppDataFolder, Instance);
					}
					else
					{
						string Msg = "Installation instance \"" + Instance + "\" found, but it is managed by another installation tool.";
						Label Label = new()
						{
							FontSize = 15,
							Margin = new Thickness(10, 10, 0, 10),
							Padding = new Thickness(20, 0, 20, 0),
							Content = new TextBlock()
							{
								TextWrapping = TextWrapping.Wrap,
								Text = Msg
							}
						};

						this.InstallationsFoundCommand.Children.Insert(this.nrInstanceControlsFound++, Label);

						Log.Warning(Msg);
					}
				}
			}
		}

		/// <summary>
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/>
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		private void RaisePropertyChanged(string PropertyName)
		{
			try
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// If application is idle, and can begin work, or be closed.
		/// </summary>
		public bool Idle => !this.installing;

		/// <summary>
		/// If application is installing (or uninstalling).
		/// </summary>
		public bool Installing
		{
			get => this.installing;
			set
			{
				if (this.installing != value)
				{
					this.installing = value;
					this.RaisePropertyChanged(nameof(this.Installing));
					this.RaisePropertyChanged(nameof(this.Idle));
					this.uninstallInstance.RaiseCanExecuteChanged();
					this.repairInstance.RaiseCanExecuteChanged();
					this.installDefault.RaiseCanExecuteChanged();
					this.installNewInstance.RaiseCanExecuteChanged();
					this.quit.RaiseCanExecuteChanged();
				}
			}
		}

		private Task ExecuteQuit()
		{
			if (this.InstallationCheck(false))
				this.Close();

			return Task.CompletedTask;
		}

		private void Window_Closing(object Sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.InstallationCheck(false))
				e.Cancel = true;
			else
				Log.Informational("Closing application.");
		}

		private void Window_Closed(object Sender, EventArgs e)
		{
			Log.TerminateAsync().Wait();
		}

		private async Task ExecuteInstallNewInstance()
		{
			try
			{
				if (!this.InstallationCheck(true))
					return;

				int Port = 80;
				bool? b;

				if ((b = await CheckPortNumber(Port)).HasValue && !b.Value)
				{
					Port = 8080;
					while ((b = await CheckPortNumber(Port)).HasValue && !b.Value && Port < ushort.MaxValue)
						Port++;
				}

				InstanceNameDialog Dialog = new(Port)
				{
					Owner = this
				};

				b = Dialog.ShowDialog();

				if (!b.HasValue || !b.Value || string.IsNullOrEmpty(Dialog.InstanceName.Text))
					return;

				string InstanceName = Dialog.InstanceName.Text;
				string AppDataFolder = GetAppDataFolder(InstanceName);

				await this.Install(AppDataFolder, InstanceName, Dialog.Port);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				ShowError(ex);
			}
		}

		internal static void ShowError(Exception ex)
		{
			MessageBox.Show(instance, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private static string GetAppDataFolder(string InstanceName)
		{
			string AppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string ProgramDataFolder = Path.Combine(AppData, string.IsNullOrEmpty(InstanceName) ? "IoT Gateway" : "IoT Gateway " + InstanceName);

			return ProgramDataFolder;
		}

		private async Task ExecuteInstallDefaultInstance()
		{
			try
			{
				bool? b = await CheckPortNumber(80);
				if (b.HasValue && b.Value)
				{
					if (!this.InstallationCheck(true))
						return;

					if (MessageBox.Show(this, "Please press OK to confirm you want to install " + App.AppNameDisplayable + " on your computer.",
						"Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
					{
						return;
					}

					string AppDataFolder = GetAppDataFolder(string.Empty);

					await this.Install(AppDataFolder, string.Empty, 80);
				}
				else
					await this.ExecuteInstallNewInstance();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				ShowError(ex);
			}
		}

		private static void ClearPortStatus()
		{
			lock (portStatus)
			{
				portStatus.Clear();
			}
		}

		internal static async Task<bool?> CheckPortNumber(int PortNumber)
		{
			try
			{
				bool? b;

				lock (portStatus)
				{
					if (portStatus.TryGetValue(PortNumber, out b))
						return b;
				}

				using BinaryTcpServer Server = new(false, PortNumber, TimeSpan.FromMinutes(1), false, []);
				await Server.Open(null, out int NrOpened, out int NrFailed);

				if (NrFailed > 0)
					b = false;
				else if (NrOpened == 0)
					b = null;
				else
					b = true;

				lock (portStatus)
				{
					portStatus[PortNumber] = b;
				}

				return b;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static readonly Dictionary<int, bool?> portStatus = [];

		private bool InstallationCheck(bool CheckAdmin)
		{
			if (this.Installing)
			{
				MessageBox.Show(this, "An installation is currently underway. Let the installation complete, and try again.",
					"Error", MessageBoxButton.OK, MessageBoxImage.Error);

				return false;
			}

			if (CheckAdmin && App.IsUserAdministrator())
			{
				if (MessageBox.Show(this, "You are running this application as an administrator. " +
					"You should run the installation using your normal user account, as privileges " +
					"will be given using the current account. Is this your normal user accout? " +
					"To continue with the installation, press the Yes button. Otherwise, press the " +
					"No button, close the application, and restart it with your normal user account.",
					"Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
				{
					return false;
				}
			}

			return true;
		}

		private async Task Install(string AppDataFolder, string InstanceName, int? PortNumber)
		{
			bool Errors = false;

			ClearPortStatus();

			ProgressWindow Progress = new(NrFilesExpected)
			{
				Owner = this
			};
			InstallationProcess InstallationProcess = new();

			Progress.Show();

			if (App.IsUserAdministrator())
			{
				Log.Register(InstallationProcess);
				InstallationProcess.Window = Progress;

				this.Installing = true;
				try
				{
					this.Cursor = Cursors.Wait;

					await Task.Run(() => App.Install(InstanceName, PortNumber));

					if (PortNumber.HasValue)
					{
						await this.Dispatcher.BeginInvoke(() =>
						{
							this.AddUpdateOrRepairButton(AppDataFolder, InstanceName);
							this.AddUninstallButton(AppDataFolder, InstanceName);
						});
					}
				}
				catch (Exception ex)
				{
					Errors = true;
					Log.Exception(ex);
				}
				finally
				{
					Log.Informational("Installation completed.",
						new KeyValuePair<string, object>("NrFilesCopied", Progress.NrFilesCopied),
						new KeyValuePair<string, object>("NrFilesSkipped", Progress.NrFilesSkipped),
						new KeyValuePair<string, object>("NrFilesDeleted", Progress.NrFilesDeleted));

					Log.Unregister(InstallationProcess);
					Progress.Close();

					await this.ShowInstallationResult(Errors, PortNumber);
				}
			}
			else
			{
				this.Installing = true;
				this.Cursor = Cursors.Wait;

				this.LogEntries.Children.Clear();

				Task _ = Task.Run(async () =>
				{
					try
					{
						string FileName = Process.GetCurrentProcess().MainModule?.FileName
							?? Assembly.GetEntryAssembly()?.Location
							?? Assembly.GetExecutingAssembly().Location;
						string TempFileName = Path.GetTempFileName();
						string TempFileName2 = Path.GetTempFileName();
						string TempLogFileName = TempFileName + ".xml";
						string TempLogFileName2 = TempFileName2 + ".xml";

						StringBuilder Arguments = new();

						Arguments.Append("-inst \"");
						Arguments.Append(InstanceName);
						Arguments.Append('"');

						if (PortNumber.HasValue)
						{
							Arguments.Append(" -port ");
							Arguments.Append(PortNumber.Value);
						}

						Arguments.Append(" -log \"");
						Arguments.Append(TempLogFileName);
						Arguments.Append('"');

						ProcessStartInfo StartInfo = new()
						{
							FileName = FileName,
							WorkingDirectory = Directory.GetCurrentDirectory(),
							Arguments = Arguments.ToString(),
							Verb = "runas", // Starts with administrator rights.
							CreateNoWindow = true,
							WindowStyle = ProcessWindowStyle.Hidden,
							UseShellExecute = true
						};

						Log.Informational("Executing installation in process with administrative privileges.",
							new KeyValuePair<string, object>("FileName", StartInfo.FileName),
							new KeyValuePair<string, object>("WorkingDirectory", StartInfo.WorkingDirectory),
							new KeyValuePair<string, object>("Arguments", StartInfo.Arguments),
							new KeyValuePair<string, object>("Verb", StartInfo.Verb),
							new KeyValuePair<string, object>("RedirectStandardError", StartInfo.RedirectStandardError),
							new KeyValuePair<string, object>("RedirectStandardOutput", StartInfo.RedirectStandardOutput),
							new KeyValuePair<string, object>("CreateNoWindow", StartInfo.CreateNoWindow),
							new KeyValuePair<string, object>("WindowStyle", StartInfo.WindowStyle),
							new KeyValuePair<string, object>("UseShellExecute", StartInfo.UseShellExecute));

						Process P = Process.Start(StartInfo)
							?? throw new Exception("Unable to start installation process.");

						while (!P.HasExited)
						{
							ProcessInstallEvents(TempLogFileName, TempLogFileName2, InstallationProcess);

							await this.Dispatcher.BeginInvoke(() => Progress.ReportProgress(InstallationProcess.NrFilesCopied,
								InstallationProcess.NrFilesSkipped, InstallationProcess.NrFilesDeleted, InstallationProcess.InstallationStatus));

							await Task.Delay(100);
						}

						ProcessInstallEvents(TempLogFileName, TempLogFileName2, InstallationProcess);

						await this.Dispatcher.BeginInvoke(() => Progress.ReportProgress(InstallationProcess.NrFilesCopied,
							InstallationProcess.NrFilesSkipped, InstallationProcess.NrFilesDeleted, InstallationProcess.InstallationStatus));

						int ExitCode = P.ExitCode;

						if (ExitCode != 0)
						{
							Errors = true;
							Log.Error("Unable to install instance.",
								new KeyValuePair<string, object>("ExitCode", ExitCode),
								new KeyValuePair<string, object>("NrFilesCopied", InstallationProcess.NrFilesCopied),
								new KeyValuePair<string, object>("NrFilesSkipped", InstallationProcess.NrFilesSkipped),
								new KeyValuePair<string, object>("NrFilesDeleted", InstallationProcess.NrFilesDeleted));
						}
						else
						{
							Log.Notice("Instance installed.",
								new KeyValuePair<string, object>("NrFilesCopied", InstallationProcess.NrFilesCopied),
								new KeyValuePair<string, object>("NrFilesSkipped", InstallationProcess.NrFilesSkipped),
								new KeyValuePair<string, object>("NrFilesDeleted", InstallationProcess.NrFilesDeleted));

							if (PortNumber.HasValue)
							{
								await this.Dispatcher.BeginInvoke(() =>
								{
									this.AddUpdateOrRepairButton(AppDataFolder, InstanceName);
									this.AddUninstallButton(AppDataFolder, InstanceName);
								});
							}
						}
					}
					catch (Exception ex)
					{
						Errors = true;
						Log.Exception(ex);
					}
					finally
					{
						await this.ShowInstallationResult(Errors, PortNumber);
						await this.Dispatcher.BeginInvoke(() => Progress.Close());
					}
				});
			}
		}

		private void AddUninstallButton(string AppDataFolder, string InstanceName)
		{
			WrapPanel InstancePanel = new()
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Width = double.NaN,
				Tag = InstanceName
			};

			TextBlock Text = new()
			{
				TextWrapping = TextWrapping.Wrap
			};

			Text.Inlines.Add(new Run()
			{
				Text = "Uninstall "
			});

			if (string.IsNullOrEmpty(InstanceName))
			{
				Text.Inlines.Add(new Run()
				{
					Text = "default installation",
					FontWeight = FontWeights.Bold
				});
			}
			else
			{
				Text.Inlines.Add(new Run()
				{
					Text = InstanceName + " instance",
					FontWeight = FontWeights.Bold
				});
			}

			Text.Inlines.Add(new Run()
			{
				Text = " of " + App.AppNameDisplayable + " from machine."
			});

			Button Button = new()
			{
				FontSize = 15,
				Margin = new Thickness(10, 10, 0, 10),
				Padding = new Thickness(20, 5, 20, 5),
				Background = Brushes.DarkRed,
				Foreground = Brushes.White,
				Content = Text,
				Command = this.uninstallInstance,
				CommandParameter = InstanceName,
				Width = 500,
				MinWidth = 50
			};

			InstancePanel.Children.Add(Button);

			this.AddInstanceLinks(InstanceName, AppDataFolder, InstancePanel);
		}

		private void AddInstanceLinks(string InstanceName, string AppDataFolder, WrapPanel InstancePanel)
		{
			int? Port = App.GetPort(AppDataFolder);
			if (Port.HasValue)
			{
				string Url = "http://localhost:" + Port.Value.ToString() + "/";
				Hyperlink Link = new()
				{
					NavigateUri = new Uri(Url),
					Command = this.openLink,
					CommandParameter = Url
				};

				Link.Inlines.Add(string.IsNullOrEmpty(InstanceName) ? "Main page" : InstanceName + " Main page");

				Label Label = new()
				{
					FontSize = 15,
					Margin = new Thickness(2, 10, 0, 10),
					Content = Link
				};

				InstancePanel.Children.Add(Label);

				Url = "http://localhost:" + Port.Value.ToString() + "/Settings/Backup.md";
				Link = new Hyperlink()
				{
					NavigateUri = new Uri(Url),
					Command = this.openLink,
					CommandParameter = Url
				};

				Link.Inlines.Add(string.IsNullOrEmpty(InstanceName) ? "Backups" : InstanceName + " Backups");

				Label = new()
				{
					FontSize = 15,
					Margin = new Thickness(2, 10, 0, 10),
					Content = Link
				};

				InstancePanel.Children.Add(Label);
			}

			this.InstallationsFoundCommand.Children.Insert(this.nrInstanceControlsFound++, InstancePanel);

			this.InstallationsFound.Visibility = Visibility.Visible;
			this.NoInstallationsFound.Visibility = Visibility.Collapsed;
		}

		private void AddUpdateOrRepairButton(string AppDataFolder, string InstanceName)
		{
			WrapPanel InstancePanel = new()
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Width = double.NaN,
				Tag = InstanceName
			};

			TextBlock Text = new()
			{
				TextWrapping = TextWrapping.Wrap
			};

			Text.Inlines.Add(new Run()
			{
				Text = "Update or Repair "
			});

			if (string.IsNullOrEmpty(InstanceName))
			{
				Text.Inlines.Add(new Run()
				{
					Text = "default installation",
					FontWeight = FontWeights.Bold
				});
			}
			else
			{
				Text.Inlines.Add(new Run()
				{
					Text = InstanceName + " instance",
					FontWeight = FontWeights.Bold
				});
			}

			Text.Inlines.Add(new Run()
			{
				Text = " of " + App.AppNameDisplayable + " on machine."
			});

			Button Button = new()
			{
				FontSize = 15,
				Margin = new Thickness(10, 10, 0, 10),
				Padding = new Thickness(20, 5, 20, 5),
				Background = Brushes.DarkBlue,
				Foreground = Brushes.White,
				Content = Text,
				Command = this.repairInstance,
				CommandParameter = InstanceName,
				Width = 500,
				MinWidth = 50
			};

			InstancePanel.Children.Add(Button);

			this.AddInstanceLinks(InstanceName, AppDataFolder, InstancePanel);
		}

		private void RemoveUninstallAndUpdateOrRepairButtons(string InstanceName)
		{
			int i;

			for (i = 0; i < this.nrInstanceControlsFound; i++)
			{
				if (this.InstallationsFoundCommand.Children[i] is WrapPanel Panel &&
					InstanceName.Equals(Panel.Tag))
				{
					this.InstallationsFoundCommand.Children.RemoveAt(i);
					this.InstallationsFoundCommand.Children.Insert(i, new Label()
					{
						FontSize = 15,
						Margin = new Thickness(10, 10, 0, 10),
						Padding = new Thickness(20, 0, 20, 0),
						Content = new TextBlock()
						{
							TextWrapping = TextWrapping.Wrap,
							Text = "Instance " + InstanceName + " has been removed."
						}
					});
				}
			}
		}

		private static void ProcessInstallEvents(string TempLogFileName, string TempLogFileName2, InstallationProcess Process)
		{
			try
			{
				File.Copy(TempLogFileName, TempLogFileName2, true);
				string Xml = File.ReadAllText(TempLogFileName2);
				if (!Xml.EndsWith("</EventOutput>"))
					Xml += "</EventOutput>";

				XmlDocument Doc = new();
				Doc.LoadXml(Xml);

				if (EventExtensions.TryParse(Doc, out Event[] ParsedEvents))
					Process.ProcessInstallEvents(true, ParsedEvents);
			}
			catch (Exception)
			{
				// Ignore
			}
		}

		private class InstallationProcess : EventSink
		{
			public ProgressWindow? Window = null;
			public List<Event> Events = [];
			public int NrFilesCopied = 0;
			public int NrFilesSkipped = 0;
			public int NrFilesDeleted = 0;
			public string InstallationStatus = string.Empty;
			private DateTime lastReport = DateTime.Now;

			public InstallationProcess()
				: base("Installation Process Event Sink")
			{
			}

			public override async Task Queue(Event Event)
			{
				this.ProcessInstallEvents(false, Event);

				if (this.Window is not null)
				{
					DateTime TP = DateTime.Now;

					if (TP.Subtract(this.lastReport).TotalMilliseconds >= 100)
					{
						this.lastReport = TP;

						await this.Window.Dispatcher.BeginInvoke(() =>
						{
							this.Window.ReportProgress(this.NrFilesCopied, this.NrFilesSkipped, this.NrFilesDeleted, this.InstallationStatus);
						});
					}
				}
			}

			public void ProcessInstallEvents(bool LogEvents, params Event[] ParsedEvents)
			{
				int c = ParsedEvents.Length;
				int NrEvents = LogEvents ? this.Events.Count : 0;

				for (int i = NrEvents; i < c; i++)
				{
					Event Event = ParsedEvents[i];

					this.Events.Add(Event);
				
					if (LogEvents)
						Log.Event(Event);

					switch (Event.EventId)
					{
						case "FileCopy":
							this.NrFilesCopied++;
							break;

						case "FileSkip":
							this.NrFilesSkipped++;
							break;

						case "FileDelete":
							this.NrFilesDeleted++;
							break;

						case "InstallationStatus":
							this.InstallationStatus = Event.Message;
							break;
					}
				}
			}
		}

		private async Task ShowInstallationResult(bool Errors, int? PortNumber)
		{
			await this.Dispatcher.BeginInvoke(() =>
			{
				this.Installing = false;
				this.Cursor = null;

				if (Errors)
				{
					MessageBox.Show(this, "There were errors during installation. Please check log file for details.", "Error",
						MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				}
				else if (PortNumber.HasValue)
				{
					MessageBox.Show(this, "Installation was successful.", "Success",
						MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

					OpenUrl("http://localhost:" + PortNumber.ToString() + "/");
				}
				else
				{
					MessageBox.Show(this, "Update or repair was successful.", "Success",
						MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				}
			});
		}

		/// <summary>
		/// Opens an URL in the currently selected browser using the Shell.
		/// </summary>
		/// <param name="Url">URL to open</param>
		public static void OpenUrl(string Url)
		{
			try
			{
				Uri _ = new(Url);   // Check syntax.

				Process.Start(new ProcessStartInfo()
				{
					FileName = Url,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				MessageBox.Show(instance, "Unable to open web page. Please check log file for details.", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
			}
		}

		private async Task ShowUninstallationResult(bool Errors)
		{
			await this.Dispatcher.BeginInvoke(() =>
			{
				this.Installing = false;
				this.Cursor = null;

				if (Errors)
				{
					MessageBox.Show(this, "There were errors during uninstallation. Please check log file for details.", "Error",
						MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				}
				else
				{
					MessageBox.Show(this, "Uninstallation was successful.", "Success",
						MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				}
			});
		}

		private void Hyperlink_RequestNavigate(object Sender, RequestNavigateEventArgs e)
		{
			try
			{
				if (Sender is Hyperlink Link)
				{
					if (Link.Tag is not string Url || string.IsNullOrEmpty(Url))
						Url = Link.NavigateUri.ToString();

					Log.Informational("Opening log file.");

					ProcessStartInfo StartInfo;

					if (Url.StartsWith("http"))
					{
						StartInfo = new ProcessStartInfo(Url)
						{
							UseShellExecute = true
						};
					}
					else
					{
						StartInfo = new("notepad.exe")
						{
							Arguments = Url
						};
					}

					Process.Start(StartInfo);
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// If setup tool is idle or not.
		/// </summary>
		/// <returns>If application is idle.</returns>
		public bool IsIdle()
		{
			return this.Idle;
		}

		/// <summary>
		/// If setup tool is idle or not.
		/// </summary>
		/// <param name="Parameter">Parameter</param>
		/// <returns>If application is idle.</returns>
		public bool IsIdle(object? Parameter)
		{
			return this.Idle;
		}

		/// <summary>
		/// Uninstalls an instance.
		/// </summary>
		/// <param name="Parameter">Instance name</param>
		public async void ExecuteUninstallInstance(object? Parameter)
		{
			try
			{
				if (Parameter is string InstanceName)
					await this.Uninstall(InstanceName);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				ShowError(ex);
			}
		}

		private async Task Uninstall(string InstanceName)
		{
			if (!this.InstallationCheck(true))
				return;

			if (MessageBox.Show(this, "Are you sure you want to uninstall the instance '" + InstanceName +
				"'? This operation cannot be undone, and all information stored in the instance will be lost, " +
				"including backups held by the instance. Before uninstalling the instance, make sure to make a backup, " +
				"and copy the backup to a secure location, that is not the instance program data folder. " +
				"You can also preferably test the backup before proceeding with the uninstallation of the instance. " +
				"Do you want to continue with the uninstallation?",
				"Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
			{
				return;
			}

			ClearPortStatus();

			InstallationProcess InstallationProcess = new();
			bool Errors = false;

			if (App.IsUserAdministrator())
			{
				Log.Register(InstallationProcess);

				this.Installing = true;
				try
				{
					this.Cursor = Cursors.Wait;
					App.Uninstall(InstanceName);

					await this.Dispatcher.BeginInvoke(() =>
					{
						this.RemoveUninstallAndUpdateOrRepairButtons(InstanceName);
					});
				}
				catch (Exception ex)
				{
					Errors = true;
					Log.Exception(ex);
				}
				finally
				{
					Log.Informational("Uninstallation completed.",
						new KeyValuePair<string, object>("NrFilesCopied", InstallationProcess.NrFilesCopied),
						new KeyValuePair<string, object>("NrFilesSkipped", InstallationProcess.NrFilesSkipped),
						new KeyValuePair<string, object>("NrFilesDeleted", InstallationProcess.NrFilesDeleted));

					Log.Unregister(InstallationProcess);
					await this.ShowUninstallationResult(Errors);
				}
			}
			else
			{
				this.Installing = true;
				this.Cursor = Cursors.Wait;

				this.LogEntries.Children.Clear();

				Task _ = Task.Run(async () =>
				{
					try
					{
						string FileName = Process.GetCurrentProcess().MainModule?.FileName
							?? Assembly.GetEntryAssembly()?.Location
							?? Assembly.GetExecutingAssembly().Location;
						string TempFileName = Path.GetTempFileName();
						string TempFileName2 = Path.GetTempFileName();
						string TempLogFileName = TempFileName + ".xml";
						string TempLogFileName2 = TempFileName2 + ".xml";

						StringBuilder Arguments = new();

						Arguments.Append("-uninst \"");
						Arguments.Append(InstanceName);
						Arguments.Append("\" -log \"");
						Arguments.Append(TempLogFileName);
						Arguments.Append('"');

						ProcessStartInfo StartInfo = new()
						{
							FileName = FileName,
							WorkingDirectory = Directory.GetCurrentDirectory(),
							Arguments = Arguments.ToString(),
							Verb = "runas", // Starts with administrator rights.
							CreateNoWindow = true,
							WindowStyle = ProcessWindowStyle.Hidden,
							UseShellExecute = true
						};

						Log.Informational("Executing uninstallation in process with administrative privileges.",
							new KeyValuePair<string, object>("FileName", StartInfo.FileName),
							new KeyValuePair<string, object>("WorkingDirectory", StartInfo.WorkingDirectory),
							new KeyValuePair<string, object>("Arguments", StartInfo.Arguments),
							new KeyValuePair<string, object>("Verb", StartInfo.Verb),
							new KeyValuePair<string, object>("RedirectStandardError", StartInfo.RedirectStandardError),
							new KeyValuePair<string, object>("RedirectStandardOutput", StartInfo.RedirectStandardOutput),
							new KeyValuePair<string, object>("CreateNoWindow", StartInfo.CreateNoWindow),
							new KeyValuePair<string, object>("WindowStyle", StartInfo.WindowStyle),
							new KeyValuePair<string, object>("UseShellExecute", StartInfo.UseShellExecute));

						Process P = Process.Start(StartInfo)
							?? throw new Exception("Unable to start installation process.");

						while (!P.HasExited)
						{
							ProcessInstallEvents(TempLogFileName, TempLogFileName2, InstallationProcess);
							await Task.Delay(100);
						}

						ProcessInstallEvents(TempLogFileName, TempLogFileName2, InstallationProcess);

						int ExitCode = P.ExitCode;

						if (ExitCode != 0)
						{
							Errors = true;
							Log.Error("Unable to uninstall instance.",
								new KeyValuePair<string, object>("ExitCode", ExitCode));
						}
						else
						{
							Log.Notice("Instance uninstalled.");
							await this.Dispatcher.BeginInvoke(() =>
							{
								this.RemoveUninstallAndUpdateOrRepairButtons(InstanceName);
							});
						}
					}
					catch (Exception ex)
					{
						Errors = true;
						Log.Exception(ex);
					}
					finally
					{
						await this.ShowUninstallationResult(Errors);
					}
				});
			}
		}

		/// <summary>
		/// Opens a link in the browser.
		/// </summary>
		/// <param name="Parameter">URL</param>
		public static void ExecuteOpenLink(object? Parameter)
		{
			try
			{
				if (Parameter is string URL)
					OpenUrl(URL);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				ShowError(ex);
			}
		}

		/// <summary>
		/// Updates or Repairs an instance.
		/// </summary>
		/// <param name="Parameter">Instance name</param>
		public async void ExecuteUpdateOrRepairInstance(object? Parameter)
		{
			try
			{
				if (Parameter is string InstanceName)
					await this.UpdateOrRepair(InstanceName);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				ShowError(ex);
			}
		}

		private async Task UpdateOrRepair(string InstanceName)
		{
			if (!this.InstallationCheck(true))
				return;

			if (MessageBox.Show(this, "Are you sure you want to update or repair the instance '" + InstanceName +
				"'? This operation will stop the service, and reinstall application files. Before updating or repairing the instance, " +
				"make sure to make a backup, and copy the backup to a secure location, that is not the instance program data folder. " +
				"Do you want to continue with the update or reparation?", "Confirmation",
				MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
			{
				return;
			}

			ClearPortStatus();

			string AppDataFolder = GetAppDataFolder(InstanceName);

			await this.Install(AppDataFolder, InstanceName, null);
		}

	}
}
