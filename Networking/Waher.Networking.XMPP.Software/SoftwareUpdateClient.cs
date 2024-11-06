using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Software
{
	/// <summary>
	/// Implements an XMPP interface for remote software updates.
	/// 
	/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
	/// https://neuro-foundation.io
	/// </summary>
	public class SoftwareUpdateClient : XmppExtension
	{
		/// <summary>
		/// Subscribing to a wildcard allows you to receive events when any software package is updated.
		/// </summary>
		public const string Wildcard = "*";

		private readonly string componentAddress;
		private readonly string packageFolder;
		private readonly Random rnd = new Random();

		/// <summary>
		/// urn:ieee:iot:swu:1.0
		/// </summary>
		public const string NamespaceSoftwareUpdatesIeeeV1 = "urn:ieee:iot:swu:1.0";

		/// <summary>
		/// urn:nf:iot:swu:1.0
		/// </summary>
		public const string NamespaceSoftwareUpdatesNeuroFoundationV1 = "urn:nf:iot:swu:1.0";

		/// <summary>
		/// Current namespace for software updates.
		/// </summary>
		public const string NamespaceSoftwareUpdatesCurrent = NamespaceSoftwareUpdatesNeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for software updates.
		/// </summary>
		public static readonly string[] NamespacesSoftwareUpdates = new string[]
		{
			NamespaceSoftwareUpdatesIeeeV1,
			NamespaceSoftwareUpdatesNeuroFoundationV1
		};

		/// <summary>
		/// Implements an XMPP interface for remote software updates.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ComponentAddress">Component XMPP Address.</param>
		public SoftwareUpdateClient(XmppClient Client, string ComponentAddress, string PackageFolder)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;
			this.packageFolder = PackageFolder;

			if (!Directory.Exists(PackageFolder))
				Directory.CreateDirectory(PackageFolder);

			#region Neuro-Foundation V1

			this.Client.RegisterMessageHandler("packageInfo", NamespaceSoftwareUpdatesNeuroFoundationV1, this.PackageNotificationHandler, true);
			this.Client.RegisterMessageHandler("packageDeleted", NamespaceSoftwareUpdatesNeuroFoundationV1, this.PackageDeletedNotificationHandler, false);

			#endregion

			#region IEEE V1

			this.Client.RegisterMessageHandler("packageInfo", NamespaceSoftwareUpdatesIeeeV1, this.PackageNotificationHandler, true);
			this.Client.RegisterMessageHandler("packageDeleted", NamespaceSoftwareUpdatesIeeeV1, this.PackageDeletedNotificationHandler, false);

			#endregion
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			#region Neuro-Foundation V1

			this.Client.UnregisterMessageHandler("packageInfo", NamespaceSoftwareUpdatesNeuroFoundationV1, this.PackageNotificationHandler, true);
			this.Client.UnregisterMessageHandler("packageDeleted", NamespaceSoftwareUpdatesNeuroFoundationV1, this.PackageDeletedNotificationHandler, false);

			#endregion

			#region IEEE V1

			this.Client.UnregisterMessageHandler("packageInfo", NamespaceSoftwareUpdatesIeeeV1, this.PackageNotificationHandler, true);
			this.Client.UnregisterMessageHandler("packageDeleted", NamespaceSoftwareUpdatesIeeeV1, this.PackageDeletedNotificationHandler, false);

			#endregion
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { };

		/// <summary>
		/// Component XMPP address.
		/// </summary>
		public string ComponentAddress => this.componentAddress;

		/// <summary>
		/// Folder of downloaded packages.
		/// </summary>
		public string PackageFolder => this.packageFolder;

		/// <summary>
		/// Gets information about a software package.
		/// </summary>
		/// <param name="FileName">Filename of software package.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetPackageInformation(string FileName, EventHandlerAsync<PackageEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getPackageInfo xmlns='");
			Xml.Append(NamespaceSoftwareUpdatesCurrent);
			Xml.Append("' fileName='");
			Xml.Append(XML.Encode(FileName));
			Xml.Append("'/>");

			return this.client.SendIqGet(this.componentAddress, Xml.ToString(), async (sender, e) =>
			{
				XmlElement E = e.FirstElement;
				Package PackageInfo = null;

				if (e.Ok && !(E is null) && E.LocalName == "packageInfo")
					PackageInfo = Package.Parse(E);
				else
					e.Ok = false;

				if (!(Callback is null))
				{
					try
					{
						PackageEventArgs e2 = new PackageEventArgs(PackageInfo, e);
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}

			}, State);
		}

		/// <summary>
		/// Gets information about a software package.
		/// </summary>
		/// <param name="FileName">Filename of software package.</param>
		/// <returns>Information about software package.</returns>
		/// <exception cref="Exception">If information about package was not accessible or granted.</exception>
		public async Task<Package> GetPackageInformationAsync(string FileName)
		{
			TaskCompletionSource<Package> Result = new TaskCompletionSource<Package>();

			await this.GetPackageInformation(FileName, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Package);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get package information."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets information about available software packages.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetPackagesInformation(EventHandlerAsync<PackagesEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getPackages xmlns='");
			Xml.Append(NamespaceSoftwareUpdatesCurrent);
			Xml.Append("'/>");

			return this.client.SendIqGet(this.componentAddress, Xml.ToString(), async (sender, e) =>
			{
				XmlElement E = e.FirstElement;
				Package[] PackagesInfo = null;

				if (e.Ok && !(E is null) && E.LocalName == "packages")
				{
					List<Package> Packages = new List<Package>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "packageInfo")
							Packages.Add(Package.Parse(E2));
					}

					PackagesInfo = Packages.ToArray();
				}
				else
					e.Ok = false;

				if (!(Callback is null))
				{
					try
					{
						PackagesEventArgs e2 = new PackagesEventArgs(PackagesInfo, e);
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}

			}, State);
		}

		/// <summary>
		/// Gets information about available software packages.
		/// </summary>
		/// <returns>Information about available software packages.</returns>
		/// <exception cref="Exception">If information about packages were not accessible or granted.</exception>
		public async Task<Package[]> GetPackagesAsync()
		{
			TaskCompletionSource<Package[]> Result = new TaskCompletionSource<Package[]>();

			await this.GetPackagesInformation((sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Packages);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get packages."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Subscribes to updates to a given software package.
		/// </summary>
		/// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
		/// events will be received when any software package is updated.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string FileName, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<subscribe xmlns='");
			Xml.Append(NamespaceSoftwareUpdatesCurrent);
			Xml.Append("' fileName='");
			Xml.Append(XML.Encode(FileName));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Subscribes to updates to a given software package.
		/// </summary>
		/// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
		/// events will be received when any software package is updated.</param>
		/// <exception cref="Exception">If subscription could not be performed.</exception>
		public async Task SubscribeAsync(string FileName)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.Subscribe(FileName, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to subscribe to software updates for " + FileName + "."));

				return Task.CompletedTask;

			}, null);

			await Result.Task;
		}

		/// <summary>
		/// Unsubscribes to updates to a given software package.
		/// </summary>
		/// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
		/// events will be received when any software package is updated.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Unsubscribe(string FileName, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<unsubscribe xmlns='");
			Xml.Append(NamespaceSoftwareUpdatesCurrent);
			Xml.Append("' fileName='");
			Xml.Append(XML.Encode(FileName));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Unsubscribes to updates to a given software package.
		/// </summary>
		/// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
		/// events will be received when any software package is updated.</param>
		/// <exception cref="Exception">If unsubscription could not be performed.</exception>
		public async Task UnsubscribeAsync(string FileName)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.Unsubscribe(FileName, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
				{
					Result.SetException(e.StanzaError ?? new Exception("Unable to unsubscribe from software updates for " + FileName + "."));
				}

				return Task.CompletedTask;

			}, null);

			await Result.Task;
		}

		/// <summary>
		/// Gets current software update subscriptions.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptions(EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getSubscriptions xmlns='");
			Xml.Append(NamespaceSoftwareUpdatesCurrent);
			Xml.Append("'/>");

			return this.client.SendIqGet(this.componentAddress, Xml.ToString(), async (sender, e) =>
			{
				XmlElement E = e.FirstElement;
				string[] FileNames = null;

				if (e.Ok && !(E is null) && E.LocalName == "subscriptions")
				{
					List<string> Subscriptions = new List<string>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "subscription")
							Subscriptions.Add(E2.InnerText);
					}

					FileNames = Subscriptions.ToArray();
				}
				else
					e.Ok = false;

				if (!(Callback is null))
				{
					try
					{
						SubscriptionsEventArgs e2 = new SubscriptionsEventArgs(FileNames, e);
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}, State);
		}

		/// <summary>
		/// Gets current software update subscriptions.
		/// </summary>
		/// <returns>Array of file names of software packages with active subscriptions.</returns>
		/// <exception cref="Exception">If list of subscriptions could not be accessed or granted.</exception>
		public async Task<string[]> GetSubscriptionsAsync()
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			await this.GetSubscriptions((sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.FileNames);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get list of current subscriptions."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		private async Task PackageNotificationHandler(object Sender, MessageEventArgs e)
		{
			if (string.Compare(e.From, this.componentAddress, true) != 0)
			{
				await this.client.Warning("Discarding package notification. Expected source: " + this.componentAddress + ". Actual source: " + e.From);
				return;
			}

			Package PackageInfo = Package.Parse(e.Content);
			PackageUpdatedEventArgs e2 = new PackageUpdatedEventArgs(PackageInfo, e);

			await this.OnSoftwareUpdated.Raise(this, e2);

			if (e2.Download)
			{
				Task _ = Task.Run(() => this.Download(PackageInfo, e));
			}
		}

		private async Task Download(Package PackageInfo, MessageEventArgs e)
		{
			try
			{
				string FileName = await this.DownloadPackageAsync(PackageInfo);
				PackageFileEventArgs e3 = new PackageFileEventArgs(PackageInfo, FileName, e);

				if (!await this.OnSoftwareValidation.Raise(this, e3))
				{
					File.Delete(FileName);
					Log.Warning("Package with invalid signature downloaded and deleted.", FileName);
					return;
				}

				await this.OnSoftwareDownloaded.Raise(this, e3);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Downloads a software package.
		/// </summary>
		/// <param name="PackageInfo">Information about the software package.</param>
		/// <returns>Filename of downloaded file.</returns>
		/// <exception cref="IOException">If package no longer exists, or is not acceissible, and therefore could not be downloaded.</exception>
		public async Task<string> DownloadPackageAsync(Package PackageInfo)
		{
			string FileName = Path.Combine(this.packageFolder, PackageInfo.FileName);
			int MaxRetryDelayMinutes = 5;

			while (true)
			{
				HttpStatusCode StatusCode;

				try
				{
					using (HttpClient WebClient = new HttpClient())
					{
						using (HttpResponseMessage Response = await WebClient.GetAsync(PackageInfo.Url, HttpCompletionOption.ResponseHeadersRead))
						{
							if (Response.IsSuccessStatusCode)
							{
								using (Stream Input = await Response.Content.ReadAsStreamAsync())
								{
									using (Stream Output = File.Create(FileName))
									{
										await Input.CopyToAsync(Output);
									}
								}

								return FileName;
							}
							else
								StatusCode = Response.StatusCode;
						}
					}
				}
				catch (Exception)
				{
					StatusCode = HttpStatusCode.InternalServerError;
				}

				if ((int)StatusCode < 500)
				{
					throw new IOException("Unable to download new package from server. HTTP Status Code returned: " +
						  StatusCode.ToString() + " (" + ((int)StatusCode).ToString() + ")");
				}

				int MsDelay;

				lock (this.rnd)
				{
					MsDelay = this.rnd.Next(1000, MaxRetryDelayMinutes * 60000);

					MaxRetryDelayMinutes <<= 1;
					if (MaxRetryDelayMinutes > 1440)
						MaxRetryDelayMinutes = 1440;    // Try at least once a day.
				}

				await Task.Delay(MsDelay);
			}
		}

		/// <summary>
		/// Event raised when new software has been made available on the server. Only events from the software package component
		/// will be raised. The event arguments contains a property to control if the software package is to be downloaded or not.
		/// </summary>
		public event EventHandlerAsync<PackageUpdatedEventArgs> OnSoftwareUpdated = null;

		/// <summary>
		/// Event raised just after physically downloading a file, but before raising the 
		/// <see cref="OnSoftwareDownloaded"/> event. Can be used by clients to validate
		/// the signature of the file. If the signature is found to be invalid, an exception
		/// must be thrown by the event handler.
		/// 
		/// Note that the software client does not know how to validate files. It only provides 
		/// information about the signature used by the packet creator. How to interpret the 
		/// signature, is packet/manufacturer-specific.
		/// </summary>
		public event EventHandlerAsync<PackageFileEventArgs> OnSoftwareValidation = null;

		/// <summary>
		/// Event raised when new software has been downloaded.
		/// </summary>
		public event EventHandlerAsync<PackageFileEventArgs> OnSoftwareDownloaded = null;

		private async Task PackageDeletedNotificationHandler(object Sender, MessageEventArgs e)
		{
			if (string.Compare(e.From, this.componentAddress, true) != 0)
				return;

			Package PackageInfo = Package.Parse(e.Content);
			PackageDeletedEventArgs e2 = new PackageDeletedEventArgs(PackageInfo, e);
			await this.OnSoftwareDeleted.Raise(this, e2);

			if (e2.Delete)
			{
				try
				{
					string FileName = Path.Combine(this.packageFolder, PackageInfo.FileName);

					if (File.Exists(FileName))
					{
						File.Delete(FileName);

						await this.OnDownloadedSoftwareDeleted.Raise(this, new PackageFileEventArgs(PackageInfo, FileName, e));
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a software package has been deleted on the server. Only events from the software package component
		/// will be raised. The event arguments contains a property to control if the software package is to be deleted or not.
		/// </summary>
		public event EventHandlerAsync<PackageDeletedEventArgs> OnSoftwareDeleted = null;

		/// <summary>
		/// Event raised when a local software package file has been deleted.
		/// </summary>
		public event EventHandlerAsync<PackageFileEventArgs> OnDownloadedSoftwareDeleted = null;

	}
}
