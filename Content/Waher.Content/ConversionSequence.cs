using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content
{
	/// <summary>
	/// Internal class that performs content conversion by joining a sequence of conversions into one larger.
	/// </summary>
	internal class ConversionSequence : IContentConverter
	{
		private KeyValuePair<string, IContentConverter>[] sequence;
		private Grade conversionGrade;
		private string from;
		private string to;

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

		public void Convert(string FromContentType, Stream From, string FromFileName, string LocalResourceName, string URL, string ToContentType, 
            Stream To, Variables Session)
		{
			Stream Intermediate = null;
			Stream Intermediate2 = null;
			bool UseMemoryStreams = To is MemoryStream;
			string FromType;
			string ToType = FromContentType;
			int i, c = this.sequence.Length;

			try
			{
				for (i = 0; i < c; i++)
				{
					FromType = ToType;

					if (i == c - 1)
					{
						this.sequence[i].Value.Convert(FromType, Intermediate, FromFileName, LocalResourceName, URL,
							  ToContentType, To, Session);
					}
					else
					{
						ToType = this.sequence[i + 1].Key;

						if (UseMemoryStreams)
							Intermediate2 = new MemoryStream();
						else
							Intermediate2 = new TemporaryFile();

						this.sequence[i].Value.Convert(FromType, Intermediate == null ? From : Intermediate, FromFileName, LocalResourceName,
							URL, ToContentType, Intermediate2, Session);

						FromFileName = string.Empty;
						LocalResourceName = string.Empty;

						if (Intermediate != null)
							Intermediate.Dispose();

						Intermediate = Intermediate2;
						Intermediate.Position = 0;

						Intermediate2 = null;
					}
				}
			}
			finally
			{
				if (Intermediate != null)
					Intermediate.Dispose();

				if (Intermediate2 != null)
					Intermediate2.Dispose();
			}
		}

	}
}
