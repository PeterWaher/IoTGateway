namespace Waher.Content.Zip
{
	/// <summary>
	/// Encapsulates a ZIP File
	/// </summary>
	public class ZipFile
	{
		private readonly byte[] binary;

		/// <summary>
		/// Encapsulates a ZIP File
		/// </summary>
		/// <param name="Binary">Binary representation of ZIP file.</param>
		public ZipFile(byte[] Binary)
		{
			this.binary = Binary;
		}

		/// <summary>
		/// Binary representation of ZIP file.
		/// </summary>
		public byte[] Binary => this.binary;
	}
}
