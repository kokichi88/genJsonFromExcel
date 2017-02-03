using System;
using Core.Utils.Extensions;

namespace Core.Skills {
	public class SkillId {
		private readonly int groupId;
		private readonly SkillCategory category;
		private readonly int minorId;
		private readonly int level;
		private readonly string stringValue;

		public SkillId(int groupId, SkillCategory category, int minorId, int level) {
			this.groupId = groupId;
			this.category = category;
			this.minorId = minorId;
			this.level = level;
			this.stringValue = ToStringValue(groupId, category, minorId, level);
		}

		public SkillId(string stringValue) {
			string[] splits = stringValue.Split('_');
			if (splits.Length != 3)
				throw new Exception(string.Format("Invalid skill config id: '{0}'", stringValue));

			string[] categoryAndMinorId = splits[1].SplitOnDigitOrLetter();
			if(categoryAndMinorId.Length != 2)
				throw new Exception(string.Format("Invalid skill config id: '{0}'", stringValue));

			this.groupId = Convert.ToInt32(splits[0]);
			this.category = (SkillCategory) Enum.Parse(typeof(SkillCategory), categoryAndMinorId[0]);
			this.minorId = Convert.ToInt32(categoryAndMinorId[1]);
			this.level = Convert.ToInt32(splits[2]);
			this.stringValue = stringValue;
		}

		public string StringValue {
			get { return stringValue; }
		}

		public int GroupId {
			get { return groupId; }
		}

		public SkillCategory Category {
			get { return category; }
		}

		public int MinorId {
			get { return minorId; }
		}

		public int Level {
			get { return level; }
		}

		public override bool Equals(object other) {
			try
			{
				if (other.GetType() == typeof(SkillId)) {
					return ((SkillId) other).stringValue.Equals(this.stringValue);
				}
			}
			catch (Exception e)
			{
			}
			

			return base.Equals(other);
		}

		public override int GetHashCode() {
			return stringValue.GetHashCode();
		}

		public override string ToString() {
			return stringValue;
		}

		private string ToStringValue(int groupId, SkillCategory category, int minorId, int level) {
			return string.Format("{0}_{1}{2}_{3}", groupId, category, minorId, level);
		}
	}
}