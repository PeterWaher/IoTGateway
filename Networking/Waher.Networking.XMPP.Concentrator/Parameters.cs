using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Things.Attributes;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Static class managing editable parameters in objects. Editable parameters are defined by using the 
	/// attributes defined in the <see cref="Waher.Things.Attributes"/> namespace.
	/// </summary>
	public static class Parameters
	{
		/// <summary>
		/// Gets a data form containing editable parameters from an object.
		/// </summary>
		/// <param name="Client">Client</param>
		/// <param name="e">IQ Event Arguments describing the request.</param>
		/// <param name="EditableObject">Object whose parameters will be edited.</param>
		/// <param name="Title">Title of form.</param>
		/// <returns>Data form containing editable parameters.</returns>
		public static async Task<DataForm> GetEditableForm(XmppClient Client, IqEventArgs e, object EditableObject, string Title)
		{
			Type T = EditableObject.GetType();
			string DefaultLanguageCode = GetDefaultLanguageCode(T);
			DataForm Parameters = new DataForm(Client, FormType.Form, e.To, e.From);
			Language Language = await ConcentratorServer.GetLanguage(e.Query, DefaultLanguageCode);
			Namespace Namespace = null;
			List<Field> Fields = new List<Field>();
			List<Page> Pages = new List<Page>();
			Dictionary<string, Page> PageByLabel = new Dictionary<string, Page>();
			Dictionary<string, Section> SectionByPageAndSectionLabel = null;
			List<KeyValuePair<string, string>> Options = null;
			string NamespaceStr;
			string LastNamespaceStr = null;
			string Header;
			string ToolTip;
			string PageLabel;
			string SectionLabel;
			string s;
			int StringId;
			int PagePriority;
			int PageOrdinal = 0;
			int FieldPriority;
			int FieldOrdinal = 0;
			bool Required;
			bool ReadOnly;
			bool Masked;
			bool Alpha;
			bool DateOnly;
			bool Nullable;
			HeaderAttribute HeaderAttribute;
			ToolTipAttribute ToolTipAttribute;
			PageAttribute PageAttribute;
			SectionAttribute SectionAttribute;
			OptionAttribute OptionAttribute;
			TextAttribute TextAttribute;
			RegularExpressionAttribute RegularExpressionAttribute;
			LinkedList<TextAttribute> TextAttributes;
			RangeAttribute RangeAttribute;
			ValidationMethod ValidationMethod;
			Type PropertyType;
			Field Field;
			Page DefaultPage = null;
			Page Page;

			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(T.Namespace);

			LinkedList<KeyValuePair<PropertyInfo, FieldInfo>> Properties = new LinkedList<KeyValuePair<PropertyInfo, FieldInfo>>();

			foreach (PropertyInfo PI in T.GetRuntimeProperties())
			{
				if (PI.CanRead)
					Properties.AddLast(new KeyValuePair<PropertyInfo, FieldInfo>(PI, null));
			}

			foreach (FieldInfo FI in T.GetRuntimeFields())
				Properties.AddLast(new KeyValuePair<PropertyInfo, FieldInfo>(null, FI));

			foreach (KeyValuePair<PropertyInfo, FieldInfo> Rec in Properties)
			{
				PropertyInfo PropertyInfo = Rec.Key;
				FieldInfo FieldInfo = Rec.Value;

				NamespaceStr = (PropertyInfo?.DeclaringType ?? FieldInfo.DeclaringType).Namespace;
				if (Namespace == null || NamespaceStr != LastNamespaceStr)
				{
					Namespace = await Language.GetNamespaceAsync(NamespaceStr);
					LastNamespaceStr = NamespaceStr;
				}

				Header = ToolTip = PageLabel = SectionLabel = null;
				TextAttributes = null;
				ValidationMethod = null;
				Options = null;
				Required = Masked = Alpha = DateOnly = false;
				ReadOnly = PropertyInfo != null && !PropertyInfo.CanWrite;
				PagePriority = PageAttribute.DefaultPriority;
				FieldPriority = HeaderAttribute.DefaultPriority;

				foreach (Attribute Attr in (PropertyInfo?.GetCustomAttributes() ?? FieldInfo.GetCustomAttributes()))
				{
					if ((HeaderAttribute = Attr as HeaderAttribute) != null)
					{
						Header = HeaderAttribute.Header;
						StringId = HeaderAttribute.StringId;
						FieldPriority = HeaderAttribute.Priority;
						if (StringId > 0)
							Header = await Namespace.GetStringAsync(StringId, Header);
					}
					else if ((ToolTipAttribute = Attr as ToolTipAttribute) != null)
					{
						ToolTip = ToolTipAttribute.ToolTip;
						StringId = ToolTipAttribute.StringId;
						if (StringId > 0)
							ToolTip = await Namespace.GetStringAsync(StringId, ToolTip);
					}
					else if ((PageAttribute = Attr as PageAttribute) != null)
					{
						PageLabel = PageAttribute.Label;
						StringId = PageAttribute.StringId;
						PagePriority = PageAttribute.Priority;
						if (StringId > 0)
							PageLabel = await Namespace.GetStringAsync(StringId, PageLabel);
					}
					else if ((SectionAttribute = Attr as SectionAttribute) != null)
					{
						SectionLabel = SectionAttribute.Label;
						StringId = SectionAttribute.StringId;
						if (StringId > 0)
							SectionLabel = await Namespace.GetStringAsync(StringId, SectionLabel);
					}
					else if ((TextAttribute = Attr as TextAttribute) != null)
					{
						if (TextAttributes == null)
							TextAttributes = new LinkedList<TextAttribute>();

						TextAttributes.AddLast(TextAttribute);
					}
					else if ((OptionAttribute = Attr as OptionAttribute) != null)
					{
						if (Options == null)
							Options = new List<KeyValuePair<string, string>>();

						StringId = OptionAttribute.StringId;
						if (StringId > 0)
						{
							Options.Add(new KeyValuePair<string, string>(OptionAttribute.Option.ToString(),
								await Namespace.GetStringAsync(StringId, OptionAttribute.Label)));
						}
						else
							Options.Add(new KeyValuePair<string, string>(OptionAttribute.Option.ToString(), OptionAttribute.Label));
					}
					else if ((RegularExpressionAttribute = Attr as RegularExpressionAttribute) != null)
						ValidationMethod = new RegexValidation(RegularExpressionAttribute.Pattern);
					else if ((RangeAttribute = Attr as RangeAttribute) != null)
						ValidationMethod = new RangeValidation(RangeAttribute.Min, RangeAttribute.Max);
					else if (Attr is OpenAttribute)
						ValidationMethod = new OpenValidation();
					else if (Attr is RequiredAttribute)
						Required = true;
					else if (Attr is ReadOnlyAttribute)
						ReadOnly = true;
					else if (Attr is MaskedAttribute)
						Masked = true;
					else if (Attr is AlphaChannelAttribute)
						Alpha = true;
					else if (Attr is DateOnlyAttribute)
						DateOnly = true;
				}

				if (Header == null)
					continue;

				PropertyType = PropertyInfo?.PropertyType ?? FieldInfo.FieldType;
				Field = null;
				Nullable = false;

				if (PropertyType.GetTypeInfo().IsGenericType)
				{
					Type GT = PropertyType.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Nullable = true;
						PropertyType = PropertyType.GenericTypeArguments[0];
					}
				}

				string PropertyName = PropertyInfo?.Name ?? FieldInfo.Name;
				object PropertyValue = (PropertyInfo?.GetValue(EditableObject) ?? FieldInfo.GetValue(EditableObject));

				if (PropertyType == typeof(string[]))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					if (Options == null)
					{
						Field = new TextMultiField(Parameters, PropertyName, Header, Required, (string[])PropertyValue,
							null, ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
					else
					{
						Field = new ListMultiField(Parameters, PropertyName, Header, Required, (string[])PropertyValue,
							Options.ToArray(), ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
				}
				else if (PropertyType.GetTypeInfo().IsEnum)
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					if (Options == null)
					{
						Options = new List<KeyValuePair<string, string>>();

						foreach (string Option in Enum.GetNames(PropertyType))
							Options.Add(new KeyValuePair<string, string>(Option, Option));
					}

					s = PropertyValue?.ToString();
					if (Nullable && s == null)
						s = string.Empty;

					Field = new ListSingleField(Parameters, PropertyName, Header, Required, new string[] { s },
						Options.ToArray(), ToolTip, null, ValidationMethod, string.Empty, false, ReadOnly, false);
				}
				else if (PropertyType == typeof(bool))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					if (Nullable && PropertyValue == null)
						s = string.Empty;
					else
						s = CommonTypes.Encode((bool)PropertyValue);

					Field = new BooleanField(Parameters, PropertyName, Header, Required, new string[] { s },
						Options?.ToArray(), ToolTip, BooleanDataType.Instance, ValidationMethod,
						string.Empty, false, ReadOnly, false);
				}
				else
				{
					DataType DataType;

					if (PropertyType == typeof(string))
						DataType = StringDataType.Instance;
					else if (PropertyType == typeof(sbyte))
						DataType = ByteDataType.Instance;
					else if (PropertyType == typeof(short))
						DataType = ShortDataType.Instance;
					else if (PropertyType == typeof(int))
						DataType = IntDataType.Instance;
					else if (PropertyType == typeof(long))
						DataType = LongDataType.Instance;
					else if (PropertyType == typeof(byte))
					{
						DataType = ShortDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(byte.MinValue.ToString(), byte.MaxValue.ToString());
					}
					else if (PropertyType == typeof(ushort))
					{
						DataType = IntDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(ushort.MinValue.ToString(), ushort.MaxValue.ToString());
					}
					else if (PropertyType == typeof(uint))
					{
						DataType = LongDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(uint.MinValue.ToString(), uint.MaxValue.ToString());
					}
					else if (PropertyType == typeof(ulong))
					{
						DataType = IntegerDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(ulong.MinValue.ToString(), ulong.MaxValue.ToString());
					}
					else if (PropertyType == typeof(DateTime))
					{
						if (DateOnly)
							DataType = DateDataType.Instance;
						else
							DataType = DateTimeDataType.Instance;
					}
					else if (PropertyType == typeof(decimal))
						DataType = DecimalDataType.Instance;
					else if (PropertyType == typeof(double))
						DataType = DoubleDataType.Instance;
					else if (PropertyType == typeof(float))
						DataType = DoubleDataType.Instance;    // Use xs:double anyway
					else if (PropertyType == typeof(TimeSpan))
						DataType = TimeDataType.Instance;
					else if (PropertyType == typeof(Uri))
						DataType = AnyUriDataType.Instance;
					else if (PropertyType == typeof(SKColor))
					{
						if (Alpha)
							DataType = ColorAlphaDataType.Instance;
						else
							DataType = ColorDataType.Instance;
					}
					else
						DataType = StringDataType.Instance;

					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					s = PropertyValue?.ToString();
					if (Nullable && s == null)
						s = string.Empty;

					if (Masked)
					{
						Field = new TextPrivateField(Parameters, PropertyName, Header, Required, new string[] { s },
							Options?.ToArray(), ToolTip, DataType, ValidationMethod,
							string.Empty, false, ReadOnly, false);
					}
					else if (Options == null)
					{
						Field = new TextSingleField(Parameters, PropertyName, Header, Required, new string[] { s },
							null, ToolTip, DataType, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
					else
					{
						Field = new ListSingleField(Parameters, PropertyName, Header, Required, new string[] { s },
							Options.ToArray(), ToolTip, DataType, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
				}

				if (Field == null)
					continue;

				if (string.IsNullOrEmpty(PageLabel))
				{
					if (DefaultPage == null)
					{
						DefaultPage = new Page(Parameters, string.Empty)
						{
							Priority = PageAttribute.DefaultPriority,
							Ordinal = PageOrdinal++
						};
						Pages.Add(DefaultPage);
						PageByLabel[string.Empty] = DefaultPage;
					}

					Page = DefaultPage;
					PageLabel = string.Empty;
				}
				else
				{
					if (!PageByLabel.TryGetValue(PageLabel, out Page))
					{
						Page = new Page(Parameters, PageLabel)
						{
							Priority = PagePriority,
							Ordinal = PageOrdinal++
						};
						Pages.Add(Page);
						PageByLabel[PageLabel] = Page;
					}
				}

				Field.Priority = FieldPriority;
				Field.Ordinal = FieldOrdinal++;
				Fields.Add(Field);

				if (string.IsNullOrEmpty(SectionLabel))
				{
					if (TextAttributes != null)
					{
						foreach (TextAttribute TextAttr in TextAttributes)
						{
							if (TextAttr.Position == TextPosition.BeforeField)
							{
								StringId = TextAttr.StringId;
								if (StringId > 0)
									Page.Add(new TextElement(Parameters, await Namespace.GetStringAsync(StringId, TextAttr.Label)));
								else
									Page.Add(new TextElement(Parameters, TextAttr.Label));
							}
						}
					}

					Page.Add(new FieldReference(Parameters, Field.Var));

					if (TextAttributes != null)
					{
						foreach (TextAttribute TextAttr in TextAttributes)
						{
							if (TextAttr.Position == TextPosition.AfterField)
							{
								StringId = TextAttr.StringId;
								if (StringId > 0)
									Page.Add(new TextElement(Parameters, await Namespace.GetStringAsync(StringId, TextAttr.Label)));
								else
									Page.Add(new TextElement(Parameters, TextAttr.Label));
							}
						}
					}
				}
				else
				{
					if (SectionByPageAndSectionLabel == null)
						SectionByPageAndSectionLabel = new Dictionary<string, Section>();

					s = PageLabel + " \xa0 " + SectionLabel;
					if (!SectionByPageAndSectionLabel.TryGetValue(s, out Section Section))
					{
						Section = new Section(Parameters, SectionLabel);
						SectionByPageAndSectionLabel[s] = Section;

						Page.Add(Section);
					}

					if (TextAttributes != null)
					{
						foreach (TextAttribute TextAttr in TextAttributes)
						{
							if (TextAttr.Position == TextPosition.BeforeField)
							{
								StringId = TextAttr.StringId;
								if (StringId > 0)
									Section.Add(new TextElement(Parameters, await Namespace.GetStringAsync(StringId, TextAttr.Label)));
								else
									Section.Add(new TextElement(Parameters, TextAttr.Label));
							}
						}
					}

					Section.Add(new FieldReference(Parameters, Field.Var));

					if (TextAttributes != null)
					{
						foreach (TextAttribute TextAttr in TextAttributes)
						{
							if (TextAttr.Position == TextPosition.AfterField)
							{
								StringId = TextAttr.StringId;
								if (StringId > 0)
									Section.Add(new TextElement(Parameters, await Namespace.GetStringAsync(StringId, TextAttr.Label)));
								else
									Section.Add(new TextElement(Parameters, TextAttr.Label));
							}
						}
					}
				}
			}

			if (EditableObject is IPropertyFormAnnotation PropertyFormAnnotation)
			{
				FormState State = new FormState()
				{
					Form = Parameters,
					PageByLabel = PageByLabel,
					SectionByPageAndSectionLabel = SectionByPageAndSectionLabel,
					DefaultPage = DefaultPage,
					LanguageCode = Language.Code,
					Fields = Fields,
					Pages = Pages,
					FieldOrdinal = FieldOrdinal,
					PageOrdinal = PageOrdinal
				};

				await PropertyFormAnnotation.AnnotatePropertyForm(State);
			}

			Fields.Sort(OrderFields);
			Parameters.Title = Title;
			Parameters.Fields = Fields.ToArray();

			if (Pages != null)
			{
				Pages.Sort(OrderPages);

				foreach (Page Page2 in Pages)
					Page2.Sort();
			}

			Parameters.Pages = Pages.ToArray();

			return Parameters;
		}

		private static int OrderPages(Page x, Page y)
		{
			int i = x.Priority - y.Priority;
			if (i != 0)
				return i;

			return x.Ordinal - y.Ordinal;
		}

		private static int OrderFields(Field x, Field y)
		{
			int i = x.Priority - y.Priority;
			if (i != 0)
				return i;

			return x.Ordinal - y.Ordinal;
		}

		private static string GetDefaultLanguageCode(Type Type)
		{
			string DefaultLanguageCode = null;

			foreach (DefaultLanguageAttribute Attr in Type.GetTypeInfo().GetCustomAttributes(typeof(DefaultLanguageAttribute), true))
			{
				DefaultLanguageCode = Attr.LanguageCode;
				if (!string.IsNullOrEmpty(DefaultLanguageCode))
					break;
			}

			if (string.IsNullOrEmpty(DefaultLanguageCode))
				DefaultLanguageCode = Translator.DefaultLanguageCode;

			return DefaultLanguageCode;
		}

		private static void AddError(ref List<KeyValuePair<string, string>> Errors, string Field, string Error)
		{
			if (Errors == null)
				Errors = new List<KeyValuePair<string, string>>();

			Errors.Add(new KeyValuePair<string, string>(Field, Error));
		}

		/// <summary>
		/// Result of a set properties operation.
		/// </summary>
		public class SetEditableFormResult
		{
			/// <summary>
			/// If any errors were encountered.
			/// </summary>
			public KeyValuePair<string, string>[] Errors;

			/// <summary>
			/// Actual property values set.
			/// </summary>
			public List<KeyValuePair<string, object>> Tags;
		}

		/// <summary>
		/// Sets parameters from a data form in an object.
		/// </summary>
		/// <param name="e">IQ Event Arguments describing the request.</param>
		/// <param name="EditableObject">Object whose parameters will be set.</param>
		/// <param name="Form">Data Form.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public static async Task<SetEditableFormResult> SetEditableForm(IqEventArgs e, object EditableObject, DataForm Form, bool OnlySetChanged)
		{
			Type T = EditableObject.GetType();
			string DefaultLanguageCode = GetDefaultLanguageCode(T);
			List<KeyValuePair<string, string>> Errors = null;
			PropertyInfo PropertyInfo;
			FieldInfo FieldInfo;
			Language Language = await ConcentratorServer.GetLanguage(e.Query, DefaultLanguageCode);
			Namespace Namespace = null;
			Namespace ConcentratorNamespace = await Language.GetNamespaceAsync(typeof(ConcentratorServer).Namespace);
			LinkedList<Tuple<PropertyInfo, FieldInfo, object>> ToSet = null;
			ValidationMethod ValidationMethod;
			OptionAttribute OptionAttribute;
			RegularExpressionAttribute RegularExpressionAttribute;
			RangeAttribute RangeAttribute;
			DataType DataType;
			Type PropertyType;
			string NamespaceStr;
			string LastNamespaceStr = null;
			object ValueToSet;
			object ValueToSet2;
			object[] Parsed;
			bool ReadOnly;
			bool Alpha;
			bool DateOnly;
			bool HasHeader;
			bool HasOptions;
			bool ValidOption;
			bool Nullable;

			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(T.Namespace);

			if (ConcentratorNamespace == null)
				ConcentratorNamespace = await Language.CreateNamespaceAsync(typeof(ConcentratorServer).Namespace);

			foreach (Field Field in Form.Fields)
			{
				PropertyInfo = T.GetRuntimeProperty(Field.Var);
				FieldInfo = PropertyInfo == null ? T.GetRuntimeField(Field.Var) : null;

				if (PropertyInfo == null && FieldInfo == null)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(1, "Property not found."));
					continue;
				}

				if (PropertyInfo != null && (!PropertyInfo.CanRead || !PropertyInfo.CanWrite))
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(2, "Property not editable."));
					continue;
				}

				NamespaceStr = (PropertyInfo?.DeclaringType ?? FieldInfo.DeclaringType).Namespace;
				if (Namespace == null || NamespaceStr != LastNamespaceStr)
				{
					Namespace = await Language.GetNamespaceAsync(NamespaceStr);
					LastNamespaceStr = NamespaceStr;
				}

				ValidationMethod = null;
				ReadOnly = Alpha = DateOnly = HasHeader = HasOptions = ValidOption = false;

				foreach (Attribute Attr in (PropertyInfo?.GetCustomAttributes() ?? FieldInfo.GetCustomAttributes()))
				{
					if (Attr is HeaderAttribute)
						HasHeader = true;
					else if ((OptionAttribute = Attr as OptionAttribute) != null)
					{
						HasOptions = true;
						if (Field.ValueString == OptionAttribute.Option.ToString())
							ValidOption = true;
					}
					else if ((RegularExpressionAttribute = Attr as RegularExpressionAttribute) != null)
						ValidationMethod = new RegexValidation(RegularExpressionAttribute.Pattern);
					else if ((RangeAttribute = Attr as RangeAttribute) != null)
						ValidationMethod = new RangeValidation(RangeAttribute.Min, RangeAttribute.Max);
					else if (Attr is OpenAttribute)
						ValidationMethod = new OpenValidation();
					else if (Attr is ReadOnlyAttribute)
						ReadOnly = true;
					else if (Attr is AlphaChannelAttribute)
						Alpha = true;
					else if (Attr is DateOnlyAttribute)
						DateOnly = true;
				}

				if (!HasHeader)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(2, "Property not editable."));
					continue;
				}

				if (ReadOnly)
				{
					if (Field.ValueString != (PropertyInfo?.GetValue(EditableObject) ?? FieldInfo?.GetValue(EditableObject))?.ToString())
						AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(3, "Property is read-only."));

					continue;
				}

				if (HasOptions && !ValidOption)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(4, "Select a valid option."));
					continue;
				}

				PropertyType = PropertyInfo?.PropertyType ?? FieldInfo.FieldType;
				ValueToSet = null;
				ValueToSet2 = null;
				Parsed = null;
				DataType = null;
				Nullable = false;

				if (PropertyType.GetTypeInfo().IsGenericType)
				{
					Type GT = PropertyType.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Nullable = true;
						PropertyType = PropertyType.GenericTypeArguments[0];
					}
				}

				if (Nullable && string.IsNullOrEmpty(Field.ValueString))
					ValueToSet2 = null;
				else
				{
					if (PropertyType == typeof(string[]))
					{
						if (ValidationMethod == null)
							ValidationMethod = new BasicValidation();

						ValueToSet = ValueToSet2 = Parsed = Field.ValueStrings;
						DataType = StringDataType.Instance;
					}
					else if (PropertyType.GetTypeInfo().IsEnum)
					{
						if (ValidationMethod == null)
							ValidationMethod = new BasicValidation();

						try
						{
							ValueToSet = ValueToSet2 = Enum.Parse(PropertyType, Field.ValueString);
						}
						catch (Exception)
						{
							AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(4, "Select a valid option."));
							continue;
						}
					}
					else if (PropertyType == typeof(bool))
					{
						if (ValidationMethod == null)
							ValidationMethod = new BasicValidation();

						if (!CommonTypes.TryParse(Field.ValueString, out bool b))
						{
							AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(5, "Invalid boolean value."));
							continue;
						}

						DataType = BooleanDataType.Instance;
						ValueToSet = ValueToSet2 = b;
					}
					else
					{
						if (PropertyType == typeof(string))
							DataType = StringDataType.Instance;
						else if (PropertyType == typeof(sbyte))
							DataType = ByteDataType.Instance;
						else if (PropertyType == typeof(short))
							DataType = ShortDataType.Instance;
						else if (PropertyType == typeof(int))
							DataType = IntDataType.Instance;
						else if (PropertyType == typeof(long))
							DataType = LongDataType.Instance;
						else if (PropertyType == typeof(byte))
						{
							DataType = ShortDataType.Instance;

							if (ValidationMethod == null)
								ValidationMethod = new RangeValidation(byte.MinValue.ToString(), byte.MaxValue.ToString());
						}
						else if (PropertyType == typeof(ushort))
						{
							DataType = IntDataType.Instance;

							if (ValidationMethod == null)
								ValidationMethod = new RangeValidation(ushort.MinValue.ToString(), ushort.MaxValue.ToString());
						}
						else if (PropertyType == typeof(uint))
						{
							DataType = LongDataType.Instance;

							if (ValidationMethod == null)
								ValidationMethod = new RangeValidation(uint.MinValue.ToString(), uint.MaxValue.ToString());
						}
						else if (PropertyType == typeof(ulong))
						{
							DataType = IntegerDataType.Instance;

							if (ValidationMethod == null)
								ValidationMethod = new RangeValidation(ulong.MinValue.ToString(), ulong.MaxValue.ToString());
						}
						else if (PropertyType == typeof(DateTime))
						{
							if (DateOnly)
								DataType = DateDataType.Instance;
							else
								DataType = DateTimeDataType.Instance;
						}
						else if (PropertyType == typeof(decimal))
							DataType = DecimalDataType.Instance;
						else if (PropertyType == typeof(double))
							DataType = DoubleDataType.Instance;
						else if (PropertyType == typeof(float))
							DataType = DoubleDataType.Instance;    // Use xs:double anyway
						else if (PropertyType == typeof(TimeSpan))
							DataType = TimeDataType.Instance;
						else if (PropertyType == typeof(Uri))
							DataType = AnyUriDataType.Instance;
						else if (PropertyType == typeof(SKColor))
						{
							if (Alpha)
								DataType = ColorAlphaDataType.Instance;
							else
								DataType = ColorDataType.Instance;
						}
						else
							DataType = null;

						if (ValidationMethod == null)
							ValidationMethod = new BasicValidation();

						try
						{
							if (DataType == null)
							{
								ValueToSet = Field.ValueString;
								ValueToSet2 = Activator.CreateInstance(PropertyType, ValueToSet);
							}
							else
							{
								ValueToSet = DataType.Parse(Field.ValueString);

								if (ValueToSet.GetType() == PropertyType)
									ValueToSet2 = ValueToSet;
								else
									ValueToSet2 = Convert.ChangeType(ValueToSet, PropertyType);
							}
						}
						catch (Exception)
						{
							AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(6, "Invalid value."));
							continue;
						}
					}

					if (Parsed == null)
						Parsed = new object[] { ValueToSet };

					ValidationMethod.Validate(Field, DataType, Parsed, Field.ValueStrings);
					if (Field.HasError)
					{
						AddError(ref Errors, Field.Var, Field.Error);
						continue;
					}
				}

				if (ToSet == null)
					ToSet = new LinkedList<Tuple<PropertyInfo, FieldInfo, object>>();

				ToSet.AddLast(new Tuple<PropertyInfo, FieldInfo, object>(PropertyInfo, FieldInfo, ValueToSet2));
			}

			if (Errors == null)
			{
				SetEditableFormResult Result = new SetEditableFormResult()
				{
					Errors = null,
					Tags = new List<KeyValuePair<string, object>>()
				};

				foreach (Tuple<PropertyInfo, FieldInfo, object> P in ToSet)
				{
					try
					{
						if (OnlySetChanged)
						{
							object Current = P.Item1?.GetValue(EditableObject) ?? P.Item2?.GetValue(EditableObject);

							if (Current == null)
							{
								if (P.Item3 == null)
									continue;
							}
							else if (P.Item3 != null && Current.Equals(P.Item3))
								continue;
						}

						if (P.Item1 != null)
						{
							P.Item1.SetValue(EditableObject, P.Item3);
							Result.Tags.Add(new KeyValuePair<string, object>(P.Item1.Name, P.Item3));
						}
						else
						{
							P.Item2.SetValue(EditableObject, P.Item3);
							Result.Tags.Add(new KeyValuePair<string, object>(P.Item2.Name, P.Item3));
						}
					}
					catch (Exception ex)
					{
						AddError(ref Errors, P.Item1?.Name ?? P.Item2.Name, ex.Message);
					}
				}

				return Result;
			}
			else
			{
				return new SetEditableFormResult()
				{
					Errors = Errors.ToArray(),
					Tags = null
				};
			}
		}

		/// <summary>
		/// Merge two forms. The <paramref name="MainForm"/> will be adjusted, and only common options will be left.
		/// </summary>
		/// <param name="MainForm">Main form.</param>
		/// <param name="SecondaryForm">Secondary form.</param>
		public static void MergeForms(DataForm MainForm, DataForm SecondaryForm)
		{
			Field F2;

			foreach (Field F in MainForm.Fields)
			{
				if (F.Exclude)
					continue;

				F2 = SecondaryForm[F.Var];
				if (F2 == null || !F.Merge(F2))
					F.Exclude = true;
			}
		}

	}
}
