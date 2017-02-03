using System;
using System.Text;

namespace Core.Commons {
	public class CharacterId {
		private int groupId;
		private int subId;
		private string stringValue;

		public CharacterId(int groupId, int subId) {
			this.groupId = groupId;
			this.subId = subId;
			this.stringValue = ToStringValue(groupId, subId);
		}

		public CharacterId(string stringValue) {
			string[] splits = stringValue.Split('_');
			if (splits.Length != 2)
				throw new Exception(string.Format("Invalid character config id: '{0}'", stringValue));

			this.groupId = Convert.ToInt32(splits[0]);
			this.subId = Convert.ToInt32(splits[1]);
			this.stringValue = stringValue;
		}

		public string ToStringValueWithLevel(int level) {
			StringBuilder sb = new StringBuilder();
			sb.Append(StringValue)
				.Append("_")
				.Append(level);
			return sb.ToString();
		}

		public string StringValue {
			get { return stringValue; }
		}

		public int GroupId {
			get { return groupId; }
		}

		public int SubId {
			get { return subId; }
		}

		public override bool Equals(object other) {
			if (other.GetType() == typeof(CharacterId)) {
				return ((CharacterId) other).stringValue.Equals(this.stringValue);
			}

			return base.Equals(other);
		}

		public override int GetHashCode() {
			return stringValue.GetHashCode();
		}

		public override string ToString() {
			return stringValue;
		}

		private string ToStringValue(int groupId, int subId) {
			return string.Format("{0}_{1}", groupId, subId);
		}
	}
}