using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script;

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

		public string[] FromContentTypes
		{
			get { return new string[] { this.from }; }
		}

		public string[] ToContentTypes
		{
			get { return new string[] { this.to }; }
		}

		public Grade ConversionGrade
		{
			get { return this.conversionGrade; }
		}

		public bool Convert(string FromContentType, Stream From, string FromFileName, string LocalResourceName, string URL, 
			ref string ToContentType, Stream To, Variables Session, params string[] PossibleContentTypes)
		{
			Stream Intermediate = null;
			Stream Intermediate2 = null;
			bool UseMemoryStreams = To is MemoryStream;
			string FromType;
			string ToType = FromContentType;
			int i, c = this.sequence.Length;
			bool Dynamic = false;

			try
			{
				for (i = 0; i < c; i++)
				{
					FromType = ToType;

					if (i == c - 1)
					{
						if (this.sequence[i].Value.Convert(FromType, Intermediate, FromFileName, LocalResourceName, URL, ref ToContentType,
							To, Session, PossibleContentTypes))
						{
							Dynamic = true;
						}
					}
					else
					{
						ToType = this.sequence[i + 1].Key;

						if (UseMemoryStreams)
							Intermediate2 = new MemoryStream();
						else
							Intermediate2 = new TemporaryFile();

						if (this.sequence[i].Value.Convert(FromType, Intermediate ?? From, FromFileName, LocalResourceName,
							URL, ref ToType, Intermediate2, Session))
						{
							Dynamic = true;
						}

						FromFileName = string.Empty;
						LocalResourceName = string.Empty;

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
