using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Runtime.Geo;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Geo-spatial contractual parameter
	/// </summary>
	public class GeoParameter : Parameter
	{
		private AltitudeUse altitude = AltitudeUse.Optional;
		private GeoPosition value;
		private GeoPosition min = null;
		private GeoPosition max = null;
		private bool contractLocation = false;
		private bool minIncluded = true;
		private bool maxIncluded = true;

		/// <summary>
		/// Parameter value
		/// </summary>
		public GeoPosition Value
		{
			get => this.@value;
			set
			{
				this.@value = value;
				this.ProtectedValue = null;
			}
		}

		/// <summary>
		/// If the parameter represents the location of the actual contract, or contents of the contract.
		/// </summary>
		public bool ContractLocation
		{
			get => this.contractLocation;
			set => this.contractLocation = value;
		}

		/// <summary>
		/// Optional minimum value.
		/// </summary>
		public GeoPosition Min
		{
			get => this.min;
			set => this.min = value;
		}

		/// <summary>
		/// Optional maximum value.
		/// </summary>
		public GeoPosition Max
		{
			get => this.max;
			set => this.max = value;
		}

		/// <summary>
		/// If the optional minimum value is included in the allowed range.
		/// </summary>
		public bool MinIncluded
		{
			get => this.minIncluded;
			set => this.minIncluded = value;
		}

		/// <summary>
		/// If the optional maximum value is included in the allowed range.
		/// </summary>
		public bool MaxIncluded
		{
			get => this.maxIncluded;
			set => this.maxIncluded = value;
		}

		/// <summary>
		/// How altitudes are managed by the parameter
		/// </summary>
		public AltitudeUse Altitude
		{
			get => this.altitude;
			set => this.altitude = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.@value;

		/// <summary>
		/// String representation of value.
		/// </summary>
		public override string StringValue
		{
			get => !(this.Value is null) ? this.Value.ToString() : string.Empty;
			set
			{
				if (GeoPosition.TryParse(value, out GeoPosition D))
				{
					if (this.altitude == AltitudeUse.Prohibited && D.Altitude.HasValue)
						this.Value = null;
					else if (this.altitude == AltitudeUse.Required && !D.Altitude.HasValue)
						this.Value = null;
					else
						this.Value = D;
				}
				else
					this.Value = null;
			}
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "geoParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<geoParameter");

			if (!UsingTemplate)
			{
				Xml.Append(" altitude=\"");
				Xml.Append(this.altitude.ToString().ToLower());
				Xml.Append('"');

				if (this.contractLocation)
					Xml.Append(" contractLocation=\"true\"");

				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append(" exp=\"");
					Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.Guide))
				{
					Xml.Append(" guide=\"");
					Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!(this.max is null))
				{
					Xml.Append(" max=\"");
					Xml.Append(this.max.XmlValue);
					Xml.Append("\" maxIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.maxIncluded)));
					Xml.Append('"');
				}

				if (!(this.min is null))
				{
					Xml.Append(" min=\"");
					Xml.Append(this.min.XmlValue);
					Xml.Append("\" minIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.minIncluded)));
					Xml.Append('"');
				}
			}

			Xml.Append(" name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append('"');

			if (this.CanSerializeProtectedValue)
			{
				Xml.Append(" protected=\"");
				Xml.Append(Convert.ToBase64String(this.ProtectedValue));
				Xml.Append('"');
			}

			if (!UsingTemplate && this.Protection != ProtectionLevel.Normal)
			{
				Xml.Append(" protection=\"");
				Xml.Append(this.Protection.ToString());
				Xml.Append('"');
			}

			if (!(this.@value is null) && this.CanSerializeValue)
			{
				Xml.Append(" value=\"");
				Xml.Append(this.@value.XmlValue);
				Xml.Append('"');
			}

			if (UsingTemplate || this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("/>");
			else
			{
				Xml.Append('>');

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", null);

				Xml.Append("</geoParameter>");
			}
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is GeoPosition Position)
				this.Value = Position;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			if (Value is GeoPosition Position)
			{
				this.Min = Position;

				if (Inclusive.HasValue)
					this.MinIncluded = Inclusive.Value;
			}
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			if (Value is GeoPosition Position)
			{
				this.Max = Position;

				if (Inclusive.HasValue)
					this.MaxIncluded = Inclusive.Value;
			}
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.@value;
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public override Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			if (this.Value is null)
			{
				this.ErrorReason = ParameterErrorReason.LacksValue;
				this.ErrorText = null;

				return Task.FromResult(false);
			}

			if ((!(this.min is null) || !(this.max is null)) &&
				this.value.LiesOutside(this.min, this.max, !this.minIncluded, !this.maxIncluded))
			{
				this.ErrorReason = ParameterErrorReason.Outside;
				this.ErrorText = null;

				return Task.FromResult(false);
			}

			switch (this.altitude)
			{
				case AltitudeUse.Required:
					if (!this.value.Altitude.HasValue)
					{
						this.ErrorReason = ParameterErrorReason.Outside;
						this.ErrorText = null;

						return Task.FromResult(false);
					}
					break;

				case AltitudeUse.Prohibited:
					if (this.value.Altitude.HasValue)
					{
						this.ErrorReason = ParameterErrorReason.Outside;
						this.ErrorText = null;

						return Task.FromResult(false);
					}
					break;
			}

			return base.IsParameterValid(Variables, Client);
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override Task<bool> Import(XmlElement Xml)
		{
			this.Value = GeoPositionAttribute(Xml, "value");
			this.contractLocation = XML.Attribute(Xml, "contractLocation", false);
			this.Min = GeoPositionAttribute(Xml, "min");
			this.MinIncluded = XML.Attribute(Xml, "minIncluded", true);
			this.Max = GeoPositionAttribute(Xml, "max");
			this.MaxIncluded = XML.Attribute(Xml, "maxIncluded", true);
			this.altitude = XML.Attribute(Xml, "altitude", true, AltitudeUse.Optional);

			return base.Import(Xml);
		}

		/// <summary>
		/// Parses an Geo-spatial position from an XML attribute.
		/// </summary>
		/// <param name="Xml">XML Element</param>
		/// <param name="Name">Attribute name.</param>
		/// <returns>Geo-spatial position, if able to parse value.</returns>
		internal static GeoPosition GeoPositionAttribute(XmlElement Xml, string Name)
		{
			if (!Xml.HasAttribute(Name))
				return null;

			string s = XML.Attribute(Xml, Name);
			if (GeoPosition.TryParse(s, out GeoPosition Result))
				return Result;
			else
				return null;
		}

	}
}
