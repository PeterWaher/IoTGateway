using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Implements an XMPP interface for remote software updates.
    /// 
    /// The interface is defined in the IEEE XMPP IoT extensions:
    /// https://gitlab.com/IEEE-SA/XMPPI/IoT
    /// </summary>
    public class SoftwareUpdateClient : XmppExtension
    {
        /// <summary>
        /// Subscribing to a wildcard allows you to receive events when any software package is updated.
        /// </summary>
        public const string Wildcard = "*";

        private readonly string componentAddress;
        private readonly string packageFolder;

        /// <summary>
        /// urn:ieee:iot:swu:1.0
        /// </summary>
        public const string NamespaceSoftwareUpdates = "urn:ieee:iot:swu:1.0";

        /// <summary>
        /// Implements an XMPP interface for remote software updates.
        /// 
        /// The interface is defined in the IEEE XMPP IoT extensions:
        /// https://gitlab.com/IEEE-SA/XMPPI/IoT
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

            Client.RegisterMessageHandler("packageInfo", NamespaceSoftwareUpdates, this.PackageNotificationHandler, true);
            Client.RegisterMessageHandler("packageDeleted", NamespaceSoftwareUpdates, this.PackageDeletedNotificationHandler, false);
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            Client.UnregisterMessageHandler("packageInfo", NamespaceSoftwareUpdates, this.PackageNotificationHandler, true);
            Client.UnregisterMessageHandler("packageDeleted", NamespaceSoftwareUpdates, this.PackageDeletedNotificationHandler, false);
        }

        /// <summary>
        /// Implemented extensions.
        /// </summary>
        public override string[] Extensions => new string[] { };

        /// <summary>
        /// Component XMPP address.
        /// </summary>
        public string ComponentAddress
        {
            get { return this.componentAddress; }
        }

        /// <summary>
        /// Gets information about a software package.
        /// </summary>
        /// <param name="FileName">Filename of software package.</param>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void GetPackageInformation(string FileName, PackageEventHandler Callback, object State)
        {
            StringBuilder Xml = new StringBuilder();

            Xml.Append("<getPackageInfo xmlns='");
            Xml.Append(NamespaceSoftwareUpdates);
            Xml.Append("' fileName='");
            Xml.Append(XML.Encode(FileName));
            Xml.Append("'/>");

            this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
            {
                XmlElement E = e.FirstElement;
                Package PackageInfo = null;

                if (e.Ok && !(E is null) && E.LocalName == "packageInfo" && E.NamespaceURI == NamespaceSoftwareUpdates)
                    PackageInfo = Package.Parse(E);
                else
                    e.Ok = false;

                if (!(Callback is null))
                {
                    try
                    {
                        PackageEventArgs e2 = new PackageEventArgs(PackageInfo, e);
                        Callback(this, e2);
                    }
                    catch (Exception ex)
                    {
                        Log.Critical(ex);
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
        public Task<Package> GetPackageInformationAsync(string FileName)
        {
            TaskCompletionSource<Package> Result = new TaskCompletionSource<Package>();

            this.GetPackageInformation(FileName, (sender, e) =>
            {
                if (e.Ok)
                    Result.TrySetResult(e.Package);
                else
                    Result.SetException(new Exception(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get package information." : e.ErrorText));
            }, null);

            return Result.Task;
        }

        /// <summary>
        /// Gets information about available software packages.
        /// </summary>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void GetPackagesInformation(PackagesEventHandler Callback, object State)
        {
            StringBuilder Xml = new StringBuilder();

            Xml.Append("<getPackages xmlns='");
            Xml.Append(NamespaceSoftwareUpdates);
            Xml.Append("'/>");

            this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
            {
                XmlElement E = e.FirstElement;
                Package[] PackagesInfo = null;

                if (e.Ok && !(E is null) && E.LocalName == "packages" && E.NamespaceURI == NamespaceSoftwareUpdates)
                {
                    List<Package> Packages = new List<Package>();

                    foreach (XmlNode N in E.ChildNodes)
                    {
                        if (N is XmlElement E2 && E2.LocalName == "packageInfo" && E2.NamespaceURI == NamespaceSoftwareUpdates)
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
                        Callback(this, e2);
                    }
                    catch (Exception ex)
                    {
                        Log.Critical(ex);
                    }
                }

            }, State);
        }

        /// <summary>
        /// Gets information about available software packages.
        /// </summary>
        /// <returns>Information about available software packages.</returns>
        /// <exception cref="Exception">If information about packages were not accessible or granted.</exception>
        public Task<Package[]> GetPackagesAsync()
        {
            TaskCompletionSource<Package[]> Result = new TaskCompletionSource<Package[]>();

            this.GetPackagesInformation((sender, e) =>
            {
                if (e.Ok)
                    Result.TrySetResult(e.Packages);
                else
                    Result.SetException(new Exception(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get packages." : e.ErrorText));
            }, null);

            return Result.Task;
        }

        /// <summary>
        /// Subscribes to updates to a given software package.
        /// </summary>
        /// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
        /// events will be received when any software package is updated.</param>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void Subscribe(string FileName, IqResultEventHandler Callback, object State)
        {
            StringBuilder Xml = new StringBuilder();

            Xml.Append("<subscribe xmlns='");
            Xml.Append(NamespaceSoftwareUpdates);
            Xml.Append("' fileName='");
            Xml.Append(XML.Encode(FileName));
            Xml.Append("'/>");

            this.client.SendIqSet(this.componentAddress, Xml.ToString(), Callback, State);
        }

        /// <summary>
        /// Subscribes to updates to a given software package.
        /// </summary>
        /// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
        /// events will be received when any software package is updated.</param>
        /// <exception cref="Exception">If subscription could not be performed.</exception>
        public Task SubscribeAsync(string FileName)
        {
            TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

            this.Subscribe(FileName, (sender, e) =>
            {
                if (e.Ok)
                    Result.TrySetResult(true);
                else
                {
                    Result.SetException(new Exception(string.IsNullOrEmpty(e.ErrorText) ? "Unable to subscribe to software updates for " +
                        FileName + "." : e.ErrorText));
                }
            }, null);

            return Result.Task;
        }

        /// <summary>
        /// Unsubscribes to updates to a given software package.
        /// </summary>
        /// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
        /// events will be received when any software package is updated.</param>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void Unsubscribe(string FileName, IqResultEventHandler Callback, object State)
        {
            StringBuilder Xml = new StringBuilder();

            Xml.Append("<unsubscribe xmlns='");
            Xml.Append(NamespaceSoftwareUpdates);
            Xml.Append("' fileName='");
            Xml.Append(XML.Encode(FileName));
            Xml.Append("'/>");

            this.client.SendIqSet(this.componentAddress, Xml.ToString(), Callback, State);
        }

        /// <summary>
        /// Unsubscribes to updates to a given software package.
        /// </summary>
        /// <param name="FileName">Filename of software package. If using a <see cref="Wildcard"/>,
        /// events will be received when any software package is updated.</param>
        /// <exception cref="Exception">If unsubscription could not be performed.</exception>
        public Task UnsubscribeAsync(string FileName)
        {
            TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

            this.Unsubscribe(FileName, (sender, e) =>
            {
                if (e.Ok)
                    Result.TrySetResult(true);
                else
                {
                    Result.SetException(new Exception(string.IsNullOrEmpty(e.ErrorText) ? "Unable to unsubscribe from software updates for " +
                        FileName + "." : e.ErrorText));
                }
            }, null);

            return Result.Task;
        }

        /// <summary>
        /// Gets current software update subscriptions.
        /// </summary>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void GetSubscriptions(SubscriptionsEventHandler Callback, object State)
        {
            StringBuilder Xml = new StringBuilder();

            Xml.Append("<getSubscriptions xmlns='");
            Xml.Append(NamespaceSoftwareUpdates);
            Xml.Append("'/>");

            this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
            {
                XmlElement E = e.FirstElement;
                string[] FileNames = null;

                if (e.Ok && !(E is null) && E.LocalName == "subscriptions" && E.NamespaceURI == NamespaceSoftwareUpdates)
                {
                    List<string> Subscriptions = new List<string>();

                    foreach (XmlNode N in E.ChildNodes)
                    {
                        if (N is XmlElement E2 && E2.LocalName == "subscription" && E2.NamespaceURI == NamespaceSoftwareUpdates)
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
                        Callback(this, e2);
                    }
                    catch (Exception ex)
                    {
                        Log.Critical(ex);
                    }
                }
            }, State);
        }

        /// <summary>
        /// Gets current software update subscriptions.
        /// </summary>
        /// <returns>Array of file names of software packages with active subscriptions.</returns>
        /// <exception cref="Exception">If list of subscriptions could not be accessed or granted.</exception>
        public Task<string[]> GetSubscriptionsAsync()
        {
            TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

            this.GetSubscriptions((sender, e) =>
            {
                if (e.Ok)
                    Result.TrySetResult(e.FileNames);
                else
                    Result.SetException(new Exception(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get list of current subscriptions." : e.ErrorText));
            }, null);

            return Result.Task;
        }

        private async void PackageNotificationHandler(object Sender, MessageEventArgs e)
        {
            if (string.Compare(e.From, this.componentAddress, true) != 0)
                return;

            Package PackageInfo = Package.Parse(e.Content);
            PackageUpdatedEventArgs e2 = new PackageUpdatedEventArgs(PackageInfo, e);
            try
            {
                this.OnSoftwareUpdated?.Invoke(this, e2);
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
            }

            if (e2.Download)
            {
                try
                {
                    string FileName = Path.Combine(this.packageFolder, PackageInfo.FileName);

                    using (HttpClient WebClient = new HttpClient())
                    {
                        using (HttpResponseMessage Response = await WebClient.GetAsync(PackageInfo.Url, HttpCompletionOption.ResponseHeadersRead))
                        {
                            using (Stream Input = await Response.Content.ReadAsStreamAsync())
                            {
                                using (Stream Output = File.Create(FileName))
                                {
                                    await Input.CopyToAsync(Output);
                                }
                            }
                        }
                    }

                    this.OnSoftwareDownloaded?.Invoke(this, new PackageFileEventArgs(PackageInfo, FileName, e));
                }
                catch (Exception ex)
                {
                    Log.Critical(ex);
                }
            }
        }

        /// <summary>
        /// Event raised when new software has been made available on the server. Only events from the software package component
        /// will be raised. The event arguments contains a property to control if the software package is to be downloaded or not.
        /// </summary>
        public event PackageUpdatedEventHandler OnSoftwareUpdated = null;

        /// <summary>
        /// Event raised when new software has been downloaded.
        /// </summary>
        public event PackageFileEventHandler OnSoftwareDownloaded = null;

        private void PackageDeletedNotificationHandler(object Sender, MessageEventArgs e)
        {
            if (string.Compare(e.From, this.componentAddress, true) != 0)
                return;

            Package PackageInfo = Package.Parse(e.Content);
            PackageDeletedEventArgs e2 = new PackageDeletedEventArgs(PackageInfo, e);
            try
            {
                this.OnSoftwareDeleted?.Invoke(this, e2);
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
            }

            if (e2.Delete)
            {
                try
                {
                    string FileName = Path.Combine(this.packageFolder, PackageInfo.FileName);

                    if (File.Exists(FileName))
                    {
                        File.Delete(FileName);
                        this.OnDownloadedSoftwareDeleted?.Invoke(this, new PackageFileEventArgs(PackageInfo, FileName, e));
                    }
                }
                catch (Exception ex)
                {
                    Log.Critical(ex);
                }
            }
        }

        /// <summary>
        /// Event raised when a software package has been deleted on the server. Only events from the software package component
        /// will be raised. The event arguments contains a property to control if the software package is to be deleted or not.
        /// </summary>
        public event PackageDeletedEventHandler OnSoftwareDeleted = null;

        /// <summary>
        /// Event raised when a local software package file has been deleted.
        /// </summary>
        public event PackageFileEventHandler OnDownloadedSoftwareDeleted = null;

    }
}
