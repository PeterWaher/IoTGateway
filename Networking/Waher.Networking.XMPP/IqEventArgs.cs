using System;
using System.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP
{
    /// <summary>
    /// Event arguments for IQ queries.
    /// </summary>
    public class IqEventArgs : EventArgs
    {
        private readonly XmppClient client;
        private readonly XmppComponent component;
        private readonly IEndToEndEncryption e2eEncryption;
        private readonly IE2eSymmetricCipher e2eSymmetricCipher;
        private readonly XmlElement iq;
        private XmlElement query = null;
        private readonly string id;
        private readonly string to;
        private string toBareJid = null;
        private readonly string from;
        private string fromBareJid = null;
        private readonly string e2eReference;

        /// <summary>
        /// Event arguments for IQ queries.
        /// </summary>
        protected IqEventArgs(IqEventArgs e)
        {
            this.client = e.client;
            this.component = e.component;
            this.iq = e.iq;
            this.id = e.id;
            this.to = e.to;
            this.from = e.from;
            this.e2eEncryption = e.e2eEncryption;
            this.e2eReference = e.e2eReference;
            this.e2eSymmetricCipher = e.e2eSymmetricCipher;
        }

        /// <summary>
        /// Event arguments for IQ queries.
        /// </summary>
        /// <param name="Client">XMPP Client.</param>
        /// <param name="Iq">IQ element.</param>
        /// <param name="Id">Id attribute of IQ stanza.</param>
        /// <param name="To">To attribute of IQ stanza.</param>
        /// <param name="From">From attribute of IQ stanza.</param>
        public IqEventArgs(XmppClient Client, XmlElement Iq, string Id, string To, string From)
        {
            this.client = Client;
            this.component = null;
            this.e2eEncryption = null;
            this.e2eReference = null;
            this.e2eSymmetricCipher = null;
            this.iq = Iq;
            this.id = Id;
            this.to = To;
            this.from = From;
        }

        /// <summary>
        /// Event arguments for IQ queries.
        /// </summary>
        /// <param name="Component">XMPP Component.</param>
        /// <param name="Iq">IQ element.</param>
        /// <param name="Id">Id attribute of IQ stanza.</param>
        /// <param name="To">To attribute of IQ stanza.</param>
        /// <param name="From">From attribute of IQ stanza.</param>
        public IqEventArgs(XmppComponent Component, XmlElement Iq, string Id, string To, string From)
        {
            this.client = null;
            this.component = Component;
            this.iq = Iq;
            this.id = Id;
            this.to = To;
            this.from = From;
            this.e2eEncryption = null;
            this.e2eReference = null;
            this.e2eSymmetricCipher = null;
        }

        /// <summary>
        /// Event arguments for IQ queries.
        /// </summary>
        /// <param name="Client">XMPP Client.</param>
        /// <param name="E2eEncryption">End-to-end encryption algorithm used.</param>
        /// <param name="E2eReference">Reference to End-to-end encryption endpoint used.</param>
        /// <param name="E2eSymmetricCipher">Type of symmetric cipher used in E2E encryption.</param>
        /// <param name="Iq">IQ element.</param>
        /// <param name="Id">Id attribute of IQ stanza.</param>
        /// <param name="To">To attribute of IQ stanza.</param>
        /// <param name="From">From attribute of IQ stanza.</param>
        public IqEventArgs(XmppClient Client, IEndToEndEncryption E2eEncryption, 
            string E2eReference, IE2eSymmetricCipher E2eSymmetricCipher, XmlElement Iq,
            string Id, string To, string From)
        {
            this.client = Client;
            this.component = null;
            this.e2eEncryption = E2eEncryption;
            this.e2eReference = E2eReference;
            this.e2eSymmetricCipher = E2eSymmetricCipher;
            this.iq = Iq;
            this.id = Id;
            this.to = To;
            this.from = From;
        }

        /// <summary>
        /// IQ element.
        /// </summary>
        public XmlElement IQ { get { return this.iq; } }

        /// <summary>
        /// Query element, if found, null otherwise.
        /// </summary>
        public XmlElement Query
        {
            get { return this.query; }
            internal set { this.query = value; }
        }

        /// <summary>
        /// ID of the request.
        /// </summary>
        public string Id { get { return this.id; } }

        /// <summary>
        /// To address attribute
        /// </summary>
        public string To { get { return this.to; } }

        /// <summary>
        /// From address attribute
        /// </summary>
        public string From { get { return this.from; } }

        /// <summary>
        /// Bare version of the "to" JID.
        /// </summary>
        public string ToBareJid
        {
            get
            {
                if (this.toBareJid is null)
                    this.toBareJid = XmppClient.GetBareJID(this.to);

                return this.toBareJid;
            }
        }

        /// <summary>
        /// Bare version of the "from" JID.
        /// </summary>
        public string FromBareJid
        {
            get
            {
                if (this.fromBareJid is null)
                    this.fromBareJid = XmppClient.GetBareJID(this.from);

                return this.fromBareJid;
            }
        }

        /// <summary>
        /// If end-to-end encryption was used in the request.
        /// </summary>
        public bool UsesE2eEncryption
        {
            get { return this.e2eEncryption != null; }
        }

        /// <summary>
        /// End-to-end encryption interface, if used in the request.
        /// </summary>
        public IEndToEndEncryption E2eEncryption
        {
            get { return this.e2eEncryption; }
        }

        /// <summary>
        /// Reference to End-to-end encryption endpoint used.
        /// </summary>
        public string E2eReference
        {
            get { return this.e2eReference; }
        }

        /// <summary>
        /// Type of symmetric cipher used in E2E encryption.
        /// </summary>
        public IE2eSymmetricCipher E2eSymmetricCipher
        {
            get { return this.e2eSymmetricCipher; }
        }

        /// <summary>
        /// Returns a response to the current request.
        /// </summary>
        /// <param name="Xml">XML to embed into the response.</param>
        public void IqResult(string Xml)
        {
            if (!(this.e2eEncryption is null))
                this.e2eEncryption.SendIqResult(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, Xml);
            else if (!(this.client is null))
                this.client.SendIqResult(this.id, this.from, Xml);
            else
                this.component.SendIqResult(this.id, this.to, this.from, Xml);
        }

        /// <summary>
        /// Returns an error response to the current request.
        /// </summary>
        /// <param name="Xml">XML to embed into the response.</param>
        public void IqError(string Xml)
        {
            if (!(this.e2eEncryption is null))
                this.e2eEncryption.SendIqError(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, Xml);
            else if (!(this.client is null))
                this.client.SendIqError(this.id, this.from, Xml);
            else
                this.component.SendIqError(this.id, this.to, this.from, Xml);
        }

        /// <summary>
        /// Returns an error response to the current request.
        /// </summary>
        /// <param name="ex">Internal exception object.</param>
        public void IqError(Exception ex)
        {
            try
            {
                if (!(this.e2eEncryption is null))
                    this.e2eEncryption.SendIqError(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, ex);
                else if (!(this.client is null))
                    this.client.SendIqError(this.id, this.from, ex);
                else
                    this.component.SendIqError(this.id, this.to, this.from, ex);
            }
            catch (Exception ex2)
            {
                Log.Critical(ex2);
            }
        }
    }
}
