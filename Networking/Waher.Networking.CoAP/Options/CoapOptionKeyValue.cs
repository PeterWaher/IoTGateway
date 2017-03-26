using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Uri-Query option
	/// 
	/// Defined in RFC 7252 §5.10.1: 
	/// https://tools.ietf.org/html/rfc7252#page-52
	/// </summary>
	public abstract class CoapOptionKeyValue : CoapOptionString
	{
		private string key;
		private string keyValue;

		/// <summary>
		/// Uri-Query option
		/// </summary>
		public CoapOptionKeyValue()
			: base()
		{
		}

		/// <summary>
		/// Uri-Query option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionKeyValue(string Value)
			: base(Value)
		{
			this.Parse();
		}

		/// <summary>
		/// Uri-Query option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionKeyValue(byte[] Value)
			: base(Value)
		{
			this.Parse();
		}

		private void Parse()
		{
			string s = this.Value;
			int i = s.IndexOf('=');

			if (i<0)
			{
				this.key = s;
				this.keyValue = string.Empty;
			}
			else
			{
				this.key = s.Substring(0, i);
				this.keyValue = s.Substring(i + 1);
			}
		}

		/// <summary>
		/// Key name.
		/// </summary>
		public string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// Key value.
		/// </summary>
		public string KeyValue
		{
			get { return this.keyValue; }
		}

	}
}
