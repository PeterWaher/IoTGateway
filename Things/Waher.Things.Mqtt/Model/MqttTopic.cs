using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Networking.MQTT;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Runtime.Inventory;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model
{
	public class MqttTopic
	{
		private readonly SortedDictionary<string, MqttTopic> topics = new SortedDictionary<string, MqttTopic>();
		private readonly MqttTopicNode node;
		private readonly MqttTopic parent;
		private readonly MqttBroker broker;
		private readonly string localTopic;
		private readonly string fullTopic;
		private long dataCount = 0;
		private Exception ex = null;
		private DateTime exTP = DateTime.MinValue;
		private Data data = null;

		public MqttTopic(MqttTopicNode Node, string FullTopic, string LocalTopic, MqttTopic Parent, MqttBroker Broker)
		{
			this.node = Node;
			this.fullTopic = FullTopic;
			this.localTopic = LocalTopic;
			this.parent = Parent;
			this.broker = Broker;
		}

		public MqttTopicNode Node => this.node;
		public string LocalTopic => this.localTopic;
		public string FullTopic => this.fullTopic;

		private MqttTopic[] GetChildNodes()
		{
			if (this.topics == null)
				return new MqttTopic[0];
			else
			{
				MqttTopic[] Result;

				lock (this.topics)
				{
					Result = new MqttTopic[this.topics.Count];
					this.topics.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		internal async Task<MqttTopic> GetTopic(string[] Parts, int Index, bool CreateNew, MqttBroker Broker)
		{
			MqttTopic Topic;

			lock (this.topics)
			{
				if (!this.topics.TryGetValue(Parts[Index], out Topic))
					Topic = null;
			}

			if (Topic == null)
			{
				if (System.Guid.TryParse(Parts[Index].Replace('_', '-'), out Guid _))
					return null;

				if (this.node.HasChildren)
				{
					foreach (INode Child in await this.node.ChildNodes)
					{
						if (Child is MqttTopicNode Topic2 && Topic2.LocalTopic == Parts[Index])
						{
							Topic = new MqttTopic(Topic2, Topic2.FullTopic, Parts[Index], null, Broker);
							break;
						}
					}
				}

				MqttTopicNode Node = null;

				if (Topic == null)
				{
					if (!CreateNew)
						return null;

					StringBuilder sb = new StringBuilder();
					int i;

					for (i = 0; i < Index; i++)
					{
						sb.Append(Parts[i]);
						sb.Append('/');
					}

					sb.Append(Parts[Index]);

					string FullTopic = sb.ToString();

					Node = new MqttTopicNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(FullTopic),
						LocalTopic = Parts[Index]
					};

					Topic = new MqttTopic(Node, FullTopic, Parts[Index], this, Broker);
				}

				lock (this.topics)
				{
					if (this.topics.ContainsKey(Parts[Index]))
						Topic = this.topics[Parts[Index]];
					else
						this.topics[Parts[Index]] = Topic;
				}

				if (Node != null)
				{
					if (Node != Topic.Node)
						await Node.DestroyAsync();
					else
						await this.node.AddAsync(Node);
				}
			}

			Index++;
			if (Parts.Length == Index)
				return Topic;
			else
				return await Topic.GetTopic(Parts, Index, CreateNew, Broker);
		}

		public void DataReported(MqttContent Content)
		{
			int Len = Content.Data.Length;
			if (Len == 0)
			{
				this.data = null;
				return;
			}

			this.dataCount += Len;

			try
			{
				if (this.data == null)
					this.data = this.FindDataType(Content);
				else
					this.data.DataReported(Content);

				this.SetOk();
			}
			catch (Exception)
			{
				try
				{
					this.data = this.FindDataType(Content);
				}
				catch (Exception ex2)
				{
					this.Exception(ex2);
					return;
				}
			}

			if (this.broker.Client?.HasSniffers ?? false)
				this.data.SnifferOutput(this.broker.Client);

			if (Types.TryGetModuleParameter("Sensor", out object Obj) && Obj is SensorServer SensorServer)
			{
				try
				{
					InternalReadoutRequest Request = new InternalReadoutRequest(string.Empty,
						new ThingReference[] { this.node }, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
						(sender, e) =>
						{
							SensorServer.NewMomentaryValues(this.node, e.Fields);

							MqttTopic Current = this;
							MqttTopic Parent = this.parent;

							while (Parent != null)
							{
								foreach (Field F in e.Fields)
								{
									if (F.Name == "Value")
										F.Name = Current.localTopic;
									else
										F.Name = Current.localTopic + ", " + F.Name;

									SensorServer.NewMomentaryValues(Parent.node, F);
								}

								Current = Parent;
								Parent = Parent.parent;
							}
						},
						(sender, e) =>
						{
						}, null);

					this.StartReadout(Request);
				}
				catch (Exception ex)
				{
					this.Exception(ex);
				}
			}
		}

		private Data FindDataType(MqttContent Content)
		{
			try
			{
				string s = Encoding.UTF8.GetString(Content.Data);

				if (long.TryParse(s, out long i))
				{
					this.RemoveWarning();
					return new IntegerData(this, i);
				}

				if (CommonTypes.TryParse(s, out double x, out byte NrDecimals))
				{
					this.RemoveWarning();
					return new FloatingPointData(this, x, NrDecimals);
				}

				if (CommonTypes.TryParse(s, out bool b))
				{
					this.RemoveWarning();
					return new BooleanData(this, b);
				}

				if (System.Guid.TryParse(s, out Guid Guid))
				{
					this.RemoveWarning();
					return new GuidData(this, Guid);
				}

				if (System.TimeSpan.TryParse(s, out TimeSpan TimeSpan))
				{
					this.RemoveWarning();
					return new TimeSpanData(this, TimeSpan);
				}

				if (System.DateTime.TryParse(s, out DateTime DateTime))
				{
					this.RemoveWarning();
					return new DateTimeData(this, DateTime);
				}

				if (CommonTypes.TryParseRfc822(s, out DateTimeOffset DateTimeOffset))
				{
					this.RemoveWarning();
					return new DateTimeOffsetData(this, DateTimeOffset);
				}

				if (Waher.Content.Duration.TryParse(s, out Duration Duration))
				{
					this.RemoveWarning();
					return new DurationData(this, Duration);
				}

				if (s.StartsWith("<") && s.EndsWith(">"))
				{
					try
					{
						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(s);
						this.RemoveWarning();
						return new XmlData(this, s, Doc);
					}
					catch (Exception)
					{
						// Not XML
					}
				}

				if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")))
				{
					try
					{
						object Obj = JSON.Parse(s);
						this.RemoveWarning();
						return new JsonData(this, s, Obj);
					}
					catch (Exception)
					{
						// Not JSON
					}
				}

				if (s.IndexOfAny(controlCharacters) >= 0)
				{
					this.RemoveWarning();
					return new BinaryData(this, Content.Data);
				}

				if (Base64Data.RegEx.IsMatch(s))
				{
					this.RemoveWarning();
					byte[] Data = Convert.FromBase64String(s.Trim());
					if (Data.Length > 0)
						return new Base64Data(this, Data);
				}

				if (HexStringData.RegEx.IsMatch(s))
				{
					this.RemoveWarning();
					byte[] Data = Security.Hashes.StringToBinary(s.Trim());
					if (Data.Length > 0)
						return new HexStringData(this, Data);
				}

				Match M = QuantityData.RegEx.Match(s);
				if (M.Success && CommonTypes.TryParse(M.Groups["Magnitude"].Value, out x, out NrDecimals))
				{
					this.RemoveWarning();
					return new QuantityData(this, x, NrDecimals, M.Groups["Unit"].Value);
				}

				if (!this.hasWarning.HasValue || !this.hasWarning.Value)
				{
					this.node?.LogWarningAsync("Format", "Unrecognized string format: " + s);
					this.hasWarning = true;
				}

				return new StringData(this, s);
			}
			catch (Exception)
			{
				return new BinaryData(this, Content.Data);
			}
		}

		private void RemoveWarning()
		{
			if (!this.hasWarning.HasValue || this.hasWarning.Value)
			{
				this.hasWarning = false;
				this.node?.RemoveWarningAsync("Format");
			}
		}

		private bool? hasWarning = null;

		private static readonly char[] controlCharacters = new char[]
		{
			'\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x0b', '\x0c', '\x0e', '\x0f',
			'\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f'
		};

		private void SetOk()
		{
			this.ex = null;
			this.exTP = DateTime.MinValue;

			this.node.RemoveErrorAsync("Eror");
		}

		private void Exception(Exception ex)
		{
			this.ex = ex;
			this.exTP = DateTime.Now;

			this.node.LogErrorAsync("Error", ex.Message);
		}

		public override string ToString()
		{
			return this.fullTopic;
		}

		public void StartReadout(ISensorReadout Request)
		{
			Task.Run(() => this.StartReadout(this.node, Request, string.Empty, true));
		}

		public async Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			try
			{
				MqttTopic[] ChildNodes = this.GetChildNodes();

				if (ChildNodes != null && ChildNodes.Length > 0)
				{
					foreach (MqttTopic ChildTopic in ChildNodes)
					{
						await ChildTopic.StartReadout(ThingReference, Request,
							string.IsNullOrEmpty(Prefix) ? ChildTopic.LocalTopic : Prefix + ", " + ChildTopic.LocalTopic, false);
					}
				}

				if (this.ex != null)
					Request.ReportErrors(Last, new ThingError(ThingReference, this.exTP, this.ex.Message));
				else if (this.data == null)
				{
					if (Last)
						Request.ReportFields(true);
				}
				else
					this.data.StartReadout(ThingReference, Request, Prefix, Last);

				await this.node.RemoveErrorAsync("Readout");
			}
			catch (Exception ex)
			{
				Request.ReportErrors(Last, new ThingError(ThingReference, DateTime.Now, ex.Message));
				await this.node.LogErrorAsync("Readout", ex.Message);
			}
		}

		public ControlParameter[] GetControlParameters()
		{
			if (this.data == null || !this.data.IsControllable)
				return new ControlParameter[0];
			else
				return this.data.GetControlParameters();
		}

		public async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(LinkedList<Parameter> Parameters,
			Language Language, RequestOrigin _)
		{
			if (this.data != null)
			{
				Parameters.AddLast(new StringParameter("Type", await Language.GetStringAsync(typeof(MqttTopicNode), 25, "Type"),
					await this.data.GetTypeName(Language)));
			}

			if (this.dataCount > 0)
			{
				Parameters.AddLast(new Int64Parameter("Data Count", await Language.GetStringAsync(typeof(MqttTopicNode), 26, "Data Count"),
					this.dataCount));
			}

			return Parameters;
		}

		public bool Remove(string LocalTopic)
		{
			lock (this.topics)
			{
				return this.topics.Remove(LocalTopic);
			}
		}

		public MqttClient MqttClient => this.broker?.Client;

	}
}
