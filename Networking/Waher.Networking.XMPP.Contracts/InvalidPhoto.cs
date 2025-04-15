namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents an invalidated photo.
	/// </summary>
	public class InvalidPhoto
	{
		/// <summary>
		/// Represents an invalidated photo.
		/// </summary>
		/// <param name="FileName">File name of Invalidated photo.</param>
		/// <param name="Reason">Reason for invalidating photo.</param>
		/// <param name="ReasonLanguage">ISO code of language used for <paramref name="Reason"/>.</param>
		/// <param name="ReasonCode">A machine-readable code for the reason for invalidating 
		/// the photo. (Each service can define its own reason codes.)</param>
		/// <param name="Service">Service validating photo.</param>
		internal InvalidPhoto(string FileName, string Reason, string ReasonLanguage, 
			string ReasonCode, string Service)
		{
			this.FileName = FileName;
			this.Reason = Reason;
			this.ReasonLanguage = ReasonLanguage;
			this.ReasonCode = ReasonCode;
			this.Service = Service;
		}

		/// <summary>
		/// File name of Invalidated photo.
		/// </summary>
		public string FileName { get; }

		/// <summary>
		/// Service invalidating photo.
		/// </summary>
		public string Service { get; }

		/// <summary>
		/// Reason for invalidating photo.
		/// </summary>
		public string Reason { get; }

		/// <summary>
		/// ISO code of language used for <see cref="Reason"/>.
		/// </summary>
		public string ReasonLanguage { get; }

		/// <summary>
		/// A machine-readable code for the reason for invalidating the photo.
		/// (Each service can define its own reason codes.)
		/// </summary>
		public string ReasonCode { get; }
	}
}
