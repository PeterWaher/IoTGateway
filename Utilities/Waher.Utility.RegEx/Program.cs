using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Content;

namespace Waher.Utility.RegEx
{
	class Program
	{
		/// <summary>
		/// Searches through one or multiple files using a regular expression.
		/// Resulting matches can be printed or exported. They can also optionally
		/// be replaced.
		/// 
		/// Command line switches:
		/// 
		/// -p PATH               Path to start the search. If not provided, the
		///                       current path will be used, with *.* as search
		///                       pattern. Can be used multiple times to search
		///                       through multiple paths, or use multiple search
		///                       patterns.
		/// -f REGEX              Regular expression to use for finding matches.
		/// -r REPLACE            If used, will be used to replace found matches.
		/// -x FILENAME           Export findings to an XML file.
		/// -enc ENCODING         Text encoding if Byte-order-marks not available.
		///                       Default=UTF-8
		/// -s                    Include subfolders in search.
		/// -o                    Print findings on the standard output.
		/// -ci                   Culture invariant search.
		/// -ecma                 Use ECMA script.
		/// -ic                   Ignore case.
		/// -iw                   Ignore pattern whitespace.
		/// -m                    Multi-line matching
		/// -n                    Single-line matching
		/// -t                    Test Mode. Files are not updated.
		/// -?                    Help.
		/// </summary>
		static int Main(string[] args)
		{
			FileStream FileOutput = null;
			XmlWriter Output = null;
			Encoding Encoding = Encoding.Default;
			RegexOptions Options = RegexOptions.Compiled;
			List<string> Paths = new List<string>();
			string Expression = null;
			string ReplaceExpression = null;
			string XmlFileName = null;
			bool Subfolders = false;
			bool Help = false;
			bool Print = false;
			bool Test = false;
			int i = 0;
			int c = args.Length;
			string s;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-p":
							if (i >= c)
								throw new Exception("Missing path.");

							Paths.Add(args[i++]);
							break;

						case "-f":
							if (i >= c)
								throw new Exception("Missing regular expression.");

							if (string.IsNullOrEmpty(Expression))
								Expression = args[i++];
							else
								throw new Exception("Only one regular expression allowed.");
							break;

						case "-r":
							if (i >= c)
								throw new Exception("Missing replace expression.");

							if (string.IsNullOrEmpty(ReplaceExpression))
								ReplaceExpression = args[i++];
							else
								throw new Exception("Only one replace expression allowed.");
							break;

						case "-x":
							if (i >= c)
								throw new Exception("Missing export filename.");

							if (string.IsNullOrEmpty(XmlFileName))
								XmlFileName = args[i++];
							else
								throw new Exception("Only one export file allowed.");
							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
							break;

						case "-s":
							Subfolders = true;
							break;

						case "-o":
							Print = true;
							break;

						case "-t":
							Test = true;
							break;

						case "-?":
							Help = true;
							break;

						case "-ci":
							Options |= RegexOptions.CultureInvariant;
							break;

						case "-ecma":
							Options |= RegexOptions.ECMAScript;
							break;

						case "-ic":
							Options |= RegexOptions.IgnoreCase;
							break;

						case "-iw":
							Options |= RegexOptions.IgnorePatternWhitespace;
							break;

						case "-m":
							if ((Options & RegexOptions.Singleline) != 0)
								throw new Exception("Contradictive options.");

							Options |= RegexOptions.Multiline;
							break;

						case "-n":
							if ((Options & RegexOptions.Multiline) != 0)
								throw new Exception("Contradictive options.");

							Options |= RegexOptions.Singleline;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					Console.Out.WriteLine("Searches through one or multiple files using a regular expression.");
					Console.Out.WriteLine("Resulting matches can be printed or exported. They can also optionally");
					Console.Out.WriteLine("be replaced.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-p PATH               Path to start the search. If not provided, the");
					Console.Out.WriteLine("                      current path will be used, with *.* as search");
					Console.Out.WriteLine("                      pattern. Can be used multiple times to search");
					Console.Out.WriteLine("                      through multiple paths, or use multiple search");
					Console.Out.WriteLine("                      patterns.");
					Console.Out.WriteLine("-f REGEX              Regular expression to use for finding matches.");
					Console.Out.WriteLine("-r REPLACE            If used, will be used to replace found matches.");
					Console.Out.WriteLine("-x FILENAME           Export findings to an XML file.");
					Console.Out.WriteLine("-enc ENCODING         Text encoding if Byte-order-marks not available.");
					Console.Out.WriteLine("                      Default=UTF-8");
					Console.Out.WriteLine("-s                    Include subfolders in search.");
					Console.Out.WriteLine("-o                    Print findings on the standard output.");
					Console.Out.WriteLine("-ci                   Culture invariant search.");
					Console.Out.WriteLine("-ecma                 Use ECMA script.");
					Console.Out.WriteLine("-ic                   Ignore case.");
					Console.Out.WriteLine("-iw                   Ignore pattern whitespace.");
					Console.Out.WriteLine("-m                    Multi-line matching");
					Console.Out.WriteLine("-n                    Single-line matching");
					Console.Out.WriteLine("-t                    Test Mode. Files are not updated.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(Expression))
					throw new Exception("No regular expression specified.");

				Regex Regex = new Regex(Expression, Options);
				string SearchPattern;

				if (Paths.Count == 0)
					Paths.Add(Directory.GetCurrentDirectory());

				if (!string.IsNullOrEmpty(XmlFileName))
				{
					XmlWriterSettings Settings = new XmlWriterSettings()
					{
						CloseOutput = true,
						ConformanceLevel = ConformanceLevel.Document,
						Encoding = Encoding.UTF8,
						Indent = true,
						IndentChars = "\t",
						NewLineChars = "\r\n",
						NewLineHandling = NewLineHandling.Entitize,
						NewLineOnAttributes = false,
						OmitXmlDeclaration = false,
						WriteEndDocumentOnClose = true
					};

					FileOutput = File.Create(XmlFileName);
					Output = XmlWriter.Create(FileOutput, Settings);
				}

				Output?.WriteStartDocument();
				Output?.WriteStartElement("Search", "http://waher.se/schema/RegExMatches.xsd");
				Output?.WriteStartElement("Files");

				Dictionary<string, bool> FileProcessed = new Dictionary<string, bool>();
				Dictionary<string, bool> FileMatches = new Dictionary<string, bool>();
				Dictionary<string, bool> FileUpdates = new Dictionary<string, bool>();
				SortedDictionary<string, SortedDictionary<string, int>> GroupCount = new SortedDictionary<string, SortedDictionary<string, int>>();

				foreach (string Path0 in Paths)
				{
					string Path = Path0;

					if (Directory.Exists(Path))
						SearchPattern = "*.*";
					else
					{
						SearchPattern = System.IO.Path.GetFileName(Path);
						Path = System.IO.Path.GetDirectoryName(Path);

						if (!Directory.Exists(Path))
							throw new Exception("Path does not exist.");
					}

					string[] FileNames = Directory.GetFiles(Path, SearchPattern, Subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

					foreach (string FileName in FileNames)
					{
						if (FileProcessed.ContainsKey(FileName))
							continue;

						FileProcessed[FileName] = true;

						byte[] Data = File.ReadAllBytes(FileName);
						string Text = CommonTypes.GetString(Data, Encoding);
						string Text2 = Text;
						int Offset = 0;

						MatchCollection Matches = Regex.Matches(Text);
						c = Matches.Count;

						if (c > 0)
						{
							FileMatches[FileName] = true;

							Output?.WriteStartElement("File");
							Output?.WriteAttributeString("name", FileName);
							Output?.WriteAttributeString("count", c.ToString());

							if (Print)
							{
								Console.Out.WriteLine("File: " + FileName);
								Console.Out.WriteLine(new string('-', Math.Max(20, FileName.Length + 10)));
								Console.Out.WriteLine("Matches: " + c.ToString());
							}

							foreach (Match M in Matches)
							{
								Output?.WriteStartElement("Match");
								Output?.WriteAttributeString("index", M.Index.ToString());
								Output?.WriteAttributeString("length", M.Length.ToString());
								Output?.WriteAttributeString("value", M.Value);

								if (Print)
									Console.Out.WriteLine("Pos " + M.Index.ToString() + ": " + M.Value);

								if (!string.IsNullOrEmpty(ReplaceExpression))
								{
									string ReplaceWith = M.Result(ReplaceExpression);

									i = M.Index + Offset;
									Text2 = Text2.Remove(i, M.Length).Insert(i, ReplaceWith);
									Offset += ReplaceWith.Length - M.Length;

									Output?.WriteAttributeString("replacedWith", ReplaceWith);

									if (Print)
										Console.Out.WriteLine("Replaced with: " + ReplaceWith);
								}

								foreach (Group G in M.Groups)
								{
									if (int.TryParse(G.Name, out i))
										continue;

									Output?.WriteStartElement("Group");
									Output?.WriteAttributeString("name", G.Name);
									Output?.WriteAttributeString("count", G.Value);
									Output?.WriteEndElement();

									if (Print)
										Console.Out.WriteLine(G.Name + ": " + G.Value);

									if (!GroupCount.TryGetValue(G.Name, out SortedDictionary<string, int> Counts))
									{
										Counts = new SortedDictionary<string, int>();
										GroupCount[G.Name] = Counts;
									}

									if (Counts.TryGetValue(G.Value, out int Count))
										Counts[G.Value] = Count + 1;
									else
										Counts[G.Value] = 1;
								}

								Output?.WriteEndElement();
							}

							Output?.WriteEndElement();

							if (Print)
								Console.Out.WriteLine();

							if (!string.IsNullOrEmpty(ReplaceExpression) && Text2 != Text)
							{
								FileUpdates[FileName] = true;

								if (!Test)
									File.WriteAllText(FileName, Text2, Encoding);
							}
						}
					}
				}

				Output?.WriteEndElement();

				Output?.WriteStartElement("Statistics");
				Output?.WriteAttributeString("fileCount", FileProcessed.Count.ToString());
				Output?.WriteAttributeString("fileMatchCount", FileMatches.Count.ToString());
				Output?.WriteAttributeString("fileUpdateCount", FileUpdates.Count.ToString());

				if (Print)
				{
					Console.Out.WriteLine("Files processed: " + FileProcessed.Count.ToString());
					Console.Out.WriteLine("Files matched: " + FileMatches.Count.ToString());
				}

				foreach (KeyValuePair<string, SortedDictionary<string, int>> GroupStat in GroupCount)
				{
					Output?.WriteStartElement("Group");
					Output?.WriteAttributeString("name", GroupStat.Key);

					if (Print)
						Console.Out.WriteLine(GroupStat.Key + ":");

					foreach (KeyValuePair<string, int> Rec in GroupStat.Value)
					{
						Output?.WriteStartElement("Group");
						Output?.WriteAttributeString("value", Rec.Key);
						Output?.WriteAttributeString("count", Rec.Value.ToString());
						Output?.WriteEndElement();

						if (Print)
							Console.Out.WriteLine("\t" + Rec.Key + ": " + Rec.Value.ToString());
					}

					Output?.WriteEndElement();
				}

				Output?.WriteEndElement();
				Output?.WriteEndDocument();

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				Output?.Flush();
				Output?.Close();
				Output?.Dispose();
				FileOutput?.Dispose();
			}
		}
	}
}
