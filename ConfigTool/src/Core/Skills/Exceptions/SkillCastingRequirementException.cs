using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;

namespace Core.Skills.Exceptions {
	public class SkillCastingRequirementException : Exception {
		public SkillCastingRequirementException() {
		}

		public SkillCastingRequirementException(string message) : base(message) {
		}

		public SkillCastingRequirementException(string message, Exception innerException) : base(message, innerException) {
		}

		protected SkillCastingRequirementException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}
