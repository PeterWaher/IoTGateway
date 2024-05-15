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
using Waher.Runtime.Inventory;
using Waher.Content.Xml;

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
			Language Language = await ConcentratorServer.GetLanguage(e.Query, DefaultLanguageCode);

			return await GetEditableForm(Client, Language, e.From, e.To, EditableObject, Title);
		}

		/// <summary>
		/// Gets a data form containing editable parameters from an object.
		/// </summary>
		/// <param name="Client">Client</param>
		/// <param name="Language">Language to use for localized strings.</param>
		/// <param name="From">From addres</param>
		/// <param name="To">To addres</param>
		/// <param name="EditableObject">Object whose parameters will be edited.</param>
		/// <param name="Title">Title of form.</param>
		/// <returns>Data form containing editable parameters.</returns>
		public static async Task<DataForm> GetEditableForm(XmppClient Client, Language Language, string From, string To, object EditableObject, string Title)
		{
			Type T = EditableObject.GetType();
			DataForm Parameters = new DataForm(Client, FormType.Form, To, From);

			if (EditableObject is IEditableObject Editable)
				await Editable.PopulateForm(Parameters, Language);
			else
			{
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
				string ContentType;
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
				PageAttribute PageAttribute;
				TextAttribute TextAttribute;
				LinkedList<TextAttribute> TextAttributes;
				ValidationMethod ValidationMethod;
				Type PropertyType;
				Field Field;
				Page DefaultPage = null;
				Page Page;

				if (Namespace is null)
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
					if (Namespace is null || NamespaceStr != LastNamespaceStr)
					{
						Namespace = await Language.GetNamespaceAsync(NamespaceStr);
						LastNamespaceStr = NamespaceStr;
					}

					Header = ToolTip = PageLabel = SectionLabel = null;
					TextAttributes = null;
					ValidationMethod = null;
					Options = null;
					ContentType = null;
					Required = Masked = Alpha = DateOnly = false;
					ReadOnly = !(PropertyInfo is null) && !PropertyInfo.CanWrite;
					PagePriority = PageAttribute.DefaultPriority;
					FieldPriority = HeaderAttribute.DefaultPriority;

					foreach (Attribute Attr in (PropertyInfo?.GetCustomAttributes() ?? FieldInfo.GetCustomAttributes()))
					{
						if (!((HeaderAttribute = Attr as HeaderAttribute) is null))
						{
							Header = HeaderAttribute.Header;
							StringId = HeaderAttribute.StringId;
							FieldPriority = HeaderAttribute.Priority;
							if (StringId > 0)
								Header = await Namespace.GetStringAsync(StringId, Header);
						}
						else if (Attr is ToolTipAttribute ToolTipAttribute)
						{
							ToolTip = ToolTipAttribute.ToolTip;
							StringId = ToolTipAttribute.StringId;
							if (StringId > 0)
								ToolTip = await Namespace.GetStringAsync(StringId, ToolTip);
						}
						else if (!((PageAttribute = Attr as PageAttribute) is null))
						{
							PageLabel = PageAttribute.Label;
							StringId = PageAttribute.StringId;
							PagePriority = PageAttribute.Priority;
							if (StringId > 0)
								PageLabel = await Namespace.GetStringAsync(StringId, PageLabel);
						}
						else if (Attr is SectionAttribute SectionAttribute)
						{
							SectionLabel = SectionAttribute.Label;
							StringId = SectionAttribute.StringId;
							if (StringId > 0)
								SectionLabel = await Namespace.GetStringAsync(StringId, SectionLabel);
						}
						else if (!((TextAttribute = Attr as TextAttribute) is null))
						{
							if (TextAttributes is null)
								TextAttributes = new LinkedList<TextAttribute>();

							TextAttributes.AddLast(TextAttribute);
						}
						else if (Attr is OptionAttribute OptionAttribute)
						{
							if (Options is null)
								Options = new List<KeyValuePair<string, string>>();

							StringId = OptionAttribute.StringId;
							if (StringId > 0)
							{
								Options.Add(new KeyValuePair<string, string>(
									await Namespace.GetStringAsync(StringId, OptionAttribute.Label),
									OptionAttribute.Option.ToString()));
							}
							else
								Options.Add(new KeyValuePair<string, string>(OptionAttribute.Label, OptionAttribute.Option.ToString()));
						}
						else if (Attr is DynamicOptionsAttribute DynamicOptions)
						{
							MethodInfo MI = T.GetRuntimeMethod(DynamicOptions.MethodName, Types.NoTypes)
								?? throw new Exception("Dynamic options method (" + DynamicOptions.MethodName +
									") missing on editable object of type " + T.FullName + ".");

							object Result = MI.Invoke(EditableObject, Types.NoParameters);

							if (!(Result is null))
							{
								if (!(Result is OptionAttribute[] OptionAttributes))
								{
									throw new Exception("Expected an OptionAttribute array (or null) as a result from calling " +
										DynamicOptions.MethodName + " on editable objects of type " + T.FullName + ".");
								}

								if (Options is null)
									Options = new List<KeyValuePair<string, string>>();

								foreach (OptionAttribute OptionAttribute2 in OptionAttributes)
								{
									StringId = OptionAttribute2.StringId;
									if (StringId > 0)
									{
										Options.Add(new KeyValuePair<string, string>(
											await Namespace.GetStringAsync(StringId, OptionAttribute2.Label),
											OptionAttribute2.Option.ToString()));
									}
									else
										Options.Add(new KeyValuePair<string, string>(OptionAttribute2.Label, OptionAttribute2.Option.ToString()));
								}
							}
						}
						else if (Attr is ContentTypeAttribute ContentTypeAttribute)
							ContentType = ContentTypeAttribute.ContentType;
						else if (Attr is DynamicContentTypeAttribute DynamicContentTypeAttribute)
						{
							MethodInfo MI = T.GetRuntimeMethod(DynamicContentTypeAttribute.MethodName, Types.NoTypes)
								?? throw new Exception("Dynamic Content-Type method (" + DynamicContentTypeAttribute.MethodName +
									") missing on editable object of type " + T.FullName + ".");

							object Result = MI.Invoke(EditableObject, Types.NoParameters);

							if (!(Result is null))
							{
								if (Result is ContentTypeAttribute ContentTypeAttribute2)
									ContentType = ContentTypeAttribute2.ContentType;
								else if (Result is string s2)
									ContentType = s2;
								else
								{
									throw new Exception("Expected an ContentTypeAttribute or string (or null) as a result from calling " +
										DynamicContentTypeAttribute.MethodName + " on editable objects of type " + T.FullName + ".");
								}
							}
						}
						else if (Attr is RegularExpressionAttribute RegularExpressionAttribute)
							ValidationMethod = new RegexValidation(RegularExpressionAttribute.Pattern);
						else if (Attr is RangeAttribute RangeAttribute)
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

					if (Header is null)
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
					object PropertyValue = (PropertyInfo?.GetValue(EditableObject) ?? FieldInfo?.GetValue(EditableObject));

					if (PropertyType == typeof(string[]))
					{
						if (ValidationMethod is null)
							ValidationMethod = new BasicValidation();

						if (Options is null)
						{
							Field = new TextMultiField(Parameters, PropertyName, Header, Required, (string[])PropertyValue,
								null, ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false, ContentType);
						}
						else
						{
							Field = new ListMultiField(Parameters, PropertyName, Header, Required, (string[])PropertyValue,
								Options.ToArray(), ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
						}
					}
					else if (PropertyType.GetTypeInfo().IsEnum)
					{
						if (ValidationMethod is null)
							ValidationMethod = new BasicValidation();

						if (Options is null)
						{
							Options = new List<KeyValuePair<string, string>>();

							foreach (string Option in Enum.GetNames(PropertyType))
								Options.Add(new KeyValuePair<string, string>(Option, Option));
						}

						s = PropertyValue?.ToString();
						if (Nullable && s is null)
							s = string.Empty;

						Field = new ListSingleField(Parameters, PropertyName, Header, Required, new string[] { s },
							Options.ToArray(), ToolTip, null, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
					else if (PropertyType == typeof(bool))
					{
						if (ValidationMethod is null)
							ValidationMethod = new BasicValidation();

						if (Nullable && PropertyValue is null)
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

						s = null;

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

							if (ValidationMethod is null)
								ValidationMethod = new RangeValidation(byte.MinValue.ToString(), byte.MaxValue.ToString());
						}
						else if (PropertyType == typeof(ushort))
						{
							DataType = IntDataType.Instance;

							if (ValidationMethod is null)
								ValidationMethod = new RangeValidation(ushort.MinValue.ToString(), ushort.MaxValue.ToString());
						}
						else if (PropertyType == typeof(uint))
						{
							DataType = LongDataType.Instance;

							if (ValidationMethod is null)
								ValidationMethod = new RangeValidation(uint.MinValue.ToString(), uint.MaxValue.ToString());
						}
						else if (PropertyType == typeof(ulong))
						{
							DataType = IntegerDataType.Instance;

							if (ValidationMethod is null)
								ValidationMethod = new RangeValidation(ulong.MinValue.ToString(), ulong.MaxValue.ToString());
						}
						else if (PropertyType == typeof(DateTime))
						{
							if (DateOnly)
								DataType = DateDataType.Instance;
							else
								DataType = DateTimeDataType.Instance;

							if (PropertyValue is DateTime TP)
							{
								if (TP == DateTime.MinValue || TP == DateTime.MaxValue)
									s = string.Empty;
								else
									s = XML.Encode(TP, DateOnly);
							}
						}
						else if (PropertyType == typeof(decimal))
						{
							DataType = DecimalDataType.Instance;

							if (PropertyValue is decimal d)
								s = CommonTypes.Encode(d);
						}
						else if (PropertyType == typeof(double))
						{
							DataType = DoubleDataType.Instance;

							if (PropertyValue is double d)
								s = CommonTypes.Encode(d);
						}
						else if (PropertyType == typeof(float))
						{
							DataType = DoubleDataType.Instance;    // Use xs:double anyway

							if (PropertyValue is float d)
								s = CommonTypes.Encode(d);
						}
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

						if (ValidationMethod is null)
							ValidationMethod = new BasicValidation();

						if (s is null)
							s = PropertyValue?.ToString();
							
						if (Nullable && s is null)
							s = string.Empty;

						if (Masked)
						{
							Field = new TextPrivateField(Parameters, PropertyName, Header, Required, new string[] { s },
								Options?.ToArray(), ToolTip, DataType, ValidationMethod,
								string.Empty, false, ReadOnly, false);
						}
						else if (Options is null)
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

					if (Field is null)
						continue;

					if (string.IsNullOrEmpty(PageLabel))
					{
						if (DefaultPage is null)
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
						if (!(TextAttributes is null))
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

						if (!(TextAttributes is null))
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
						if (SectionByPageAndSectionLabel is null)
							SectionByPageAndSectionLabel = new Dictionary<string, Section>();

						s = PageLabel + " \xa0 " + SectionLabel;
						if (!SectionByPageAndSectionLabel.TryGetValue(s, out Section Section))
						{
							Section = new Section(Parameters, SectionLabel);
							SectionByPageAndSectionLabel[s] = Section;

							Page.Add(Section);
						}

						if (!(TextAttributes is null))
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

						if (!(TextAttributes is null))
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

				if (!(Pages is null))
				{
					Pages.Sort(OrderPages);

					foreach (Page Page2 in Pages)
						Page2.Sort();
				}

				Parameters.Pages = Pages.ToArray();
			}

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

			foreach (object Item in Type.GetTypeInfo().GetCustomAttributes(typeof(DefaultLanguageAttribute), true))
			{
				if (!(Item is DefaultLanguageAttribute Attr))
					continue;

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
			if (Errors is null)
				Errors = new List<KeyValuePair<string, string>>();

			Errors.Add(new KeyValuePair<string, string>(Field, Error));
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
			Language Language = await ConcentratorServer.GetLanguage(e.Query, DefaultLanguageCode);
			return await SetEditableForm(Language, EditableObject, Form, OnlySetChanged);
		}

		/// <summary>
		/// Sets parameters from a data form in an object.
		/// </summary>
		/// <param name="Language">User language.</param>
		/// <param name="EditableObject">Object whose parameters will be set.</param>
		/// <param name="Form">Data Form.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public static async Task<SetEditableFormResult> SetEditableForm(Language Language, object EditableObject, DataForm Form, bool OnlySetChanged)
		{
			if (EditableObject is IEditableObject Editable)
				return await Editable.SetParameters(Form, Language, OnlySetChanged);
			else
			{
				Type T = EditableObject.GetType();
				List<KeyValuePair<string, string>> Errors = null;
				PropertyInfo PropertyInfo;
				FieldInfo FieldInfo;
				Namespace Namespace = null;
				Namespace ConcentratorNamespace = await Language.GetNamespaceAsync(typeof(ConcentratorServer).Namespace);
				LinkedList<SetRec> ToSet = null;
				ValidationMethod ValidationMethod;
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

				if (Namespace is null)
					Namespace = await Language.CreateNamespaceAsync(T.Namespace);

				if (ConcentratorNamespace is null)
					ConcentratorNamespace = await Language.CreateNamespaceAsync(typeof(ConcentratorServer).Namespace);

				foreach (Field Field in Form.Fields)
				{
					PropertyInfo = T.GetRuntimeProperty(Field.Var);
					FieldInfo = PropertyInfo is null ? T.GetRuntimeField(Field.Var) : null;

					if (PropertyInfo is null && FieldInfo is null)
					{
						if (EditableObject is ICustomFormProperties CustomFormProperties)
						{
							await CustomFormProperties.ValidateCustomProperty(Field);
							if (Field.HasError)
							{
								AddError(ref Errors, Field.Var, Field.Error);
								continue;
							}

							if (ToSet is null)
								ToSet = new LinkedList<SetRec>();

							ToSet.AddLast(new SetRec()
							{
								Field = Field,
								CustomFormProperties = CustomFormProperties,
								Value = Field.ValueStrings
							});
							continue;
						}

						AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(1, "Property not found."));
						continue;
					}

					if (!(PropertyInfo is null) && (!PropertyInfo.CanRead || !PropertyInfo.CanWrite))
					{
						AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(2, "Property not editable."));
						continue;
					}

					NamespaceStr = (PropertyInfo?.DeclaringType ?? FieldInfo.DeclaringType).Namespace;
					if (Namespace is null || NamespaceStr != LastNamespaceStr)
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
						else if (Attr is OptionAttribute OptionAttribute)
						{
							HasOptions = true;
							if (Field.ValueString == OptionAttribute.Option.ToString())
								ValidOption = true;
						}
						else if (Attr is DynamicOptionsAttribute DynamicOptions)
						{
							MethodInfo MI = T.GetRuntimeMethod(DynamicOptions.MethodName, Types.NoTypes)
								?? throw new Exception("Dynamic options method (" + DynamicOptions.MethodName +
									") missing on editable object of type " + T.FullName + ".");

							object Result = MI.Invoke(EditableObject, Types.NoParameters);

							if (!(Result is null))
							{
								if (!(Result is OptionAttribute[] OptionAttributes))
								{
									throw new Exception("Expected an OptionAttribute array (or null) as a result from calling " +
										DynamicOptions.MethodName + " on editable objects of type " + T.FullName + ".");
								}

								HasOptions = true;

								foreach (OptionAttribute OptionAttribute2 in OptionAttributes)
								{
									if (Field.ValueString == OptionAttribute2.Option.ToString())
										ValidOption = true;
								}
							}
						}
						else if (Attr is RegularExpressionAttribute RegularExpressionAttribute)
							ValidationMethod = new RegexValidation(RegularExpressionAttribute.Pattern);
						else if (Attr is RangeAttribute RangeAttribute)
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
							if (ValidationMethod is null)
								ValidationMethod = new BasicValidation();

							ValueToSet = ValueToSet2 = Parsed = Field.ValueStrings;
							DataType = StringDataType.Instance;
						}
						else if (PropertyType.GetTypeInfo().IsEnum)
						{
							if (ValidationMethod is null)
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
							if (ValidationMethod is null)
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

								if (ValidationMethod is null)
									ValidationMethod = new RangeValidation(byte.MinValue.ToString(), byte.MaxValue.ToString());
							}
							else if (PropertyType == typeof(ushort))
							{
								DataType = IntDataType.Instance;

								if (ValidationMethod is null)
									ValidationMethod = new RangeValidation(ushort.MinValue.ToString(), ushort.MaxValue.ToString());
							}
							else if (PropertyType == typeof(uint))
							{
								DataType = LongDataType.Instance;

								if (ValidationMethod is null)
									ValidationMethod = new RangeValidation(uint.MinValue.ToString(), uint.MaxValue.ToString());
							}
							else if (PropertyType == typeof(ulong))
							{
								DataType = IntegerDataType.Instance;

								if (ValidationMethod is null)
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

							if (ValidationMethod is null)
								ValidationMethod = new BasicValidation();

							try
							{
								if (DataType is null)
								{
									ValueToSet = Field.ValueString;
									ValueToSet2 = Types.Instantiate(true, PropertyType, ValueToSet);
									if (ValueToSet2 is null)
									{
										AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(6, "Invalid value."));
										continue;
									}
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

						if (Parsed is null)
							Parsed = new object[] { ValueToSet };

						ValidationMethod.Validate(Field, DataType, Parsed, Field.ValueStrings);
						if (Field.HasError)
						{
							AddError(ref Errors, Field.Var, Field.Error);
							continue;
						}
					}

					if (ToSet is null)
						ToSet = new LinkedList<SetRec>();

					ToSet.AddLast(new SetRec()
					{
						Field = Field,
						PInfo = PropertyInfo,
						FInfo = FieldInfo,
						Value = ValueToSet2
					});
				}

				if (Errors is null)
				{
					SetEditableFormResult Result = new SetEditableFormResult()
					{
						Errors = null,
						Tags = new List<KeyValuePair<string, object>>()
					};

					if (!(ToSet is null))
					{
						foreach (SetRec Rec in ToSet)
						{
							try
							{
								if (OnlySetChanged)
								{
									object Current = Rec.PInfo?.GetValue(EditableObject) ?? Rec.FInfo?.GetValue(EditableObject);

									if (Current is null)
									{
										if (Rec.Value is null)
											continue;
									}
									else if (!(Rec.Value is null) && Current.Equals(Rec.Value))
										continue;
								}

								if (!(Rec.PInfo is null))
								{
									Rec.PInfo.SetValue(EditableObject, Rec.Value);
									Result.AddTag(Rec.PInfo.Name, Rec.Value);
								}
								else if (!(Rec.FInfo is null))
								{
									Rec.FInfo.SetValue(EditableObject, Rec.Value);
									Result.AddTag(Rec.FInfo.Name, Rec.Value);
								}
								else
								{
									await Rec.CustomFormProperties.SetCustomProperty(Rec.Field);
									Result.AddTag(Rec.Field.Var, Rec.Value);
								}
							}
							catch (Exception ex)
							{
								AddError(ref Errors, Rec.Field.Var, ex.Message);
							}
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
		}

		private class SetRec
		{
			public PropertyInfo PInfo;
			public FieldInfo FInfo;
			public Field Field;
			public ICustomFormProperties CustomFormProperties;
			public object Value;
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
				if (F2 is null || !F.Merge(F2))
					F.Exclude = true;
			}
		}

	}
}
