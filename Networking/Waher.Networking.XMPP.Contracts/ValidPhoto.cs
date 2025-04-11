namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents a validated photo.
	/// </summary>
	public class ValidPhoto
	{
		/// <summary>
		/// Represents a validated photo.
		/// </summary>
		/// <param name="FileName">File name of validated photo.</param>
		/// <param name="Service">Service validating photo.</param>
		internal ValidPhoto(string FileName, string Service)
		{
			this.FileName = FileName;
			this.Service = Service;
		}

		/// <summary>
		/// File name of validated photo.
		/// </summary>
		public string FileName { get; }

		/// <summary>
		/// Service validating photo.
		/// </summary>
		public string Service { get; }
	}
}
