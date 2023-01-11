using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Represents a sequence of keywords.
	/// </summary>
	public class SequenceOfKeywords : Keyword
	{
		/// <summary>
		/// Represents a sequence of keywords.
		/// </summary>
		/// <param name="Keywords">Keywords</param>
		public SequenceOfKeywords(params Keyword[] Keywords)
			: base()
		{
			this.Optional = false;
			this.Required = true;
			this.OrderCategory = int.MaxValue;
			this.OrderComplexity = int.MaxValue;
			this.Ignore = true;

			foreach (Keyword Keyword in Keywords)
			{
				if (Keyword.Ignore)
					continue;

				this.Ignore = false;
				this.Optional |= Keyword.Optional;
				this.Required &= Keyword.Required;
				this.OrderCategory = Math.Min(this.OrderCategory, Keyword.OrderCategory);
				this.OrderComplexity = Math.Min(this.OrderComplexity, Keyword.OrderCategory);
			}

			this.Keywords = Keywords;

		}

		/// <summary>
		/// Keyword
		/// </summary>
		public Keyword[] Keywords { get; }

		/// <summary>
		/// If keyword is optional
		/// </summary>
		public override bool Optional { get; }

		/// <summary>
		/// If keyword is required
		/// </summary>
		public override bool Required { get; }

		/// <summary>
		/// Order category of keyword
		/// </summary>
		public override int OrderCategory { get; }

		/// <summary>
		/// Order complexity (within category) of keyword
		/// </summary>
		public override int OrderComplexity { get; }

		/// <summary>
		/// If keyword should be ignored.
		/// </summary>
		public override bool Ignore { get; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			int i, c;

			if (!(obj is SequenceOfKeywords Sequence) ||
				(c = this.Keywords.Length) != Sequence.Keywords.Length)
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!this.Keywords[i].Equals(Sequence.Keywords[i]))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('"');

			foreach (Keyword Keyword in this.Keywords)
			{
				if (Keyword.Ignore)
					continue;

				if (First)
					First = false;
				else
					sb.Append(' ');

				sb.Append(Keyword.ToString());
			}

			sb.Append('"');

			return sb.ToString();
		}

		/// <summary>
		/// Gets available token references.
		/// </summary>
		/// <param name="Process">Current search process.</param>
		/// <returns>Enumerable set of token references.</returns>
		public override async Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(SearchProcess Process)
		{
			Dictionary<ulong, ObjectReference> ExpectedDocIndex = null;
			IEnumerable<KeyValuePair<string, TokenReferences>> Records;
			ObjectReference Ref;
			ulong Key;
			int i, c;

			foreach (Keyword Keyword in this.Keywords)
			{
				if (Keyword.Ignore)
					continue;

				Records = await Keyword.GetTokenReferences(Process);

				if (ExpectedDocIndex is null)
				{
					ExpectedDocIndex = new Dictionary<ulong, ObjectReference>();

					foreach (KeyValuePair<string, TokenReferences> Rec in Records)
					{
						c = Rec.Value.ObjectReferences.Length;
						for (i = 0; i < c; i++)
						{
							Ref = await Process.TryGetObjectReference(Rec.Value.ObjectReferences[i], true);
							if (Ref is null)
								continue;

							if (Ref.TryGetCount(Rec.Key, out TokenCount TokenCount))
							{
								foreach (uint DocIndex in TokenCount.DocIndex)
								{
									Key = Ref.Index;
									Key <<= 32;
									Key |= DocIndex;

									ExpectedDocIndex[Key + 1] = Ref;
								}
							}
						}
					}
				}
				else
				{
					Dictionary<ulong, ObjectReference> ExpectedDocIndex2 = new Dictionary<ulong, ObjectReference>();

					foreach (KeyValuePair<string, TokenReferences> Rec in Records)
					{
						c = Rec.Value.ObjectReferences.Length;
						for (i = 0; i < c; i++)
						{
							Ref = await Process.TryGetObjectReference(Rec.Value.ObjectReferences[i], false);
							if (Ref is null)
								continue;

							if (Ref.TryGetCount(Rec.Key, out TokenCount TokenCount))
							{
								foreach (uint DocIndex in TokenCount.DocIndex)
								{
									Key = Ref.Index;
									Key <<= 32;
									Key |= DocIndex;

									if (ExpectedDocIndex.ContainsKey(Key))
										ExpectedDocIndex2[Key + 1] = Ref;
								}
							}
						}
					}

					ExpectedDocIndex = ExpectedDocIndex2;
				}

				if (ExpectedDocIndex.Count == 0)
					return new KeyValuePair<string, TokenReferences>[0];
			}

			if (ExpectedDocIndex is null)
				return new KeyValuePair<string, TokenReferences>[0];

			c = ExpectedDocIndex.Count;
			ulong[] ObjectReferences = new ulong[c];
			uint[] Counts = new uint[c];
			DateTime[] Timestamps = new DateTime[c];

			i = 0;
			foreach (ObjectReference Ref2 in ExpectedDocIndex.Values)
			{
				ObjectReferences[i] = Ref2.Index;
				Counts[i] = 1;
				Timestamps[i] = Ref2.Indexed;

				i++;
			}

			return new KeyValuePair<string, TokenReferences>[]
			{
				new KeyValuePair<string, TokenReferences>(this.ToString(),
					new TokenReferences()
					{
						LastBlock = 0,
						ObjectReferences = ObjectReferences,
						Timestamps = Timestamps,
						Counts = Counts
					})
			};
		}

		/// <summary>
		/// Processes the keyword in a search process.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns>If the process can continue (true) or if an empty result is concluded (false).</returns>
		public override async Task<bool> Process(SearchProcess Process)
		{
			IEnumerable<KeyValuePair<string, TokenReferences>> Records = await this.GetTokenReferences(Process);

			foreach (KeyValuePair<string, TokenReferences> Rec in Records)
			{
				string Token = Rec.Key;
				TokenReferences References = Rec.Value;

				int j, d = References.ObjectReferences.Length;

				for (j = 0; j < d; j++)
				{
					ulong ObjectReference = References.ObjectReferences[j];

					if (Process.IsRestricted)
						Process.Found[ObjectReference] = true;

					if (!Process.ReferencesByObject.TryGetValue(ObjectReference, out LinkedList<TokenReference> ByObjectReference))
					{
						if (Process.IsRestricted)
							continue;

						ByObjectReference = new LinkedList<TokenReference>();
						Process.ReferencesByObject[ObjectReference] = ByObjectReference;
					}

					ByObjectReference.AddLast(new TokenReference()
					{
						Count = References.Counts[j],
						LastBlock = References.LastBlock,
						ObjectReference = ObjectReference,
						Timestamp = References.Timestamps[j],
						Token = Token
					});
				}
			}

			if (Process.IsRestricted)
			{
				LinkedList<ulong> ToRemove = null;

				foreach (ulong ObjectReference in Process.ReferencesByObject.Keys)
				{
					if (!Process.Found.ContainsKey(ObjectReference))
					{
						if (ToRemove is null)
							ToRemove = new LinkedList<ulong>();

						ToRemove.AddLast(ObjectReference);
					}
				}

				if (!(ToRemove is null))
				{
					foreach (ulong ObjectReference in ToRemove)
						Process.ReferencesByObject.Remove(ObjectReference);
				}
			}

			if (Process.ReferencesByObject.Count == 0)
				return false;

			Process.IncRestricted();

			return true;
		}

	}
}
