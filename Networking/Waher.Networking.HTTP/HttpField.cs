namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all HTTP fields.
	/// </summary>
	public class HttpField
	{
		private readonly string key;
		private string value;

		/// <summary>
		/// Base class for all HTTP fields.
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpField(string Key, string Value)
		{
			this.key = Key;
			this.value = Value;
		}

		/// <summary>
		/// HTTP Field Name
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// HTTP Field Value
		/// </summary>
		public string Value
		{
			get => this.value;
			protected set => this.value = value;
		}
	}
}
