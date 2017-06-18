using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Networking.XMPP.Concentrator.Attributes;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Static class managing editable parameters in objects. Editable parameters are defined by using the 
	/// attributes defined in the <see cref="Waher.Networking.XMPP.Concentrator.Attributes"/> namespace.
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
			Namespace Namespace = await Language.GetNamespaceAsync(T.Namespace);
			List<Field> Fields = new List<Field>();
			List<Page> Pages = new List<Page>();
			Dictionary<string, Page> PageByLabel = new Dictionary<string, Page>();
			Dictionary<string, Section> SectionByPageAndSectionLabel = null;
			List<KeyValuePair<string, string>> Options = null;
			string Header;
			string ToolTip;
			string PageLabel;
			string SectionLabel;
			string s;
			int StringId;
			bool Required;
			bool ReadOnly;
			bool Masked;
			bool Alpha;
			bool DateOnly;
			HeaderAttribute HeaderAttribute;
			ToolTipAttribute ToolTipAttribute;
			PageAttribute PageAttribute;
			SectionAttribute SectionAttribute;
			OptionAttribute OptionAttribute;
			TextAttribute TextAttribute;
			RegularExpressionAttribute RegularExpressionAttribute;
			LinkedList<string> TextAttributes;
			RangeAttribute RangeAttribute;
			ValidationMethod ValidationMethod;
			Type PropertyType;
			Field Field;
			Page DefaultPage = null;
			Page Page;
			
			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(T.Namespace);

			foreach (PropertyInfo PI in T.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (!PI.CanRead || !PI.CanWrite)
					continue;

				Header = ToolTip = PageLabel = SectionLabel = null;
				TextAttributes = null;
				ValidationMethod = null;
				Required = ReadOnly = Masked = Alpha = DateOnly = false;

				foreach (Attribute Attr in PI.GetCustomAttributes())
				{
					if ((HeaderAttribute = Attr as HeaderAttribute) != null)
					{
						Header = HeaderAttribute.Header;
						StringId = HeaderAttribute.StringId;
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
							TextAttributes = new LinkedList<string>();

						StringId = TextAttribute.StringId;
						if (StringId > 0)
							TextAttributes.AddLast(await Namespace.GetStringAsync(StringId, TextAttribute.Label));
						else
							TextAttributes.AddLast(TextAttribute.Label);
					}
					else if ((OptionAttribute = Attr as OptionAttribute) != null)
					{
						if (Options == null)
							Options = new List<KeyValuePair<string, string>>();

						StringId = OptionAttribute.StringId;
						if (StringId > 0)
						{
							Options.Add(new KeyValuePair<string, string>(OptionAttribute.Option.ToString(),
								await Namespace.GetStringAsync(StringId, TextAttribute.Label)));
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

				PropertyType = PI.PropertyType;
				Field = null;

				if (PropertyType == typeof(string[]))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					if (Options == null)
					{
						Field = new TextMultiField(Parameters, PI.Name, Header, Required, (string[])PI.GetValue(EditableObject),
							null, ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
					else
					{
						Field = new ListMultiField(Parameters, PI.Name, Header, Required, (string[])PI.GetValue(EditableObject),
							Options.ToArray(), ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
				}
				else if (PropertyType == typeof(Enum))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					if (Options == null)
					{
						Options = new List<KeyValuePair<string, string>>();

						foreach (string Option in Enum.GetNames(PropertyType))
							Options.Add(new KeyValuePair<string, string>(Option, Option));
					}

					Field = new ListSingleField(Parameters, PI.Name, Header, Required, new string[] { PI.GetValue(EditableObject).ToString() },
						Options.ToArray(), ToolTip, null, ValidationMethod, string.Empty, false, ReadOnly, false);
				}
				else if (PropertyType == typeof(bool))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					Field = new BooleanField(Parameters, PI.Name, Header, Required,
						new string[] { CommonTypes.Encode((bool)PI.GetValue(EditableObject)) },
						Options?.ToArray(), ToolTip, BooleanDataType.Instance, ValidationMethod,
						string.Empty, false, ReadOnly, false);
				}
				else
				{
					DataType DataType;

					if (PropertyType == typeof(string))
						DataType = StringDataType.Instance;
					else if (PropertyType == typeof(byte))
						DataType = ByteDataType.Instance;
					else if (PropertyType == typeof(short))
						DataType = ShortDataType.Instance;
					else if (PropertyType == typeof(int))
						DataType = IntDataType.Instance;
					else if (PropertyType == typeof(long))
						DataType = LongDataType.Instance;
					else if (PropertyType == typeof(sbyte))
					{
						DataType = ShortDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(sbyte.MinValue.ToString(), sbyte.MaxValue.ToString());
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

					if (Masked)
					{
						Field = new TextPrivateField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(EditableObject) },
							Options?.ToArray(), ToolTip, StringDataType.Instance, ValidationMethod,
							string.Empty, false, ReadOnly, false);
					}
					else if (Options == null)
					{
						Field = new TextSingleField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(EditableObject) },
							null, ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
					else
					{
						Field = new ListSingleField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(EditableObject) },
							Options.ToArray(), ToolTip, StringDataType.Instance, ValidationMethod, string.Empty, false, ReadOnly, false);
					}
				}

				if (Field == null)
					continue;

				Fields.Add(Field);

				if (string.IsNullOrEmpty(PageLabel))
				{
					if (DefaultPage == null)
					{
						DefaultPage = new Page(Parameters, string.Empty);
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
						Page = new Page(Parameters, PageLabel);
						Pages.Add(Page);
						PageByLabel[PageLabel] = Page;
					}
				}

				if (string.IsNullOrEmpty(SectionLabel))
				{
					if (TextAttributes != null)
					{
						foreach (string Text in TextAttributes)
							Page.Add(new TextElement(Parameters, Text));
					}

					Page.Add(new FieldReference(Parameters, Field.Var));
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
						foreach (string Text in TextAttributes)
							Section.Add(new TextElement(Parameters, Text));
					}

					Section.Add(new FieldReference(Parameters, Field.Var));
				}
			}

			Parameters.Title = Title;
			Parameters.Fields = Fields.ToArray();
			Parameters.Pages = Pages.ToArray();

			return Parameters;
		}

		private static string GetDefaultLanguageCode(Type Type)
		{
			string DefaultLanguageCode = null;

			foreach (DefaultLanguageAttribute Attr in Type.GetCustomAttributes(typeof(DefaultLanguageAttribute), true))
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
		/// Sets parameters from a data form in an object.
		/// </summary>
		/// <param name="e">IQ Event Arguments describing the request.</param>
		/// <param name="EditableObject">Object whose parameters will be set.</param>
		/// <param name="Form">Data Form.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public static async Task<KeyValuePair<string, string>[]> SetEditableForm(IqEventArgs e, object EditableObject, DataForm Form)
		{
			Type T = EditableObject.GetType();
			string DefaultLanguageCode = GetDefaultLanguageCode(T);
			List<KeyValuePair<string, string>> Errors = null;
			PropertyInfo PI;
			Language Language = await ConcentratorServer.GetLanguage(e.Query, DefaultLanguageCode);
			Namespace Namespace = await Language.GetNamespaceAsync(T.Namespace);
			Namespace ConcentratorNamespace = await Language.GetNamespaceAsync(typeof(ConcentratorServer).Namespace);
			LinkedList<KeyValuePair<PropertyInfo, object>> ToSet = null;
			ValidationMethod ValidationMethod;
			OptionAttribute OptionAttribute;
			RegularExpressionAttribute RegularExpressionAttribute;
			RangeAttribute RangeAttribute;
			DataType DataType;
			Type PropertyType;
			string Header;
			object ValueToSet;
			object[] Parsed;
			bool ReadOnly;
			bool Alpha;
			bool DateOnly;
			bool HasHeader;
			bool HasOptions;
			bool ValidOption;

			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(T.Namespace);

			if (ConcentratorNamespace == null)
				ConcentratorNamespace = await Language.CreateNamespaceAsync(typeof(ConcentratorServer).Namespace);

			foreach (Field Field in Form.Fields)
			{
				PI = T.GetProperty(Field.Var, BindingFlags.Public | BindingFlags.Instance);
				if (PI == null)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(1, "Property not found."));
					continue;
				}

				if (!PI.CanRead || !PI.CanWrite)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(2, "Property not editable."));
					continue;
				}

				Header = null;
				ValidationMethod = null;
				ReadOnly = Alpha = DateOnly = HasHeader = HasOptions = ValidOption = false;

				foreach (Attribute Attr in PI.GetCustomAttributes())
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

				if (Header == null)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(2, "Property not editable."));
					continue;
				}

				if (ReadOnly)
				{
					if (Field.ValueString != PI.GetValue(EditableObject).ToString())
						AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(3, "Property is read-only."));

					continue;
				}

				if (HasOptions && !ValidOption)
				{
					AddError(ref Errors, Field.Var, await ConcentratorNamespace.GetStringAsync(4, "Select a valid option."));
					continue;
				}

				PropertyType = PI.PropertyType;
				ValueToSet = null;
				Parsed = null;
				DataType = null;

				if (PropertyType == typeof(string[]))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					ValueToSet = Parsed = Field.ValueStrings;
					DataType = StringDataType.Instance;
				}
				else if (PropertyType == typeof(Enum))
				{
					if (ValidationMethod == null)
						ValidationMethod = new BasicValidation();

					try
					{
						ValueToSet = Enum.Parse(PropertyType, Field.ValueString);
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
				}
				else
				{
					if (PropertyType == typeof(string))
						DataType = StringDataType.Instance;
					else if (PropertyType == typeof(byte))
						DataType = ByteDataType.Instance;
					else if (PropertyType == typeof(short))
						DataType = ShortDataType.Instance;
					else if (PropertyType == typeof(int))
						DataType = IntDataType.Instance;
					else if (PropertyType == typeof(long))
						DataType = LongDataType.Instance;
					else if (PropertyType == typeof(sbyte))
					{
						DataType = ShortDataType.Instance;

						if (ValidationMethod == null)
							ValidationMethod = new RangeValidation(sbyte.MinValue.ToString(), sbyte.MaxValue.ToString());
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
						ValueToSet = DataType.Parse(Field.ValueString);
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

				if (ToSet == null)
					ToSet = new LinkedList<KeyValuePair<PropertyInfo, object>>();

				ToSet.AddLast(new KeyValuePair<PropertyInfo, object>(PI, ValueToSet));
			}

			if (Errors == null)
			{
				foreach (KeyValuePair<PropertyInfo, object> P in ToSet)
				{
					try
					{
						P.Key.SetValue(EditableObject, P.Value);
					}
					catch (Exception ex)
					{
						AddError(ref Errors, P.Key.Name, ex.Message);
					}
				}
			}

			if (Errors == null)
				return null;
			else
				return Errors.ToArray();
		}

		/// <summary>
		/// Sets parameters from a data form in a set of objects.
		/// </summary>
		/// <param name="e">IQ Event Arguments describing the request.</param>
		/// <param name="EditableObjects">Objects whose parameters will be set.</param>
		/// <param name="Form">Data Form.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public static async Task<KeyValuePair<string, string>[]> SetEditableForm(IqEventArgs e, IEnumerable<object> EditableObjects, DataForm Form)
		{
			KeyValuePair<string, string>[] Result;

			foreach (Object EditableObject in EditableObjects)
			{
				Result = await SetEditableForm(e, EditableObject, Form);
				if (Result != null)
					return Result;
			}

			return null;
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
