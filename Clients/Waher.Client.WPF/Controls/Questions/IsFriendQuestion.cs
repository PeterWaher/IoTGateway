using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Client.WPF.Controls.Questions
{
	public class IsFriendQuestion : Question
	{
		public IsFriendQuestion()
		{
		}

		public override string QuestionString => "Are friends?";
	}
}
