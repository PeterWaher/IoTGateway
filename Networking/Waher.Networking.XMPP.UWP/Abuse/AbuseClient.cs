using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Events;

namespace Waher.Networking.XMPP.Abuse
{
    /// <summary>
    /// Reason for blocking.
    /// </summary>
    public enum ReportingReason
    {
        /// <summary>
        /// Spam
        /// </summary>
        Spam,

        /// <summary>
        /// Abuse
        /// </summary>
        Abuse,

        /// <summary>
        /// Other reason.
        /// </summary>
        Other
    }

    /// <summary>
    /// Class implementing blocking (XEP-0191) and spam reporting (XEP-0377).
    /// </summary>
    public class AbuseClient : IDisposable
    {
        /// <summary>
        /// urn:xmpp:blocking
        /// </summary>
        public const string NamespaceBlocking = "urn:xmpp:blocking";

        /// <summary>
        /// urn:xmpp:reporting:0
        /// </summary>
        public const string NamespaceReporting = "urn:xmpp:reporting:0";

        /// <summary>
        /// urn:xmpp:reporting:reason:spam:0
        /// </summary>
        public const string NamespaceSpamReason = "urn:xmpp:reporting:reason:spam:0";

        /// <summary>
        /// urn:xmpp:reporting:reason:abuse:0
        /// </summary>
        public const string NamespaceAbuseReason = "urn:xmpp:reporting:reason:abuse:0";

        private SortedDictionary<string, bool> blockList = new SortedDictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        private XmppClient client;
        private bool supportsBlocking = false;
        private bool supportsReporting = false;
        private bool supportsSpamReason = false;
        private bool supportsAbuseReason = false;

        /// <summary>
        /// Class implementing blocking (XEP-0191) and spam reporting (XEP-0377).
        /// </summary>
        /// <param name="Client">XMPP Client.</param>
        public AbuseClient(XmppClient Client)
        {
            this.client = Client;
            this.client.RegisterIqSetHandler("block", NamespaceBlocking, this.BlockPushHandler, true);
            this.client.RegisterIqSetHandler("unblock", NamespaceBlocking, this.UnblockPushHandler, false);

            this.BeginSearchSupport();

            this.client.OnStateChanged += Client_OnStateChanged;
        }

        private void Client_OnStateChanged(object Sender, XmppState NewState)
        {
            switch (NewState)
            {
                case XmppState.Connected:
                    this.BeginSearchSupport();
                    break;

                case XmppState.Offline:
                case XmppState.Error:
                    this.supportsBlocking = false;
                    this.supportsReporting = false;
                    this.supportsSpamReason = false;
                    this.supportsAbuseReason = false;
                    break;
            }
        }

        /// <summary>
        /// XMPP Client.
        /// </summary>
        public XmppClient Client
        {
            get { return this.client; }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            this.client.UnregisterIqSetHandler("block", NamespaceBlocking, this.BlockPushHandler, true);
            this.client.UnregisterIqSetHandler("unblock", NamespaceBlocking, this.UnblockPushHandler, false);
        }

        private void BlockPushHandler(object Sender, IqEventArgs e)
        {
            XmlElement E;
            string JID;

            lock (this.blockList)
            {
                foreach (XmlNode N in e.Query.ChildNodes)
                {
                    E = N as XmlElement;
                    if (E != null && E.LocalName == "item")
                    {
                        JID = XML.Attribute(E, "jid");
                        this.blockList[JID] = true;
                    }
                }
            }
        }

        private void UnblockPushHandler(object Sender, IqEventArgs e)
        {
            XmlElement E;
            string JID;
            bool Found = false;

            lock (this.blockList)
            {
                foreach (XmlNode N in e.Query.ChildNodes)
                {
                    E = N as XmlElement;
                    if (E != null && E.LocalName == "item")
                    {
                        Found = true;
                        JID = XML.Attribute(E, "jid");
                        this.blockList.Remove(JID);
                    }
                }

                if (!Found)
                    this.blockList.Clear();
            }
        }

        private void BeginSearchSupport()
        {
            Client.SendServiceDiscoveryRequest(Client.Domain, (sender, e) =>
            {
                if (e.Ok)
                {
                    this.supportsBlocking = e.HasFeature(NamespaceBlocking);
                    this.supportsReporting = e.HasFeature(NamespaceReporting);
                    this.supportsSpamReason = e.HasFeature(NamespaceSpamReason);
                    this.supportsAbuseReason = e.HasFeature(NamespaceAbuseReason);

                    if (this.supportsBlocking)
                        this.StartGetBlockList(null, null);
                }
                else
                {
                    this.supportsBlocking = false;
                    this.supportsReporting = false;
                    this.supportsSpamReason = false;
                    this.supportsAbuseReason = false;
                }
            }, null);
        }

        /// <summary>
        /// If the server supports the blocking extension.
        /// </summary>
        public bool SupportsBlocking
        {
            get { return this.supportsBlocking; }
        }

        /// <summary>
        /// If the server supports reporting.
        /// </summary>
        public bool SupportsReporting
        {
            get { return this.supportsReporting; }
        }

        /// <summary>
        /// If the server supports spam reporting.
        /// </summary>
        public bool SupportsSpamReporting
        {
            get { return this.supportsSpamReason; }
        }

        /// <summary>
        /// If the server supports abuse reporting.
        /// </summary>
        public bool SupportsAbuseReporting
        {
            get { return this.supportsAbuseReason; }
        }

        public string[] BlockedJIDs
        {
            get
            {
                string[] Result;

                lock (this.blockList)
                {
                    Result = new string[this.blockList.Count];
                    this.blockList.Keys.CopyTo(Result, 0);
                }

                return Result;
            }
        }

        /// <summary>
        /// Gets the block-list from the server.
        /// </summary>
        /// <param name="Callback">Callback method to call when response is available.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void StartGetBlockList(BlockListEventHandler Callback, object State)
        {
            this.client.SendIqGet(string.Empty, "<blocklist xmlns='" + NamespaceBlocking + "'/>", (sender, e) =>
            {
                List<string> JIDs = new List<string>();
                XmlElement E, E2;
                string JID;

                if (e.Ok && (E = e.Response) != null && E.LocalName == "blocklist" && E.NamespaceURI == NamespaceBlocking)
                {
                    lock (this.blockList)
                    {
                        this.blockList.Clear();

                        foreach (XmlNode N in e.Response.ChildNodes)
                        {
                            E2 = N as XmlElement;
                            if (E2 != null && E2.LocalName == "item")
                            {
                                JID = XML.Attribute(E2, "jid");
                                if (!string.IsNullOrEmpty(JID))
                                {
                                    this.blockList[JID] = true;
                                    JIDs.Add(JID);
                                }
                            }
                        }
                    }
                }

                if (Callback != null)
                {
                    BlockListEventArgs e2 = new BlockListEventArgs(e, JIDs.ToArray(), State);

                    try
                    {
                        Callback(this, e2);
                    }
                    catch (Exception ex)
                    {
                        Log.Critical(ex);
                    }
                }

            }, null);
        }

        /// <summary>
        /// Checks if a JID is in the block list.
        /// </summary>
        /// <param name="JID">JID to check.</param>
        /// <returns>If JID is in block list.</returns>
        public bool IsBlocked(string JID)
        {
            lock (this.blockList)
            {
                return this.blockList.ContainsKey(JID);
            }
        }

        /// <summary>
        /// Blocks a JID
        /// </summary>
        /// <param name="JID">JID to block.</param>
        /// <param name="Reason">Reason for blocking JID.</param>
        /// <param name="Callback">Callback method.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void BlockJID(string JID, ReportingReason Reason, IqResultEventHandler Callback, object State)
        {
            bool Blocked;

            lock (this.blockList)
            {
                if (!this.blockList.TryGetValue(JID, out Blocked))
                    Blocked = false;
            }

            if (Blocked)
                this.CallCallback(Callback, State, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, true, State));
            else if (this.supportsBlocking)
            {
                StringBuilder Xml = new StringBuilder();

                Xml.Append("<block xmlns='");
                Xml.Append(NamespaceBlocking);
                Xml.Append("'><item jid='");
                Xml.Append(JID);

                if (!this.supportsReporting)
                    Xml.Append("'/></block>");
                else if (Reason == ReportingReason.Spam && this.supportsSpamReason)
                {
                    Xml.Append("'><report xmlns='");
                    Xml.Append(NamespaceReporting);
                    Xml.Append("'><spam/></report></item></block>");
                }
                else if (Reason == ReportingReason.Abuse && this.supportsAbuseReason)
                {
                    Xml.Append("'><report xmlns='");
                    Xml.Append(NamespaceReporting);
                    Xml.Append("'><abuse/></report></item></block>");
                }
                else
                    Xml.Append("'/></block>");

                this.client.SendIqSet(this.client.Domain, Xml.ToString(), (sender, e) =>
                {
                    if (e.Ok)
                    {
                        lock (this.blockList)
                        {
                            this.blockList[JID] = true;
                        }
                    }

                    this.CallCallback(Callback, State, e);

                }, null);
            }
            else
                this.CallCallback(Callback, State, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State));
        }

        private void CallCallback(IqResultEventHandler Callback, object State, IqResultEventArgs e)
        {
            if (Callback != null)
            {
                try
                {
                    Callback(this, e);
                }
                catch (Exception ex)
                {
                    Log.Critical(ex);
                }
            }
        }

        /// <summary>
        /// Unblocks a JID
        /// </summary>
        /// <param name="JID">JID to unblock.</param>
        /// <param name="Callback">Callback method.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void UnblockJID(string JID, IqResultEventHandler Callback, object State)
        {
            bool Blocked;

            lock (this.blockList)
            {
                if (!this.blockList.TryGetValue(JID, out Blocked))
                    Blocked = false;
            }

            if (!Blocked)
                this.CallCallback(Callback, State, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, true, State));
            else if (this.supportsBlocking)
            {
                this.client.SendIqSet(this.client.Domain, "<unblock xmlns='" + NamespaceBlocking + "'><item jid='" + JID + "'/></unblock>", (sender, e) =>
                {
                    if (e.Ok)
                    {
                        lock (this.blockList)
                        {
                            this.blockList.Remove(JID);
                        }
                    }

                    this.CallCallback(Callback, State, e);

                }, null);
            }
            else
                this.CallCallback(Callback, State, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State));
        }

        /// <summary>
        /// Unblocks all JIDs
        /// </summary>
        /// <param name="Callback">Callback method.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public void UnblockAll(string JID, IqResultEventHandler Callback, object State)
        {
            if (this.supportsBlocking)
            {
                this.client.SendIqSet(this.client.Domain, "<unblock xmlns='" + NamespaceBlocking + "'/>", (sender, e) =>
                {
                    if (e.Ok)
                    {
                        lock (this.blockList)
                        {
                            this.blockList.Clear();
                        }
                    }

                    this.CallCallback(Callback, State, e);

                }, null);
            }
            else
                this.CallCallback(Callback, State, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State));
        }

    }
}
