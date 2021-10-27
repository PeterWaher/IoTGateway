using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Corresponds to an item in the Accept, Accept-Charset, Accept-Encoding and Accept-Language header fields.
	/// </summary>
	public class AcceptRecord
	{
		private string item = null;
		private double q = 1;
		private KeyValuePair<string, string>[] parameters = null;
		private int order = 0;

		/// <summary>
		/// Corresponds to an item in the Accept, Accept-Charset, Accept-Encoding and Accept-Language header fields.
		/// </summary>
		internal AcceptRecord()
		{
		}

		/// <summary>
		/// Item name.
		/// </summary>
		public string Item
		{
			get { return this.item; }
			internal set { this.item = value; }
		}

		/// <summary>
		/// Quality property, between 0 and 1.
		/// </summary>
		public double Quality
		{
			get { return this.q; }
			internal set { this.q = value; }
		}

		/// <summary>
		/// Any additional parameters available. If no parameters are available, null is returned.
		/// </summary>
		public KeyValuePair<string, string>[] Parameters
		{
			get { return this.parameters; }
			internal set { this.parameters = value; }
		}

		/// <summary>
		/// Order of record, as it appears in the header.
		/// </summary>
		public int Order
		{
			get { return this.order; }
			internal set { this.order = value; }
		}

		internal int Detail
		{
			get
			{
				if (!(this.parameters is null))
					return 3;

				if (!this.item.EndsWith("/*"))
					return 2;

				if (this.item == "*/*")
					return 0;
				else
					return 1;
			}
		}

		/// <summary>
		/// Checks if a content type is acceptable to the client sending a request.
		/// </summary>
		/// <param name="ContentType">Content Type to check.</param>
		/// <param name="Quality">Quality level of client support.</param>
		/// <param name="Acceptance">How well the content type was matched by the acceptance criteria.</param>
		/// <param name="Parameters">Any content type parameters that might be relevant.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string ContentType, out double Quality, out AcceptanceLevel Acceptance, params KeyValuePair<string, string>[] Parameters)
		{
			AcceptanceLevel CurrentAcceptance;

			Quality = 0;
			Acceptance = AcceptanceLevel.Wildcard;

			if (string.Compare(ContentType, this.item, true) == 0)
			{
				bool? Found;

				CurrentAcceptance = AcceptanceLevel.TopAndSubType;

				if (!(this.parameters is null) && !(Parameters is null))
				{
					Found = null;

					foreach (KeyValuePair<string, string> P in this.parameters)
					{
						foreach (KeyValuePair<string, string> P2 in Parameters)
						{
							if (string.Compare(P.Key, P2.Key, true) == 0)
							{
								if (string.Compare(P.Value, P2.Value, true) == 0)
									Found = true;
								else
									Found = false;

								break;
							}
						}

						if (Found.HasValue)
							break;
					}

					if (Found.HasValue)
					{
						if (Found.Value)
							CurrentAcceptance = AcceptanceLevel.TopSubTypeAndParameters;
						else
							return false;
					}
				}
			}
			else
			{
				string TopType;
				int i = ContentType.IndexOf('/');

				if (i < 0)
					TopType = ContentType;
				else
					TopType = ContentType.Substring(0, i);

				if (this.item.EndsWith("/*") && string.Compare(TopType, this.item.Substring(0, this.item.Length - 2)) == 0)
					CurrentAcceptance = AcceptanceLevel.TopTypeOnly;
				else if (this.item == "*/*")
					CurrentAcceptance = AcceptanceLevel.Wildcard;
				else
					return false;
			}

			if (this.q > Quality)
			{
				Quality = this.q;
				Acceptance = CurrentAcceptance;
			}
			else if (this.q == Quality && CurrentAcceptance > Acceptance)
				Acceptance = CurrentAcceptance;

			return Quality > 0;
		}

	}
}
