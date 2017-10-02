using System;
using SkiaSharp;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Cache;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Things.ControlParameters;

namespace Waher.Networking.XMPP.Chat
{
	/// <summary>
	/// Class managing a chat interface for things.
	/// 
	/// The chat interface is defined in:
	/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
	/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
	/// </summary>
	public class ChatServer : IDisposable
	{
		private Cache<string, Variables> sessions = new Cache<string, Variables>(1000, TimeSpan.MaxValue, new TimeSpan(0, 20, 0));
		private SensorServer sensorServer;
		private ControlServer controlServer;
		private XmppClient client;

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		public ChatServer(XmppClient Client, SensorServer SensorServer)
			: this(Client, SensorServer, null)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, ControlServer ControlServer)
			: this(Client, null, ControlServer)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, SensorServer SensorServer, ControlServer ControlServer)
		{
			this.client = Client;
			this.sensorServer = SensorServer;
			this.controlServer = ControlServer;

			this.client.OnChatMessage += new MessageEventHandler(Client_OnChatMessage);

			this.client.RegisterFeature("urn:xmpp:iot:chat");
			this.client.SetPresence(Availability.Chat);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterFeature("urn:xmpp:iot:chat");
		}

		private void SendChatMessage(string To, string Message, bool Markdown)
		{
			if (Markdown)
			{
				MarkdownDocument MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
				string PlainText = MarkdownDocument.GeneratePlainText();

				this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
					"<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">" + XML.Encode(Message) + "</content>",
					PlainText, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
			}
			else
				this.client.SendChatMessage(To, Message);
		}

		private void Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			string s = e.Body;
			if (e.Content != null && e.Content.LocalName == "content" && e.Content.NamespaceURI == "urn:xmpp:content" &&
				XML.Attribute(e.Content, "type") == "text/markdown")
			{
				string s2 = e.Content.InnerText;

				if (!string.IsNullOrEmpty(s2))
					s = s2;
			}

			if (s == null || string.IsNullOrEmpty(s = s.Trim()))
				return;

			switch (s.ToLower())
			{
				case "hi":
				case "hello":
				case "hej":
				case "hallo":
				case "hola":
					this.SendChatMessage(e.From, "Hello. Type # to display the menu.", true);
					break;

				case "#":
					this.ShowMenu(e.From, false);
					break;

				case "##":
					this.ShowMenu(e.From, true);
					break;

				case "?":
					this.InitReadout(e.From);
					this.SendChatMessage(e.From, "Readout started...", true);
					this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, null, DateTime.MinValue, DateTime.MaxValue,
						this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, new object[] { e.From, true, null });
					break;

				case "??":
					this.SendChatMessage(e.From, "Readout started...", true);
					this.InitReadout(e.From);
					this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
						this.AllFieldsRead, this.AllFieldsErrorsRead, new object[] { e.From, true, null });
					break;

				case "=":
					StringBuilder Markdown = new StringBuilder();
					Variables Variables = this.GetVariables(e.From);

					Markdown.AppendLine("|Variable|Value|");
					Markdown.AppendLine("|:-------|:---:|");

					foreach (Variable v in Variables)
					{
						string s2 = v.ValueElement.ToString().Replace("|", "&#124;").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");

						if (s2.Length > 100)
							s2 = s2.Substring(0, 100) + "...";

						Markdown.Append('|');
						Markdown.Append(v.Name);
						Markdown.Append('|');
						Markdown.Append(s2);
						Markdown.AppendLine("|");

						if (Markdown.Length > 3000)
						{
							this.SendChatMessage(e.From, Markdown.ToString(), true);
							Markdown.Clear();
						}
					}

					if (Markdown.Length > 0)
						this.SendChatMessage(e.From, Markdown.ToString(), true);
					break;

				default:
					if (s.EndsWith("??"))
					{
						this.InitReadout(e.From);
						string Field = s.Substring(0, s.Length - 2).Trim();
						this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...", true);
						this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
							this.AllFieldsRead, this.AllFieldsErrorsRead, new object[] { e.From, true, Field });
					}
					else if (s.EndsWith("?"))
					{
						this.InitReadout(e.From);
						string Field = s.Substring(0, s.Length - 1).Trim();
						this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...", true);
						this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, null, DateTime.MinValue, DateTime.MaxValue,
							this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, new object[] { e.From, true, Field });
					}
					else
					{
						int i;

						if (this.controlServer != null && (i = s.IndexOf(":=")) > 0)
						{
							string ParameterName = s.Substring(0, i).Trim();
							string ValueStr = s.Substring(i + 2).Trim();
							ThingReference Ref;

							i = ParameterName.IndexOf('.');
							if (i < 0)
								Ref = null;
							else
							{
								Ref = new ThingReference(ParameterName.Substring(0, i), string.Empty, string.Empty);
								ParameterName = ParameterName.Substring(i + 1).TrimStart();
							}

							try
							{
								ControlParameter[] Parameters = this.controlServer.GetControlParameters(Ref);
								foreach (ControlParameter P in Parameters)
								{
									if (string.Compare(P.Name, ParameterName, true) != 0)
										continue;

									if (!P.SetStringValue(Ref, ValueStr))
										throw new Exception("Unable to set control parameter value.");

									this.SendChatMessage(e.From, "Control parameter set.", false);

									return;
								}
							}
							catch (Exception ex)
							{
								this.Error(e.From, ex.Message);
							}
						}

						this.Execute(s, e.From);
					}
					break;
			}
		}

		private void Execute(string s, string From)
		{
			Expression Exp;

			try
			{
				Exp = new Expression(s);
			}
			catch (Exception)
			{
				this.Que(From);
				return;
			}

			Variables Variables = this.GetVariables(From);
			TextWriter Bak = Variables.ConsoleOut;
			StringBuilder sb = new StringBuilder();
			bool Markdown;

			Variables.Lock();
			Variables.ConsoleOut = new StringWriter(sb);
			try
			{
				IElement Result = Exp.Root.Evaluate(Variables);
				Variables["Ans"] = Result;

				Graph G = Result as Graph;
				SKImage Img;

				if (G != null)
				{
					GraphSettings Settings = new GraphSettings();
					object Obj;
					double d;

					Settings.Width = 600;
					Settings.Height = 300;

					if (Variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
						Settings.Width = (int)Math.Round(d);
					else
					{
						d = (double)Settings.Width;
						if (!Variables.ContainsVariable("GraphWidth"))
							Variables["GraphWidth"] = d;
					}

					Settings.MarginLeft = (int)Math.Round(15 * d / 640);
					Settings.MarginRight = Settings.MarginLeft;

					if (Variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
						Settings.Height = (int)Math.Round(d);
					else
					{
						d = Settings.Height;
						if (!Variables.ContainsVariable("GraphHeight"))
							Variables["GraphHeight"] = d;
					}

					Settings.MarginTop = (int)Math.Round(15 * d / 480);
					Settings.MarginBottom = Settings.MarginTop;
					Settings.LabelFontSize = 12 * d / 480;

					using (SKImage Bmp = G.CreateBitmap(Settings))
					{
						SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
						byte[] Bin = Data.ToArray();

						s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
						s = "![" + Result.ToString() + "](data:image/png;base64," + s + ")";
						Markdown = true;

						Data.Dispose();
					}
				}
				else if ((Img = Result.AssociatedObjectValue as SKImage) != null)
				{
					SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100);
					byte[] Bin = Data.ToArray();

					s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "![" + Result.ToString() + "](data:image/png;base64," + s + ")";
					Markdown = true;

					Data.Dispose();
				}
				else
				{
					s = Result.ToString();
					Markdown = false;
				}

				this.SendChatMessage(From, s, Markdown);
			}
			catch (Exception ex)
			{
				this.Error(From, ex.Message);
				this.Que(From);
			}
			finally
			{
				Variables.ConsoleOut.Flush();
				Variables.ConsoleOut = Bak;
				Variables.Release();
			}
		}

		private void InitReadout(string Address)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields = new Dictionary<string, SortedDictionary<DateTime, Field>>();
			Variables[" Readout "] = Fields;
		}

		private Variables GetVariables(string Address)
		{
			if (!this.sessions.TryGetValue(Address, out Variables Variables))
			{
				Variables = new Variables();
				this.sessions.Add(Address, Variables);
			}

			return Variables;
		}

		private KeyValuePair<string, string>[] UpdateReadoutVariables(string Address, InternalReadoutFieldsEventArgs e, string From, string Field)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields;
			List<KeyValuePair<string, string>> Exp = null;

			if (Variables.TryGetVariable(" Readout ", out Variable v) &&
				(Fields = v.ValueObject as Dictionary<string, SortedDictionary<DateTime, Field>>) != null)
			{
				foreach (Field F in e.Fields)
				{
					if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
						continue;

					if (!Fields.TryGetValue(F.Name, out SortedDictionary<DateTime, Field> Times))
					{
						Times = new SortedDictionary<DateTime, Field>();
						Fields[F.Name] = Times;
					}

					Times[F.Timestamp] = F;
				}

				if (e.Done)
				{
					Variables.Remove(" Readout ");

					SortedDictionary<string, SeriesTypes> Series = null;
					string s;

					foreach (KeyValuePair<string, SortedDictionary<DateTime, Field>> P in Fields)
					{
						if (P.Value.Count == 1)
						{
							foreach (Field F in P.Value.Values)
								Variables[this.PascalCasing(P.Key)] = this.FieldElement(F);
						}
						else
						{
							List<ObjectVector> Values = new List<ObjectVector>();
							IElement E;
							bool Numeric = true;
							SeriesTypes Types;
							SeriesTypes Type;

							foreach (KeyValuePair<DateTime, Field> P2 in P.Value)
							{
								E = this.FieldElement(P2.Value);
								Values.Add(new ObjectVector(new DateTimeValue(P2.Key),
									Expression.Encapsulate(E),
									new ObjectValue(P2.Value.Type), new ObjectValue(P2.Value.QoS)));

								if (!(E is DoubleNumber) && !(E is PhysicalQuantity))
									Numeric = false;
							}

							if (Numeric)
							{
								string Suffix;

								s = P.Key;

								if (EndsWith(ref s, "Average") || EndsWith(ref s, "Avg"))
								{
									Type = SeriesTypes.Average;
									Suffix = "Average";
								}
								else if (EndsWith(ref s, "Minimum") || EndsWith(ref s, "Min"))
								{
									Type = SeriesTypes.Minimum;
									Suffix = "Minimum";
								}
								else if (EndsWith(ref s, "Maximum") || EndsWith(ref s, "Max"))
								{
									Type = SeriesTypes.Maximum;
									Suffix = "Maximum";
								}
								else
								{
									Type = SeriesTypes.Normal;
									Suffix = string.Empty;
								}

								Variables[this.PascalCasing(s) + Suffix] = VectorDefinition.Encapsulate(Values.ToArray(), true, null);

								if (Series == null)
								{
									Series = new SortedDictionary<string, SeriesTypes>();
									Types = Type;
								}
								else if (Series.TryGetValue(s, out Types))
									Types |= Type;
								else
									Types = Type;

								Series[s] = Types;
							}
						}
					}

					if (Series != null)
					{
						StringBuilder Expression = new StringBuilder();
						string VariableName;

						foreach (KeyValuePair<string, SeriesTypes> P in Series)
						{
							VariableName = this.PascalCasing(P.Key);

							if ((P.Value & SeriesTypes.Minimum) != 0)
							{
								Expression.Append("MinTP:=");
								Expression.Append(VariableName);
								Expression.Append("Minimum[0,];");
								Expression.Append("Min:=");
								Expression.Append(VariableName);
								Expression.Append("Minimum[1,];");
							}

							if ((P.Value & SeriesTypes.Maximum) != 0)
							{
								Expression.Append("MaxTP:=");
								Expression.Append(VariableName);
								Expression.Append("Maximum[0,];");
								Expression.Append("Max:=");
								Expression.Append(VariableName);
								Expression.Append("Maximum[1,];");
							}

							if ((P.Value & SeriesTypes.Average) != 0)
							{
								Expression.Append("AvgTP:=");
								Expression.Append(VariableName);
								Expression.Append("Average[0,];");
								Expression.Append("Avg:=");
								Expression.Append(VariableName);
								Expression.Append("Average[1,];");
							}

							if ((P.Value & SeriesTypes.Normal) != 0)
							{
								Expression.Append("TP:=");
								Expression.Append(VariableName);
								Expression.Append("[0,];");
								Expression.Append("V:=");
								Expression.Append(VariableName);
								Expression.Append("[1,];");
							}

							bool First = true;

							if ((P.Value & SeriesTypes.MinMax) == SeriesTypes.MinMax)
							{
								First = false;
								Expression.Append("MinMaxTP:=join(MinTP,reverse(MaxTP));");
								Expression.Append("MinMax:=join(Min,reverse(Max));");
								Expression.Append("polygon2d(MinMaxTP, MinMax, rgba(0, 0, 255, 32))");
							}
							else if ((P.Value & SeriesTypes.Minimum) != 0)
							{
								First = false;
								Expression.Append("plot2dline(MinTP, Min, \"Blue\")");
							}
							else if ((P.Value & SeriesTypes.Maximum) != 0)
							{
								First = false;
								Expression.Append("plot2dline(MaxTP, Max, \"Orange\")");
							}

							if ((P.Value & SeriesTypes.Average) != 0)
							{
								if (First)
									First = false;
								else
									Expression.Append('+');

								Expression.Append("plot2dline(AvgTP, Avg, \"Green\")");
							}

							if ((P.Value & SeriesTypes.Normal) != 0)
							{
								if (First)
									First = false;
								else
									Expression.Append('+');

								Expression.Append("plot2dline(TP, V, \"Red\")");
							}

							if (Exp == null)
								Exp = new List<KeyValuePair<string, string>>();

							s = Expression.ToString();
							Exp.Add(new KeyValuePair<string, string>(P.Key, s));
							Expression.Clear();
						}
					}
				}
			}

			return Exp?.ToArray();
		}

		[Flags]
		private enum SeriesTypes
		{
			Normal = 1,
			Minimum = 2,
			Maximum = 4,
			MinMax = 6,
			Average = 8
		}

		private static bool EndsWith(ref string s, string Suffix)
		{
			if (!s.EndsWith(Suffix, StringComparison.CurrentCultureIgnoreCase))
				return false;

			s = s.Substring(0, s.Length - Suffix.Length).TrimEnd();

			if (s.EndsWith(","))
				s = s.Substring(0, s.Length - 1).TrimEnd();

			return true;
		}

		private IElement FieldElement(Field Field)
		{
			if (Field is QuantityField Q)
			{
				if (string.IsNullOrEmpty(Q.Unit))
					return new DoubleNumber(Q.Value);

				if (Q.Unit == "%")
					return new PhysicalQuantity(Q.Value, new Script.Units.Unit("%"));

				try
				{
					Expression Exp = new Expression(Expression.ToString(Q.Value) + " " + Q.Unit);
					object Result = Exp.Evaluate(null);

					if (Result is PhysicalQuantity Q2)
						return Q2;
					else if (Result is double d)
						return new DoubleNumber(d);
				}
				catch (Exception)
				{
					// Ignore
				}

				return new StringValue(Q.ValueString);
			}

			if (Field is Int32Field I32)
				return new DoubleNumber(I32.Value);

			if (Field is Int64Field I64)
				return new DoubleNumber(I64.Value);

			if (Field is StringField S)
				return new StringValue(S.Value);

			if (Field is BooleanField B)
				return new BooleanValue(B.Value);

			if (Field is DateTimeField DT)
				return new DateTimeValue(DT.Value);

			if (Field is DateField D)
				return new DateTimeValue(D.Value);

			if (Field is DurationField DU)
				return new ObjectValue(DU.Value);

			if (Field is EnumField E)
				return new ObjectValue(E.Value);

			if (Field is TimeField T)
				return new ObjectValue(T.Value);

			return new StringValue(Field.ValueString);
		}

		private string PascalCasing(string Name)
		{
			StringBuilder Result = new StringBuilder();
			bool First = true;

			foreach (char ch in Name)
			{
				if (char.IsLetter(ch))
				{
					if (First)
					{
						First = false;
						Result.Append(char.ToUpper(ch));
					}
					else
						Result.Append(char.ToLower(ch));
				}
				else
					First = true;
			}

			return Result.ToString();
		}

		private void MomentaryFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			StringBuilder sb = new StringBuilder();
			QuantityField QF;

			KeyValuePair<string, string>[] Exp = this.UpdateReadoutVariables(From, e, From, Field);

			foreach (Field F in e.Fields)
			{
				if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
					continue;

				this.CheckMomentaryValuesHeader(P, sb);

				QF = F as QuantityField;

				if (QF != null)
				{
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(F.Name));
					sb.Append('|');
					sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(QF.Unit));
					sb.AppendLine("|");
				}
				else
				{
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(F.Name));
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(F.ValueString));
					sb.AppendLine("||");
				}

				if (sb.Length > 3000)
				{
					this.SendChatMessage(From, sb.ToString(), true);
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				this.SendChatMessage(From, sb.ToString(), true);

			this.SendExpressionResults(Exp, From);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);

			// TODO: Localization
		}

		private void SendExpressionResults(KeyValuePair<string, string>[] Exp, string From)
		{
			if (Exp != null)
			{
				foreach (KeyValuePair<string, string> Expression in Exp)
				{
					this.SendChatMessage(From, "## " + Expression.Key, true);
					this.Execute(Expression.Value, From);
				}
			}
		}

		private void CheckMomentaryValuesHeader(object[] P, StringBuilder sb)
		{
			if ((bool)P[1])
			{
				P[1] = false;
				sb.AppendLine("|Field|Value|Unit|");
				sb.AppendLine("|---|--:|---|");
			}
		}

		private void MomentaryFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			StringBuilder sb = new StringBuilder();

			foreach (ThingError Error in e.Errors)
			{
				this.CheckMomentaryValuesHeader(P, sb);

				sb.Append("|");
				sb.Append(MarkdownDocument.Encode(Error.ToString()));
				sb.AppendLine("|||");

				if (sb.Length > 3000)
				{
					this.SendChatMessage(From, sb.ToString(), true);
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				this.SendChatMessage(From, sb.ToString(), true);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);
		}

		private void AllFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			StringBuilder sb = new StringBuilder();
			QuantityField QF;
			DateTime TP;
			string s;

			KeyValuePair<string, string>[] Exp = this.UpdateReadoutVariables(From, e, From, Field);

			foreach (Field F in e.Fields)
			{
				if ((F.Type & FieldType.Historical) > 0)
					continue;

				if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
					continue;

				this.CheckAllFieldsHeader(P, sb);

				TP = F.Timestamp;

				sb.Append('|');
				sb.Append(s = MarkdownDocument.Encode(F.Name));
				sb.Append('|');
				sb.Append(s);
				sb.Append('|');

				QF = F as QuantityField;

				if (QF != null)
				{
					sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(QF.Unit));
				}
				else
				{
					sb.Append(MarkdownDocument.Encode(F.ValueString));
					sb.Append('|');
				}

				sb.Append('|');
				sb.Append(TP.Year.ToString("D4"));
				sb.Append('-');
				sb.Append(TP.Month.ToString("D2"));
				sb.Append('-');
				sb.Append(TP.Day.ToString("D2"));
				sb.Append(' ');
				sb.Append(TP.Hour.ToString("D2"));
				sb.Append(':');
				sb.Append(TP.Minute.ToString("D2"));
				sb.Append(':');
				sb.Append(TP.Second.ToString("D2"));
				sb.Append('|');
				sb.Append(F.Type.ToString());
				sb.Append('|');
				sb.Append(F.QoS.ToString());
				sb.AppendLine("|");

				if (sb.Length > 3000)
				{
					this.SendChatMessage(From, sb.ToString(), true);
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				this.SendChatMessage(From, sb.ToString(), true);

			this.SendExpressionResults(Exp, From);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);

			// TODO: Localization
		}

		private void CheckAllFieldsHeader(object[] P, StringBuilder sb)
		{
			if ((bool)P[1])
			{
				P[1] = false;
				sb.AppendLine("|Field|Localized|Value|Unit|Timestamp|Type|QoS|");
				sb.AppendLine("|---|---|--:|---|:-:|:-:|:-:|");
			}
		}

		private void AllFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			StringBuilder sb = new StringBuilder();

			foreach (ThingError Error in e.Errors)
			{
				this.CheckAllFieldsHeader(P, sb);

				sb.Append('|');
				sb.Append(MarkdownDocument.Encode(Error.ToString()));
				sb.AppendLine("|||||||");

				if (sb.Length > 3000)
				{
					this.SendChatMessage(From, sb.ToString(), true);
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				this.SendChatMessage(From, sb.ToString(), true);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);
		}

		private void Que(string To)
		{
			this.Error(To, "Sorry. Can't understand what you're trying to say. Type # to display the menu.");
		}

		private void Error(string To, string ErrorMessage)
		{
			this.SendChatMessage(To, "**" + MarkdownDocument.Encode(ErrorMessage) + "**", true);
		}

		private void ShowMenu(string To, bool Extended)
		{
			StringBuilder Output = new StringBuilder();

			Output.AppendLine("|Command | Description");
			Output.AppendLine("|---|---");
			Output.AppendLine("|#|Displays the short version of the menu.");
			Output.AppendLine("|##|Displays the extended version of the menu.");

			if (this.sensorServer != null)
			{
				Output.AppendLine("|?|Reads non-historical values of the currently selected object.");
				Output.AppendLine("|??|Performs a full readout of the currently selected object.");
				Output.AppendLine("|FIELD?|Reads the non-historical fields that begin with \"FIELD\", of the currently selected object.");
				Output.AppendLine("|FIELD??|Reads all values from fields that begin with \"FIELD\", of the currently selected object.");
			}

			if (this.controlServer != null)
				Output.AppendLine("|PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" to the value VALUE.");

			Output.AppendLine("|=|Displays available variables in the session.");
			Output.AppendLine("| |Anything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)");

			this.SendChatMessage(To, Output.ToString(), true);

			if (Extended)
			{
				Output.Clear();

				Output.AppendLine("When reading the device, results will be available as pascal cased variables in the current session. You can use ");
				Output.AppendLine("these to perform calculations. If a single field value is available for a specific field name, the corresponding ");
				Output.AppendLine("variable will contain only the field value. If several values are available for a given field name, the corresponding");
				Output.AppendLine("variable will contain a matrix with their corresponding contents. Use column indexing `Field[Col,]` to access ");
				Output.AppendLine("individual columns.");

				Output.AppendLine();
				Output.AppendLine("Historical values with multiple numerical values will be shown in graph formats.");
				Output.AppendLine("You can control the graph size using the variables `GraphWidth` and `GraphHeight`.");

				this.SendChatMessage(To, Output.ToString(), true);
			}
		}

		// TODO: Support for concentrator.
		// TODO: Control
		// TODO: Configuration
		// TODO: Provisioning
		// TODO: Node Commands.
		// TODO: Browsing data sources.
		// TODO: User authentication
		// TODO: Localization
	}
}
