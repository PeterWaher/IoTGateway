namespace Waher.Content.Binary
{
    /// <summary>
    /// A custom encoded object.
    /// </summary>
    public class CustomEncoding
    {
        private readonly string contentType;
        private readonly byte[] encoded;

        /// <summary>
        /// A custom encoded object.
        /// </summary>
        /// <param name="ContentType">Internet Content-Type of encoded object.</param>
        /// <param name="Encoded">Encoded object.</param>
        public CustomEncoding(string ContentType, byte[] Encoded)
        {
            this.contentType = ContentType;
			this.encoded = Encoded;
        }

        /// <summary>
        /// Internet Content-Type of encoded object.
        /// </summary>
        public string ContentType => this.contentType;

        /// <summary>
        /// Encoded object.
        /// </summary>
        public byte[] Encoded => this.encoded;
    }
}
