using System.IO;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using System.Threading.Tasks;

namespace Waher.Content
{
	/// <summary>
	/// Internal class that performs content conversion by joining a sequence of conversions into one larger.
	/// </summary>
	internal class ConversionSequence : IContentConverter
	{
		private readonly KeyValuePair<string, IContentConverter>[] sequence;
		private readonly Grade conversionGrade;
		private readonly string from;
		private readonly string to;

		public ConversionSequence(string From, string To, Grade ConversionGrade, params KeyValuePair<string, IContentConverter>[] Sequence)
		{
			this.sequence = Sequence;
			this.from = From;
			this.to = To;
			this.conversionGrade = ConversionGrade;
		}

		public string[] FromContentTypes => new string[] { this.from };
		public string[] ToContentTypes => new string[] { this.to };
		public Grade ConversionGrade => this.conversionGrade;

		public async Task<bool> ConvertAsync(ConversionState State)
		{
			Stream Intermediate = null;
			Stream Intermediate2 = null;
			bool UseMemoryStreams = State.To is MemoryStream;
			string FromType;
			string ToType = State.FromContentType;
			int i, c = this.sequence.Length;
			bool Dynamic = false;

			try
			{
				for (i = 0; i < c; i++)
				{
					FromType = ToType;

					if (i == c - 1)
					{
						ConversionState State2 = new ConversionState(FromType, Intermediate ?? State.From, State.FromFileName, 
							State.LocalResourceName, State.URL, State.ToContentType, State.To, State.Session, State.Progress, 
							State.ResourceMap, State.TryGetLocalResourceFileName, State.PossibleContentTypes);

						if (await this.sequence[i].Value.ConvertAsync(State2))
							Dynamic = true;

						if (State2.HasError)
						{
							State.Error = State2.Error;
							return Dynamic;
						}

						State.ToContentType = State2.ToContentType;
					}
					else
					{
						ToType = this.sequence[i + 1].Key;

						if (UseMemoryStreams)
							Intermediate2 = new MemoryStream();
						else
							Intermediate2 = new TemporaryStream();

						ConversionState State2 = new ConversionState(FromType, Intermediate ?? State.From, State.FromFileName,
							State.LocalResourceName, State.URL, ToType, Intermediate2, State.Session, State.Progress,
							State.ResourceMap, State.TryGetLocalResourceFileName);

						if (await this.sequence[i].Value.ConvertAsync(State2))
							Dynamic = true;

						if (State2.HasError)
						{
							State.Error = State2.Error;
							return Dynamic;
						}

						ToType = State2.ToContentType;

						State.FromFileName = string.Empty;
						State.LocalResourceName = string.Empty;

						Intermediate?.Dispose();
						
						Intermediate = Intermediate2;
						Intermediate.Position = 0;

						Intermediate2 = null;
					}
				}
			}
			finally
			{
				Intermediate?.Dispose();
				Intermediate2?.Dispose();
			}

			return Dynamic;
		}

	}
}
