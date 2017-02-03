using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Core.FiniteStateMachine {
	public interface State {
		string Name();

		void Execute(float dt, StateMachine stateMachine);

		void Enter(bool resume, string fromStateName);

		void Exit();
	}
}
