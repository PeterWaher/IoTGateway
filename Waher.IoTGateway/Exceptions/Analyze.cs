using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Waher.Events;

namespace Waher.IoTGateway.Exceptions
{
	/// <summary>
	/// Analyzes exceptions and extracts basic statistics.
	/// </summary>
	public static class Analyze
	{
		private const int BufSize = 16 * 65536;


		/// <summary>
		/// Analyzes exceptions and extracts basic statistics.
		/// </summary>
		/// <param name="ExceptionFileName">File name of exception file.</param>
		/// <param name="OutputFileName">XML Output file.</param>
		public static void Process(string ExceptionFileName, string OutputFileName)
		{
			string s;
			FileStream FileOutput = null;
			XmlWriter Output = null;
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\n",
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true,
				CheckCharacters = false
			};

			Statistics Statistics = new Statistics();
			byte[] Buffer = new byte[BufSize];
			int NrRead = 0;
			int Last = 0;
			int i, j;
			bool SkipHyphens;

			try
			{
				using (FileStream fs = File.OpenRead(ExceptionFileName))
				{
					FileOutput = File.Create(OutputFileName);
					Output = XmlWriter.Create(FileOutput, Settings);

					Output.WriteStartDocument();
					Output.WriteStartElement("Statistics", "http://waher.se/schema/ExStat.xsd");

					do
					{
						SkipHyphens = false;
						i = 0;

						if (Last > 0)
						{
							if (Last < BufSize)
								Array.Copy(Buffer, Last, Buffer, 0, (i = BufSize - Last));
							else
								SkipHyphens = true;

							Last = 0;
						}

						NrRead = fs.Read(Buffer, i, BufSize - i);
						if (NrRead <= 0)
							break;

						if (SkipHyphens)
						{
							while (i < BufSize && NrRead > 0 && Buffer[i] == '-')
								i++;

							Last = i;
						}
						else
						{
							NrRead += i;
							i = 0;
						}

						j = NrRead - 4;
						while (i < j)
						{
							if (Buffer[i] == '-' && Buffer[i + 1] == '-' && Buffer[i + 2] == '-' &&
								Buffer[i + 3] == '-' && Buffer[i + 4] == '-')
							{
								s = Encoding.Default.GetString(Buffer, Last, i - Last);
								Process(s, Statistics);

								i += 5;
								while (i < NrRead && Buffer[i] == '-')
									i++;

								Last = i;
							}
							else
								i++;
						}
					}
					while (NrRead == BufSize);

					if (Last < NrRead)
					{
						s = Encoding.Default.GetString(Buffer, Last, NrRead - Last);
						Process(s, Statistics);
					}
				}

				Export(Output, "PerType", "type", Statistics.PerType, "PerMessage", "PerSource");
				Export(Output, "PerMessage", "message", Statistics.PerMessage, "PerType", "PerSource");
				Export(Output, "PerSource", string.Empty, Statistics.PerSource, "PerType", "PerMessage");
				Export(Output, "PerHour", "hour", "yyyy-MM-ddTHH", Statistics.PerHour);
				Export(Output, "PerDay", "day", "yyyy-MM-dd", Statistics.PerDay);
				Export(Output, "PerMonth", "month", "yyyy-MM", Statistics.PerMonth);

				Output.WriteEndElement();
				Output.WriteEndDocument();
			}
			catch (Exception ex)
			{
				Log.Critical(ex, ExceptionFileName);
			}
			finally
			{
				Output?.Flush();
				Output?.Close();
				Output?.Dispose();
				FileOutput?.Dispose();
			}
		}

		private static void Export(XmlWriter Output, string ElementName, string AttributeName, Histogram<string> Histogram, params string[] SubElementNames)
		{
			KeyValuePair<string, Bucket>[] A = new KeyValuePair<string, Bucket>[Histogram.Buckets.Count];
			Histogram.Buckets.CopyTo(A, 0);
			Array.Sort<KeyValuePair<string, Bucket>>(A, (r1, r2) =>
			{
				long i = r2.Value.Count - r1.Value.Count;
				if (i != 0)
				{
					if (i > int.MaxValue)
						i = int.MaxValue;
					else if (i < int.MinValue)
						i = int.MinValue;

					return (int)i;
				}

				return string.Compare(r1.Key, r2.Key);
			});

			Output.WriteStartElement(ElementName);

			string s;
			bool AsValue = string.IsNullOrEmpty(AttributeName);

			foreach (KeyValuePair<string, Bucket> Rec in A)
			{
				Output.WriteStartElement("Stat");
				Output.WriteAttributeString("count", Rec.Value.Count.ToString());

				s = Rec.Key.Replace("\r\n", "\n").Replace('\r', '\n');
				if (AsValue)
					Output.WriteElementString("Value", s);
				else
					Output.WriteAttributeString(AttributeName, s);

				if (!(Rec.Value.SubHistograms is null))
					Export(Output, Rec.Value.SubHistograms, SubElementNames);

				Output.WriteEndElement();
			}

			Output.WriteEndElement();
		}

		private static void Export(XmlWriter Output, Histogram<string>[] SubHistograms, params string[] ElementNames)
		{
			int i, c = SubHistograms.Length, d = ElementNames.Length;

			for (i = 0; i < c; i++)
				Export(Output, i < d ? ElementNames[i] : "Details", string.Empty, SubHistograms[i]);
		}

		private static void Export(XmlWriter Output, string ElementName, string AttributeName, string DateTimeFormat,
			Histogram<DateTime> Histogram, params string[] SubElementNames)
		{
			Output.WriteStartElement(ElementName);

			foreach (KeyValuePair<DateTime, Bucket> Rec in Histogram.Buckets)
			{
				Output.WriteStartElement("Stat");
				Output.WriteAttributeString(AttributeName, Rec.Key.ToString(DateTimeFormat));
				Output.WriteAttributeString("count", Rec.Value.Count.ToString());

				if (!(Rec.Value.SubHistograms is null))
					Export(Output, Rec.Value.SubHistograms, SubElementNames);

				Output.WriteEndElement();
			}

			Output.WriteEndElement();
		}

		private static void Process(string s, Statistics Statistics)
		{
			int i = s.IndexOf("Type:");
			if (i < 0)
				return;

			int j = s.IndexOf('\n', i);
			if (j < 0)
				return;

			string Type = s.Substring(i + 5, j - i - 5).Trim();

			i = s.IndexOf("Time:", j);
			if (i < 0)
				return;

			j = s.IndexOf('\n', i);
			if (j < 0)
				return;

			string TimeStr = s.Substring(i + 5, j - i - 5).Trim();
			if (!DateTime.TryParse(TimeStr, out DateTime Time))
				return;

			DateTime Day = Time.Date;
			DateTime Month = new DateTime(Day.Year, Day.Month, 1);
			DateTime Hour = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour, 0, 0);

			i = j;
			j = s.IndexOf("   at", i);
			if (j < 0)
				return;

			string Message = s.Substring(i, j - i).Trim();
			string StackTrace = s.Substring(j).TrimEnd();

			Statistics.PerType.Inc(Type, Message, StackTrace);
			Statistics.PerMessage.Inc(Message, Type, StackTrace);
			Statistics.PerSource.Inc(StackTrace, Type, Message);
			Statistics.PerHour.Inc(Hour);
			Statistics.PerDay.Inc(Day);
			Statistics.PerMonth.Inc(Month);
		}

	}
}
