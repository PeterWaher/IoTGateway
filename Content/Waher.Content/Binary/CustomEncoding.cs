using System;

namespace Waher.Content.Binary
{
    /// <summary>
    /// A custom encoded object.
    /// </summary>
    public class CustomEncoding
    {
        private readonly string contentType;
        private readonly byte[] encoded;
        private readonly Uri uri;

        /// <summary>
        /// A custom encoded object.
        /// </summary>
        /// <param name="ContentType">Internet Content-Type of encoded object.</param>
        /// <param name="Encoded">Encoded object.</param>
        public CustomEncoding(string ContentType, byte[] Encoded)
            : this(ContentType, Encoded, null)
        {
        }

		/// <summary>
		/// A custom encoded object.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type of encoded object.</param>
		/// <param name="Encoded">Encoded object.</param>
        /// <param name="Uri">Optional original URI of content.</param>
		public CustomEncoding(string ContentType, byte[] Encoded, Uri Uri)
        {
            this.contentType = ContentType;
			this.encoded = Encoded;
            this.uri = Uri;
        }

        /// <summary>
        /// Internet Content-Type of encoded object.
        /// </summary>
        public string ContentType => this.contentType;

        /// <summary>
        /// Encoded object.
        /// </summary>
        public byte[] Encoded => this.encoded;

        /// <summary>
        /// Optional original URI of content.
        /// </summary>
        public Uri Uri => this.uri;
    }
}
