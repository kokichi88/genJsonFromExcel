using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Core.Scheduling {
	public interface ViewFrameTimeObserver {
		void OnViewFrameTime(float dt);
	}
}
