namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Expect HTTP Field header. (RFC 2616, §14.20)
	/// </summary>
	public class HttpFieldExpect : HttpField
	{
		private readonly bool continue100;

		/// <summary>
		/// Expect HTTP Field header. (RFC 2616, §14.20)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldExpect(string Key, string Value)
			: base(Key, Value)
		{
			this.continue100 = string.Compare(Value, "100-continue", true) == 0;
		}

		/// <summary>
		/// If the value is "100-continue".
		/// </summary>
		public bool Continue100 => this.continue100;
	}
}
