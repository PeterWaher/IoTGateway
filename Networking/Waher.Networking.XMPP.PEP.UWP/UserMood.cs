using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User mood enumeration.
	/// </summary>
	public enum UserMoods
	{
		/// <summary>
		/// Impressed with fear or apprehension; in fear; apprehensive.
		/// </summary>
		afraid,

		/// <summary>
		/// Astonished; confounded with fear, surprise or wonder.
		/// </summary>
		amazed,

		/// <summary>
		/// Displaying or feeling anger, i.e., a strong feeling of displeasure, hostility or antagonism towards someone or something, usually combined with an urge to harm.
		/// </summary>
		angry,

		/// <summary>
		/// Inclined to love; having a propensity to love, or to sexual enjoyment; loving, fond, affectionate, passionate, lustful, sexual, etc.
		/// </summary>
		amorous,

		/// <summary>
		/// To be disturbed or irritated, especially by continued or repeated acts.
		/// </summary>
		annoyed,

		/// <summary>
		/// Full of anxiety or disquietude; greatly concerned or solicitous, esp. respecting something future or unknown; being in painful suspense.
		/// </summary>
		anxious,

		/// <summary>
		/// To be stimulated in one's feelings, especially to be sexually stimulated.
		/// </summary>
		aroused,

		/// <summary>
		/// Feeling shame or guilt.
		/// </summary>
		ashamed,

		/// <summary>
		/// Suffering from boredom; uninterested, without attention.
		/// </summary>
		bored,

		/// <summary>
		/// Strong in the face of fear; courageous. 
		/// </summary>
		brave,

		/// <summary>
		/// Peaceful, quiet.
		/// </summary>
		calm,

		/// <summary>
		/// Taking care or caution; tentative.
		/// </summary>
		cautious,

		/// <summary>
		/// Feeling the sensation of coldness, especially to the point of discomfort.
		/// </summary>
		cold,

		/// <summary>
		/// Feeling very sure of or positive about something, especially about one's own capabilities.
		/// </summary>
		confident,

		/// <summary>
		/// Chaotic, jumbled or muddled.
		/// </summary>
		confused,

		/// <summary>
		/// Feeling introspective or thoughtful.
		/// </summary>
		contemplative,

		/// <summary>
		/// Pleased at the satisfaction of a want or desire; satisfied.
		/// </summary>
		contented,

		/// <summary>
		/// Grouchy, irritable; easily upset.
		/// </summary>
		cranky,

		/// <summary>
		/// Feeling out of control; feeling overly excited or enthusiastic.
		/// </summary>
		crazy,

		/// <summary>
		/// Feeling original, expressive, or imaginative.
		/// </summary>
		creative,

		/// <summary>
		/// Inquisitive; tending to ask questions, investigate, or explore.
		/// </summary>
		curious,

		/// <summary>
		/// Feeling sad and dispirited.
		/// </summary>
		dejected,

		/// <summary>
		/// Severely despondent and unhappy.
		/// </summary>
		depressed,

		/// <summary>
		/// Defeated of expectation or hope; let down.
		/// </summary>
		disappointed,

		/// <summary>
		/// Filled with disgust; irritated and out of patience.
		/// </summary>
		disgusted,

		/// <summary>
		/// Feeling a sudden or complete loss of courage in the face of trouble or danger.
		/// </summary>
		dismayed,

		/// <summary>
		/// Having one's attention diverted; preoccupied.
		/// </summary>
		distracted,

		/// <summary>
		/// Having a feeling of shameful discomfort.
		/// </summary>
		embarrassed,

		/// <summary>
		/// Feeling pain by the excellence or good fortune of another.
		/// </summary>
		envious,

		/// <summary>
		/// Having great enthusiasm.
		/// </summary>
		excited,

		/// <summary>
		/// In the mood for flirting.
		/// </summary>
		flirtatious,

		/// <summary>
		/// Suffering from frustration; dissatisfied, agitated, or discontented because one is unable to perform an action or fulfill a desire.
		/// </summary>
		frustrated,
		/// <summary>
		/// Feeling appreciation or thanks.
		/// </summary>
		grateful,

		/// <summary>
		/// Feeling very sad about something, especially something lost; mournful; sorrowful.
		/// </summary>
		grieving,

		/// <summary>
		/// Unhappy and irritable.
		/// </summary>
		grumpy,

		/// <summary>
		/// Feeling responsible for wrongdoing; feeling blameworthy.
		/// </summary>
		guilty,

		/// <summary>
		/// Experiencing the effect of favourable fortune; having the feeling arising from the consciousness of well-being or of enjoyment; enjoying good of any kind, as peace, tranquillity, comfort; contented; joyous.
		/// </summary>
		happy,

		/// <summary>
		/// Having a positive feeling, belief, or expectation that something wished for can or will happen.
		/// </summary>
		hopeful,

		/// <summary>
		/// Feeling the sensation of heat, especially to the point of discomfort.
		/// </summary>
		hot,

		/// <summary>
		/// Having or showing a modest or low estimate of one's own importance; feeling lowered in dignity or importance.
		/// </summary>
		humbled,

		/// <summary>
		/// Feeling deprived of dignity or self-respect.
		/// </summary>
		humiliated,

		/// <summary>
		/// Having a physical need for food.
		/// </summary>
		hungry,

		/// <summary>
		/// Wounded, injured, or pained, whether physically or emotionally.
		/// </summary>
		hurt,

		/// <summary>
		/// Favourably affected by something or someone.
		/// </summary>
		impressed,

		/// <summary>
		/// Feeling amazement at something or someone; or feeling a combination of fear and reverence.
		/// </summary>
		in_awe,

		/// <summary>
		/// Feeling strong affection, care, liking, or attraction.
		/// </summary>
		in_love,

		/// <summary>
		/// Showing anger or indignation, especially at something unjust or wrong.
		/// </summary>
		indignant,

		/// <summary>
		/// Showing great attention to something or someone; having or showing interest.
		/// </summary>
		interested,

		/// <summary>
		/// Under the influence of alcohol; drunk.
		/// </summary>
		intoxicated,

		/// <summary>
		/// Feeling as if one cannot be defeated, overcome or denied.
		/// </summary>
		invincible,

		/// <summary>
		/// Fearful of being replaced in position or affection.
		/// </summary>
		jealous,

		/// <summary>
		/// Feeling isolated, empty, or abandoned.
		/// </summary>
		lonely,

		/// <summary>
		/// Unable to find one's way, either physically or emotionally.
		/// </summary>
		lost,

		/// <summary>
		/// Feeling as if one will be favored by luck.
		/// </summary>
		lucky,

		/// <summary>
		/// Causing or intending to cause intentional harm; bearing ill will towards another; cruel; malicious.
		/// </summary>
		mean,

		/// <summary>
		/// Given to sudden or frequent changes of mind or feeling; temperamental.
		/// </summary>
		moody,

		/// <summary>
		/// Easily agitated or alarmed; apprehensive or anxious.
		/// </summary>
		nervous,

		/// <summary>
		/// Not having a strong mood or emotional state.
		/// </summary>
		neutral,

		/// <summary>
		/// Feeling emotionally hurt, displeased, or insulted. 
		/// </summary>
		offended,

		/// <summary>
		/// Feeling resentful anger caused by an extremely violent or vicious attack, or by an offensive, immoral, or indecent act.
		/// </summary>
		outraged,

		/// <summary>
		/// Interested in play; fun, recreational, unserious, lighthearted; joking, silly.
		/// </summary>
		playful,

		/// <summary>
		/// Feeling a sense of one's own worth or accomplishment.
		/// </summary>
		proud,

		/// <summary>
		/// Having an easy-going mood; not stressed; calm.
		/// </summary>
		relaxed,

		/// <summary>
		/// Feeling uplifted because of the removal of stress or discomfort.
		/// </summary>
		relieved,

		/// <summary>
		/// Feeling regret or sadness for doing something wrong.
		/// </summary>
		remorseful,

		/// <summary>
		/// Without rest; unable to be still or quiet; uneasy; continually moving.
		/// </summary>
		restless,

		/// <summary>
		/// Feeling sorrow; sorrowful, mournful.
		/// </summary>
		sad,

		/// <summary>
		/// Mocking and ironical.
		/// </summary>
		sarcastic,

		/// <summary>
		/// Pleased at the fulfillment of a need or desire.
		/// </summary>
		satisfied,

		/// <summary>
		/// Without humor or expression of happiness; grave in manner or disposition; earnest; thoughtful; solemn.
		/// </summary>
		serious,

		/// <summary>
		/// Surprised, startled, confused, or taken aback.
		/// </summary>
		shocked,

		/// <summary>
		/// Feeling easily frightened or scared; timid; reserved or coy.
		/// </summary>
		shy,

		/// <summary>
		/// Feeling in poor health; ill.
		/// </summary>
		sick,

		/// <summary>
		/// Feeling the need for sleep.
		/// </summary>
		sleepy,

		/// <summary>
		/// Acting without planning; natural; impulsive.
		/// </summary>
		spontaneous,

		/// <summary>
		/// Suffering emotional pressure.
		/// </summary>
		stressed,

		/// <summary>
		/// Capable of producing great physical force; or, emotionally forceful, able, determined, unyielding.
		/// </summary>
		strong,

		/// <summary>
		/// Experiencing a feeling caused by something unexpected.
		/// </summary>
		surprised,

		/// <summary>
		/// Showing appreciation or gratitude.
		/// </summary>
		thankful,

		/// <summary>
		/// Feeling the need to drink.
		/// </summary>
		thirsty,

		/// <summary>
		/// In need of rest or sleep.
		/// </summary>
		tired,

		/// <summary>
		/// Feeling an emotion not defined by this enumeration.
		/// </summary>
		undefined,

		/// <summary>
		/// Lacking in force or ability, either physical or emotional.
		/// </summary>
		weak,

		/// <summary>
		/// Thinking about unpleasant things that have happened or that might happen; feeling afraid and unhappy.
		/// </summary>
		worried
	}

	/// <summary>
	/// User Mood event, as defined in XEP-0107:
	/// https://xmpp.org/extensions/xep-0107.html
	/// </summary>
	public class UserMood : PersonalEvent
	{
		private UserMoods? mood = null;
		private string text = null;

		/// <summary>
		/// User Mood event, as defined in XEP-0107:
		/// https://xmpp.org/extensions/xep-0107.html
		/// </summary>
		public UserMood()
		{
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "mood";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => "http://jabber.org/protocol/mood";

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<mood xmlns='");
				Xml.Append(this.Namespace);
				Xml.Append("'>");

				if (this.mood.HasValue)
				{
					Xml.Append('<');
					Xml.Append(this.mood.Value.ToString());
					Xml.Append("/>");
				}

				if (!(this.text is null))
				{
					Xml.Append("<text>");
					Xml.Append(XML.Encode(this.text));
					Xml.Append("</text>");
				}

				Xml.Append("</mood>");

				return Xml.ToString();
			}
		}

		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public override IPersonalEvent Parse(XmlElement E)
		{
			UserMood Result = new UserMood();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "text":
							Result.text = E2.InnerText; 
							break;

						default:
							if (Enum.TryParse<UserMoods>(E2.LocalName, out UserMoods Mood))
								Result.mood = Mood;
							else if (!Result.mood.HasValue)
								Result.mood = UserMoods.undefined;
							break;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// User mood
		/// </summary>
		public UserMoods? Mood
		{
			get => this.mood;
			set => this.mood = value;
		}

		/// <summary>
		/// Custom text provided by the use.
		/// </summary>
		public string Text
		{
			get => this.text;
			set => this.text = value;
		}

	}
}
