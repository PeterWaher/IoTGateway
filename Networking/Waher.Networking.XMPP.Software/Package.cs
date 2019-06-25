using System;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Information about a software package.
    /// </summary>
    public class Package
    {
        private string fileName = string.Empty;
        private byte[] signature = null;
        private string url = null;
        private DateTime published = DateTime.MinValue;
        private DateTime supercedes = DateTime.MinValue;
        private DateTime created = DateTime.MinValue;
        private long bytes = 0;

        /// <summary>
        /// Information about a software package.
        /// </summary>
        public Package()
        {
        }

        /// <summary>
        /// Name of software package file.
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        /// <summary>
        /// Digital signature of file. The client must know what signature method have been used, and what keys should
        /// be used to validate the signature.
        /// </summary>
        public byte[] Signature
        {
            get { return this.signature; }
            set { this.signature = value; }
        }

        /// <summary>
        /// URL to download software package.
        /// </summary>
        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        /// <summary>
        /// When the software package was published.
        /// </summary>
        public DateTime Published
        {
            get { return this.published; }
            set { this.published = value; }
        }

        /// <summary>
        /// If the package supercedes an earlier version of the same software package. If no such package,
        /// this property returns <see cref="DateTime.MinValue"/>.
        /// </summary>
        public DateTime Supercedes
        {
            get { return this.supercedes; }
            set { this.supercedes = value; }
        }

        /// <summary>
        /// When the first version of the software package was first published.
        /// </summary>
        public DateTime Created
        {
            get { return this.created; }
            set { this.created = value; }
        }

        /// <summary>
        /// Size of software package, in bytes.
        /// </summary>
        public long Bytes
        {
            get { return this.bytes; }
            set { this.bytes = value; }
        }

        /// <summary>
        /// Parses a package information element.
        /// </summary>
        /// <param name="Xml">XML definition.</param>
        /// <returns>Parsed package information.</returns>
        public static Package Parse(XmlElement Xml)
        {
            Package Package = new Package();

            foreach (XmlAttribute Attr in Xml.Attributes)
            {
                switch (Attr.Name)
                {
                    case "fileName":
                        Package.fileName = Attr.Value;
                        break;

                    case "url":
                        Package.url = Attr.Value;
                        break;

                    case "signature":
                        Package.signature = Convert.FromBase64String(Attr.Value);
                        break;

                    case "published":
                        if (XML.TryParse(Attr.Value, out DateTime TP))
                            Package.published = TP;
                        break;

                    case "supercedes":
                        if (XML.TryParse(Attr.Value, out TP))
                            Package.supercedes = TP;
                        break;

                    case "created":
                        if (XML.TryParse(Attr.Value, out TP))
                            Package.created = TP;
                        break;

                    case "bytes":
                        if (long.TryParse(Attr.Value, out long l))
                            Package.bytes = l;
                        break;
                }
            }

            return Package;
        }

    }
}
