using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Settings;

namespace Waher.IoTGateway.App
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				// Ensure the current window is active
				Window.Current.Activate();
				Task.Run((Action)this.Init);
			}
		}

		private async void Init()
		{
			try
			{
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					if (e.IsTerminating)
					{
						using (StreamWriter w = File.CreateText(Path.Combine(Gateway.AppDataFolder, "UnhandledException.txt")))
						{
							w.Write("Type: ");

							if (e.ExceptionObject != null)
								w.WriteLine(e.ExceptionObject.GetType().FullName);
							else
								w.WriteLine("null");

							w.Write("Time: ");
							w.WriteLine(DateTime.Now.ToString());

							w.WriteLine();
							if (e.ExceptionObject is Exception ex)
							{
								w.WriteLine(ex.Message);
								w.WriteLine();
								w.WriteLine(ex.StackTrace);
							}
							else
							{
								if (e.ExceptionObject != null)
									w.WriteLine(e.ExceptionObject.ToString());

								w.WriteLine();
								w.WriteLine(Environment.StackTrace);
							}

							w.Flush();
						}
					}

					if (e.ExceptionObject is Exception ex2)
						Log.Critical(ex2);
					else if (e.ExceptionObject != null)
						Log.Critical(e.ExceptionObject.ToString());
					else
						Log.Critical("Unexpected null exception thrown.");
				};

				Gateway.GetDatabaseProvider += GetDatabase;
				Gateway.GetXmppClientCredentials += GetXmppClientCredentials;
				Gateway.XmppCredentialsUpdated += XmppCredentialsUpdated;
				Gateway.RegistrationSuccessful += RegistrationSuccessful;

				if (!Gateway.Start(false))
				{
					throw new Exception("Gateway being started in another process.");
				}
			}
			catch (Exception ex)
			{
				Log.Emergency(ex);

				MessageDialog Dialog = new MessageDialog(ex.Message, "Error");
				await MainPage.Instance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
					async () => await Dialog.ShowAsync());
			}
		}

		private static IDatabaseProvider GetDatabase(XmlElement DatabaseConfig)
		{
			if (CommonTypes.TryParse(DatabaseConfig.Attributes["encrypted"].Value, out bool Encrypted) && Encrypted)
				throw new Exception("Encrypted database storage not supported on this platform.");

			return new FilesProvider(Gateway.AppDataFolder + DatabaseConfig.Attributes["folder"].Value,
				DatabaseConfig.Attributes["defaultCollectionName"].Value,
				int.Parse(DatabaseConfig.Attributes["blockSize"].Value),
				int.Parse(DatabaseConfig.Attributes["blocksInCache"].Value),
				int.Parse(DatabaseConfig.Attributes["blobBlockSize"].Value), Encoding.UTF8,
				int.Parse(DatabaseConfig.Attributes["timeoutMs"].Value),
				false);
		}

		private static XmppCredentials GetXmppClientCredentials(string XmppConfigFileName)
		{
			string Host = RuntimeSettings.Get("XmppHost", "waher.se");
			int Port = (int)RuntimeSettings.Get("XmppPort", 5222);
			string UserName = RuntimeSettings.Get("XmppUserName", string.Empty);
			string PasswordHash = RuntimeSettings.Get("XmppPasswordHash", string.Empty);
			string PasswordHashMethod = RuntimeSettings.Get("XmppPasswordHashMethod", string.Empty);
			string ThingRegistry = RuntimeSettings.Get("XmppRegistry", string.Empty);
			string Provisioning = RuntimeSettings.Get("XmppProvisioning", string.Empty);
			string Key = string.Empty;
			string Secret = string.Empty;
			bool TrustCertificate = RuntimeSettings.Get("XmppTrustCertificate", false);
			bool AllowInsecure = RuntimeSettings.Get("XmppAllowInsecure", false);
			bool StorePassword = RuntimeSettings.Get("XmppStorePassword", false);
			bool CreateAccount = false;

			if (string.IsNullOrEmpty(Host) ||
				Port <= 0 || Port > ushort.MaxValue ||
				string.IsNullOrEmpty(UserName) ||
				string.IsNullOrEmpty(PasswordHash) ||
				string.IsNullOrEmpty(PasswordHashMethod))
			{
				while (true)
				{
					MainPage.Instance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
					{
						try
						{
							AccountDialog Dialog = new AccountDialog(Host, Port, UserName, TrustCertificate, AllowInsecure, StorePassword, false, Key, Secret);

							switch (await Dialog.ShowAsync())
							{
								case ContentDialogResult.Primary:
									Host = Dialog.Host;
									Port = Dialog.Port;
									UserName = Dialog.UserName;
									PasswordHash = Dialog.Password;
									PasswordHashMethod = string.Empty;
									TrustCertificate = Dialog.TrustServer;
									AllowInsecure = Dialog.AllowInsecure;
									StorePassword = Dialog.AllowStorePassword;
									CreateAccount = Dialog.AllowRegistration;
									Key = Dialog.Key;
									Secret = Dialog.Secret;
									break;

								case ContentDialogResult.Secondary:
									break;
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

					}).AsTask().Wait();

					using (XmppClient Client = new XmppClient(Host, Port, UserName, PasswordHash, "en", typeof(App).Assembly)
					{
						TrustServer = TrustCertificate,
						AllowCramMD5 = AllowInsecure,
						AllowDigestMD5 = AllowInsecure,
						AllowPlain = AllowInsecure,
						AllowScramSHA1 = AllowInsecure,
						AllowEncryption = AllowInsecure
					})
					{
						if (CreateAccount)
							Client.AllowRegistration(Key, Secret);

						ManualResetEvent Ok = new ManualResetEvent(false);
						ManualResetEvent Error = new ManualResetEvent(false);

						Client.OnStateChanged += (sender, State) =>
						{
							if (State == XmppState.Connected)
							{
								Client.SendServiceItemsDiscoveryRequest(Client.Domain, (sender2, e2) =>
								{
									if (e2.Ok)
									{
										Dictionary<string, bool> Items = new Dictionary<string, bool>();

										foreach (Item Item in e2.Items)
											Items[Item.JID] = true;

										foreach (Item Item in e2.Items)
										{
											Log.Informational("Checking " + Item.JID + ".");

											Client.SendServiceDiscoveryRequest(Item.JID, (sender3, e3) =>
											{
												string JID = (string)e3.State;

												if (e3.Ok)
												{
													if (e3.Features.ContainsKey(ThingRegistryClient.NamespaceDiscovery))
													{
														ThingRegistry = JID;
														Console.Out.WriteLine("Thing registry found.", ThingRegistry);
													}

													if (e3.Features.ContainsKey(ProvisioningClient.NamespaceProvisioningDevice))
													{
														Provisioning = JID;
														Console.Out.WriteLine("Provisioning server found.", Provisioning);
													}
												}

												Items.Remove(JID);
												if (Items.Count == 0)
													Ok.Set();

											}, Item.JID);
										}
									}
									else
										Ok.Set();
								}, null);
							}
						};

						Client.OnConnectionError += (sender, e) =>
						{
							Error.Set();
						};

						Log.Informational("Connecting to " + Host + ":" + Port.ToString());
						Client.Connect();

						switch (WaitHandle.WaitAny(new WaitHandle[] { Ok, Error }, 30000))
						{
							case 0:
								if (!StorePassword)
								{
									PasswordHash = Client.PasswordHash;
									PasswordHashMethod = Client.PasswordHashMethod;
								}

								RuntimeSettings.Set("XmppHost", Host);
								RuntimeSettings.Set("XmppPort", Port);
								RuntimeSettings.Set("XmppUserName", UserName);
								RuntimeSettings.Set("XmppPasswordHash", PasswordHash);
								RuntimeSettings.Set("XmppPasswordHashMethod", PasswordHashMethod);
								RuntimeSettings.Set("XmppTrustCertificate", TrustCertificate );
								RuntimeSettings.Set("XmppAllowInsecure", AllowInsecure);
								RuntimeSettings.Set("XmppStorePassword", StorePassword);
								RuntimeSettings.Set("XmppRegistry", ThingRegistry);
								RuntimeSettings.Set("XmppProvisioning", Provisioning);

								return new XmppCredentials()
								{
									Host = Host,
									Port = Port,
									Account = UserName,
									Password = PasswordHash,
									PasswordType = PasswordHashMethod,
									TrustServer = TrustCertificate,
									AllowCramMD5 = AllowInsecure,
									AllowDigestMD5 = AllowInsecure,
									AllowPlain = AllowInsecure,
									AllowScramSHA1 = true,
									AllowEncryption = true,
									AllowRegistration = false,
									FormSignatureKey = string.Empty,
									FormSignatureSecret = string.Empty,
									ThingRegistry = ThingRegistry,
									Provisioning = Provisioning,
									Events = string.Empty
								};

							case 1:
							default:
								break;
						}
					}
				}
			}
			else
			{
				return new XmppCredentials()
				{
					Host = Host,
					Port = Port,
					Account = UserName,
					Password = PasswordHash,
					PasswordType = PasswordHashMethod,
					TrustServer = TrustCertificate,
					AllowCramMD5 = AllowInsecure,
					AllowDigestMD5 = AllowInsecure,
					AllowPlain = AllowInsecure,
					AllowScramSHA1 = true,
					AllowEncryption = true,
					AllowRegistration = false,
					FormSignatureKey = string.Empty,
					FormSignatureSecret = string.Empty,
					ThingRegistry = ThingRegistry,
					Provisioning = Provisioning,
					Events = string.Empty
				};
			}

		}

		private static void XmppCredentialsUpdated(string XmppConfigFileName, XmppCredentials Credentials)
		{
			bool StorePassword = RuntimeSettings.Get("XmppStorePassword", false);

			if (!StorePassword)
			{
				RuntimeSettings.Set("XmppPasswordHash", Credentials.Password);
				RuntimeSettings.Set("XmppPasswordHashMethod", Credentials.PasswordType);
			}
		}

		private static void RegistrationSuccessful(MetaDataTag[] MetaData, RegistrationEventArgs e)
		{
			if (!e.IsClaimed)
			{
				string ClaimUrl = ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData);
				string FilePath = Path.Combine(Gateway.AppDataFolder, "Gateway.iotdisco");

				Log.Informational("Registration successful.");
				Log.Informational(ClaimUrl, new KeyValuePair<string, object>("Path", FilePath));

				File.WriteAllText(FilePath, ClaimUrl);
			}
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			Gateway.Stop();
			Log.Terminate();

			deferral.Complete();
		}
	}
}
