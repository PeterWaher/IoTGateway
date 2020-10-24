using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User general activities enumeration.
	/// </summary>
	public enum UserGeneralActivities
	{
		/// <summary>
		/// Doing chores
		/// </summary>
		doing_chores,

		/// <summary>
		/// Dinking
		/// </summary>
		drinking,

		/// <summary>
		/// Eating
		/// </summary>
		eating,

		/// <summary>
		/// Exercising
		/// </summary>
		exercising,

		/// <summary>
		/// Grooming
		/// </summary>
		grooming,

		/// <summary>
		/// Having Appointment
		/// </summary>
		having_appointment,

		/// <summary>
		/// Inactive
		/// </summary>
		inactive,

		/// <summary>
		/// Relaxing
		/// </summary>
		relaxing,

		/// <summary>
		/// Talking
		/// </summary>
		talking,

		/// <summary>
		/// Traveling
		/// </summary>
		traveling,

		/// <summary>
		/// Undefined
		/// </summary>
		undefined,

		/// <summary>
		/// Working
		/// </summary>
		working
	}

	/// <summary>
	/// User specific activities enumeration.
	/// </summary>
	public enum UserSpecificActivities
	{
		/// <summary>
		/// At the spa
		/// </summary>
		at_the_spa,

		/// <summary>
		/// Brrushing teeth
		/// </summary>
		brushing_teeth,

		/// <summary>
		/// Buying groceries
		/// </summary>
		buying_groceries,

		/// <summary>
		/// Cleaning
		/// </summary>
		cleaning,

		/// <summary>
		/// Coding
		/// </summary>
		coding,

		/// <summary>
		/// Commuting
		/// </summary>
		commuting,

		/// <summary>
		/// Cooking
		/// </summary>
		cooking,

		/// <summary>
		/// Cycling
		/// </summary>
		cycling,

		/// <summary>
		/// Dancing
		/// </summary>
		dancing,

		/// <summary>
		/// Day off
		/// </summary>
		day_off,

		/// <summary>
		/// Doing maintenance
		/// </summary>
		doing_maintenance,

		/// <summary>
		/// Doing the dishes
		/// </summary>
		doing_the_dishes,

		/// <summary>
		/// Doing the laundry
		/// </summary>
		doing_the_laundry,

		/// <summary>
		/// Driving
		/// </summary>
		driving,

		/// <summary>
		/// Fishing
		/// </summary>
		fishing,

		/// <summary>
		/// Gaming
		/// </summary>
		gaming,

		/// <summary>
		/// Gardening
		/// </summary>
		gardening,

		/// <summary>
		/// Getting a haircut
		/// </summary>
		getting_a_haircut,

		/// <summary>
		/// Going out
		/// </summary>
		going_out,

		/// <summary>
		/// Hanging out
		/// </summary>
		hanging_out,

		/// <summary>
		/// Hving a beer
		/// </summary>
		having_a_beer,

		/// <summary>
		/// Having a snack
		/// </summary>
		having_a_snack,

		/// <summary>
		/// Having breakfast
		/// </summary>
		having_breakfast,

		/// <summary>
		/// Having coffee
		/// </summary>
		having_coffee,

		/// <summary>
		/// Having dinner
		/// </summary>
		having_dinner,

		/// <summary>
		/// Having lunch
		/// </summary>
		having_lunch,

		/// <summary>
		/// Having tea
		/// </summary>
		having_tea,

		/// <summary>
		/// Hiding
		/// </summary>
		hiding,

		/// <summary>
		/// Hiking
		/// </summary>
		hiking,

		/// <summary>
		/// In a car
		/// </summary>
		in_a_car,

		/// <summary>
		/// In a meeting
		/// </summary>
		in_a_meeting,

		/// <summary>
		/// In real life
		/// </summary>
		in_real_life,

		/// <summary>
		/// Jogging
		/// </summary>
		jogging,

		/// <summary>
		/// On a bus
		/// </summary>
		on_a_bus,

		/// <summary>
		/// On a plane
		/// </summary>
		on_a_plane,

		/// <summary>
		/// On a train
		/// </summary>
		on_a_train,

		/// <summary>
		/// On a trip
		/// </summary>
		on_a_trip,

		/// <summary>
		/// On the phone
		/// </summary>
		on_the_phone,

		/// <summary>
		/// On vacation
		/// </summary>
		on_vacation,

		/// <summary>
		/// On video phone
		/// </summary>
		on_video_phone,

		/// <summary>
		/// Other
		/// </summary>
		other,

		/// <summary>
		/// Partying
		/// </summary>
		partying,

		/// <summary>
		/// Playing sports
		/// </summary>
		playing_sports,

		/// <summary>
		/// Praying
		/// </summary>
		praying,

		/// <summary>
		/// Reading
		/// </summary>
		reading,

		/// <summary>
		/// Rehearsing
		/// </summary>
		rehearsing,

		/// <summary>
		/// Running
		/// </summary>
		running,

		/// <summary>
		/// Running an errand
		/// </summary>
		running_an_errand,

		/// <summary>
		/// Scheduled holiday
		/// </summary>
		scheduled_holiday,

		/// <summary>
		/// Shaving
		/// </summary>
		shaving,

		/// <summary>
		/// Shopping
		/// </summary>
		shopping,

		/// <summary>
		/// Skiing
		/// </summary>
		skiing,

		/// <summary>
		/// Sleeping
		/// </summary>
		sleeping,

		/// <summary>
		/// Smoking
		/// </summary>
		smoking,

		/// <summary>
		/// Socializing
		/// </summary>
		socializing,

		/// <summary>
		/// Studying
		/// </summary>
		studying,

		/// <summary>
		/// Sunbathing
		/// </summary>
		sunbathing,

		/// <summary>
		/// Swimming
		/// </summary>
		swimming,

		/// <summary>
		/// Taking a bath
		/// </summary>
		taking_a_bath,

		/// <summary>
		/// Taking a shower
		/// </summary>
		taking_a_shower,

		/// <summary>
		/// Thinking
		/// </summary>
		thinking,

		/// <summary>
		/// Walking
		/// </summary>
		walking,

		/// <summary>
		/// Walking the dog
		/// </summary>
		walking_the_dog,

		/// <summary>
		/// Watching a movie
		/// </summary>
		watching_a_movie,

		/// <summary>
		/// Watching TV
		/// </summary>
		watching_tv,

		/// <summary>
		/// Working out
		/// </summary>
		working_out,

		/// <summary>
		/// Writing
		/// </summary>
		writing
	}

	/// <summary>
	/// User Activity event, as defined in XEP-0108:
	/// https://xmpp.org/extensions/xep-0108.html
	/// </summary>
	public class UserActivity : PersonalEvent
	{
		private UserGeneralActivities? generalActivity = null;
		private UserSpecificActivities? specificActivity = null;
		private string text = null;

		/// <summary>
		/// User Activity event, as defined in XEP-0108:
		/// https://xmpp.org/extensions/xep-0108.html
		/// </summary>
		public UserActivity()
		{
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "activity";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => "http://jabber.org/protocol/activity";

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<activity xmlns='");
				Xml.Append(this.Namespace);
				Xml.Append("'>");

				if (this.generalActivity.HasValue)
				{
					Xml.Append('<');
					Xml.Append(this.generalActivity.Value.ToString());

					if (this.specificActivity.HasValue)
					{
						Xml.Append("><");
						Xml.Append(this.specificActivity.Value.ToString());
						Xml.Append("/></");
						Xml.Append(this.generalActivity.Value.ToString());
						Xml.Append('>');
					}
					else
						Xml.Append("/>");
				}

				if (!(this.text is null))
				{
					Xml.Append("<text>");
					Xml.Append(XML.Encode(this.text));
					Xml.Append("</text>");
				}

				Xml.Append("</activity>");

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
			UserActivity Result = new UserActivity();

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
							if (Enum.TryParse<UserGeneralActivities>(E2.LocalName, out UserGeneralActivities GeneralActivity))
								Result.generalActivity = GeneralActivity;
							else if (Result.generalActivity.HasValue)
								break;
							else
								Result.generalActivity = UserGeneralActivities.undefined;

							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3)
								{
									if (Enum.TryParse<UserSpecificActivities>(E3.LocalName, out UserSpecificActivities SpecificActivity))
										Result.specificActivity = SpecificActivity;
									else if (!Result.specificActivity.HasValue)
										Result.specificActivity = UserSpecificActivities.other;
								}
							}
							break;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// User general activity
		/// </summary>
		public UserGeneralActivities? GeneralActivity
		{
			get => this.generalActivity;
			set => this.generalActivity = value;
		}

		/// <summary>
		/// User specific activity
		/// </summary>
		public UserSpecificActivities? SpecificActivity
		{
			get => this.specificActivity;
			set => this.specificActivity = value;
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
