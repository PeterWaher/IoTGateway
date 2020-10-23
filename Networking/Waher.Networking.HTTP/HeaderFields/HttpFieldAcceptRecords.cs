using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// What parts of the alternative were used when it was accepted.
	/// </summary>
	public enum AcceptanceLevel
	{
		/// <summary>
		/// If the full type is accepted, and type parameters recognized.
		/// </summary>
		TopSubTypeAndParameters = 3,

		/// <summary>
		/// If the full type is accepted.
		/// </summary>
		TopAndSubType = 2,

		/// <summary>
		/// If the type is accepted through a sub-type wildcard (top/*).
		/// </summary>
		TopTypeOnly = 1,

		/// <summary>
		/// If the type is accepted through a top and sub-type wildcard (*/*).
		/// </summary>
		Wildcard = 0
	}

	/// <summary>
	/// Abstract base class for acceptance header fields.
	/// </summary>
	public abstract class HttpFieldAcceptRecords : HttpField
	{
		private AcceptRecord[] records = null;

		/// <summary>
		/// Abstract base class for acceptance header fields.
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptRecords(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Accept records, sorted by acceptability by the client.
		/// </summary>
		public AcceptRecord[] Records
		{
			get
			{
				if (this.records is null)
					this.records = Parse(this.Value);

				return this.records;
			}
		}

		/// <summary>
		/// Gets the best alternative acceptable to the client.
		/// </summary>
		/// <param name="Alternatives">Array of alternatives to choose from.</param>
		/// <returns>The best choice. If none are acceptable, null is returned.</returns>
		public string GetBestAlternative(params string[] Alternatives)
		{
			AcceptanceLevel BestAcceptance = AcceptanceLevel.Wildcard;
			string Best = null;
			double BestQuality = 0;

			foreach (string Alternative in Alternatives)
			{
				if (!this.IsAcceptable(Alternative, out double Quality, out AcceptanceLevel Acceptance, null))
					continue;

				if (Quality > BestQuality || (Quality == BestQuality && Acceptance > BestAcceptance))
				{
					Best = Alternative;
					BestQuality = Quality;
					BestAcceptance = Acceptance;
				}
			}

			return Best;
		}

		/// <summary>
		/// Gets the best alternative acceptable to the client.
		/// </summary>
		/// <param name="Alternatives">Array of alternatives to choose from, together with arrays of any parameters that might 
		/// be relevant in the comparison.</param>
		/// <returns>The best choice. If none are acceptable, (null, null) is returned.</returns>
		public KeyValuePair<string, KeyValuePair<string, string>[]> GetBestAlternative(
			params KeyValuePair<string, KeyValuePair<string, string>[]>[] Alternatives)
		{
			AcceptanceLevel BestAcceptance = AcceptanceLevel.Wildcard;
			KeyValuePair<string, KeyValuePair<string, string>[]> Best = new KeyValuePair<string, KeyValuePair<string, string>[]>(null, null);
			double BestQuality = 0;

			foreach (KeyValuePair<string, KeyValuePair<string, string>[]> Alternative in Alternatives)
			{
				if (!this.IsAcceptable(Alternative.Key, out double Quality, out AcceptanceLevel Acceptance, Alternative.Value))
					continue;

				if (Quality > BestQuality || (Quality == BestQuality && Acceptance > BestAcceptance))
				{
					Best = Alternative;
					BestQuality = Quality;
					BestAcceptance = Acceptance;
				}
			}

			return Best;
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string Alternative)
		{
			return this.IsAcceptable(Alternative, out double _, out AcceptanceLevel _);
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <param name="Quality">Quality level of client support.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string Alternative, out double Quality)
		{
			return this.IsAcceptable(Alternative, out Quality, out AcceptanceLevel _);
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <param name="Quality">Quality level of client support.</param>
		/// <param name="Acceptance">How well the alternative was matched by the acceptance criteria.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string Alternative, out double Quality, out AcceptanceLevel Acceptance)
		{
			int i = Alternative.IndexOf(';');
			if (i < 0)
				return this.IsAcceptable(Alternative, out Quality, out Acceptance, null);
			else
			{
				return this.IsAcceptable(Alternative.Substring(0, i).Trim(), out Quality, out Acceptance,
					CommonTypes.ParseFieldValues(Alternative.Substring(i + 1).Trim()));
			}
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <param name="Quality">Quality level of client support.</param>
		/// <param name="Acceptance">How well the alternative was matched by the acceptance criteria.</param>
		/// <param name="Parameters">Any alternative parameters that might be relevant.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string Alternative, out double Quality, out AcceptanceLevel Acceptance,
			params KeyValuePair<string, string>[] Parameters)
		{
			AcceptanceLevel CurrentAcceptance;
			string s, TopType;
			int i;
			bool? Found;

			i = Alternative.IndexOf('/');
			if (i < 0)
				TopType = Alternative;
			else
				TopType = Alternative.Substring(0, i);

			Quality = 0;
			Acceptance = AcceptanceLevel.Wildcard;

			foreach (AcceptRecord Record in this.Records)
			{
				if (string.Compare(Alternative, s = Record.Item, true) == 0)
				{
					CurrentAcceptance = AcceptanceLevel.TopAndSubType;

					if (Record.Parameters != null && Parameters != null)
					{
						Found = null;

						foreach (KeyValuePair<string, string> P in Record.Parameters)
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
								continue;
						}
					}
				}
				else if (s.EndsWith("/*") && string.Compare(TopType, s.Substring(0, s.Length - 2)) == 0)
					CurrentAcceptance = AcceptanceLevel.TopTypeOnly;
				else if (s == "*/*")
					CurrentAcceptance = AcceptanceLevel.Wildcard;
				else
					continue;

				if (Record.Quality > Quality)
				{
					Quality = Record.Quality;
					Acceptance = CurrentAcceptance;
				}
				else if (Record.Quality == Quality && CurrentAcceptance > Acceptance)
					Acceptance = CurrentAcceptance;
			}

			return Quality > 0;
		}

		/// <summary>
		/// Parses an accept header string.
		/// </summary>
		/// <param name="Value">Accept header string</param>
		/// <returns>Parsed items.</returns>
		public static AcceptRecord[] Parse(string Value)
		{
			List<AcceptRecord> Records = new List<AcceptRecord>();
			List<KeyValuePair<string, string>> Parameters = null;
			StringBuilder sb = new StringBuilder();
			AcceptRecord Record = null;
			string ParameterName = null;
			string ParameterValue = null;
			double q;
			int State = 0;
			int Order = 0;

			foreach (char ch in Value)
			{
				switch (State)
				{
					case 0: // Item name.
						if (ch <= 32)
							break;
						else if (ch == ';' || ch == ',')
						{
							Record = new AcceptRecord()
							{
								Item = sb.ToString().Trim(),
								Order = Order++
							};
							sb.Clear();

							if (ch == ';')
								State++;
							else
							{
								Records.Add(Record);
								Record = null;
							}
						}
						else
							sb.Append(ch);
						break;

					case 1: // Parameter Name
						if (ch == '=')
						{
							ParameterName = sb.ToString().Trim();
							sb.Clear();
							State++;
						}
						else if (ch == ';' || ch == ',')
						{
							ParameterName = sb.ToString().Trim();
							sb.Clear();

							if (Parameters is null)
								Parameters = new List<KeyValuePair<string, string>>();

							Parameters.Add(new KeyValuePair<string, string>(ParameterName, string.Empty));
							ParameterName = null;

							if (ch == ',')
							{
								Record.Parameters = Parameters.ToArray();
								Records.Add(Record);
								Record = null;
								Parameters = null;
								State = 0;
							}
						}
						else
							sb.Append(ch);
						break;

					case 2: // Parameter value.
						if (ch == '"')
							State++;
						else if (ch == '\'')
							State += 3;
						else if (ch == ';' || ch == ',')
						{
							ParameterValue = sb.ToString().Trim();
							sb.Clear();

							if (ParameterName == "q" && CommonTypes.TryParse(ParameterValue, out q))
								Record.Quality = q;
							else
							{
								if (Parameters is null)
									Parameters = new List<KeyValuePair<string, string>>();

								Parameters.Add(new KeyValuePair<string, string>(ParameterName, ParameterValue));
							}

							ParameterName = null;
							ParameterValue = null;

							if (ch == ',')
							{
								if (!(Parameters is null))
									Record.Parameters = Parameters.ToArray();

								Records.Add(Record);
								Record = null;
								Parameters = null;
								State = 0;
							}
							else
								State--;
						}
						else
							sb.Append(ch);
						break;

					case 3: // "Value"
						if (ch == '"')
							State--;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 4: // Escape
						sb.Append(ch);
						State--;
						break;

					case 5: // 'Value'
						if (ch == '\'')
							State -= 3;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 6: // Escape
						sb.Append(ch);
						State--;
						break;
				}
			}

			switch (State)
			{
				case 0:
					Record = new AcceptRecord()
					{
						Item = sb.ToString().Trim(),
						Order = Order++
					};
					if (!string.IsNullOrEmpty(Record.Item))
						Records.Add(Record);
					break;

				case 1:
					ParameterName = sb.ToString().Trim();
					if (!string.IsNullOrEmpty(ParameterName))
					{
						if (Parameters is null)
							Parameters = new List<KeyValuePair<string, string>>();

						Parameters.Add(new KeyValuePair<string, string>(ParameterName, string.Empty));
					}

					if (!(Parameters is null))
						Record.Parameters = Parameters.ToArray();

					Records.Add(Record);
					break;

				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
					ParameterValue = sb.ToString().Trim();

					if (ParameterName == "q" && CommonTypes.TryParse(ParameterValue, out q))
						Record.Quality = q;
					else
					{
						if (Parameters is null)
							Parameters = new List<KeyValuePair<string, string>>();

						Parameters.Add(new KeyValuePair<string, string>(ParameterName, ParameterValue));
					}

					if (!(Parameters is null))
						Record.Parameters = Parameters.ToArray();

					Records.Add(Record);
					break;
			}

			Records.Sort(CompareRecords);

			return Records.ToArray();
		}

		private static int CompareRecords(AcceptRecord Rec1, AcceptRecord Rec2)
		{
			int i = Rec1.Quality.CompareTo(Rec2.Quality);
			if (i != 0)
				return i;

			i = Rec2.Detail - Rec1.Detail;
			if (i != 0)
				return i;

			return Rec1.Order - Rec2.Order;
		}

	}
}
