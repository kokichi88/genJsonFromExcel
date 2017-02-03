using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ssar.Combat.HeroStateMachines;
using Checking;
using UnityEngine;

namespace Assets.Scripts.Core.FiniteStateMachine {
	public class StateMachine {
		private State currentState;
		private Stack<string> statesStack = new Stack<string>();
		private Dictionary<string, List<string>> stateTransitionTable = new Dictionary<string, List<string>>();
		private Dictionary<string, State> definedStates = new Dictionary<string, State>();
		private NotNullReference notNullReference = new NotNullReference();
		private List<Action<string, string, bool>> listeners = new List<Action<string, string, bool>>();
		private List<Action<string, string, bool>> preTransitionListeners = new List<Action<string, string, bool>>();

		private bool debug = false;

		public StateMachine(State startState) {
			notNullReference.Check(startState, "startState");

			ChangeStateWithoutTransitionChecking(startState);
		}

		public StateMachine(State startState, bool debug) {
			notNullReference.Check(startState, "startState");

			ChangeStateWithoutTransitionChecking(startState);
			this.debug = debug;
		}

		public void ListenToStateTransition(Action<string, string, bool> action) {
			listeners.Add(action);
		}

		public void ListenToPreStateTransition(Action<string, string, bool> action) {
			preTransitionListeners.Add(action);
		}
//
//		public void UnlistenToPreStateTransition(Action<string, string, bool> action) {
//			preTransitionListeners.Remove(action);
//		}

		public void Update(float dt) {
			currentState.Execute(dt, this);
		}

		public void DefineState(string stateName, State state) {
			notNullReference.Check(stateName, "stateName");
			notNullReference.Check(state, "state");

			definedStates[stateName] = state;
		}

		public void DefineTransition(string stateName, List<string> destinationStates) {
			notNullReference.Check(stateName, "stateName");
			notNullReference.Check(destinationStates, "destinationStates");

			stateTransitionTable[stateName] = destinationStates;
		}

		public void GoBackToPreviousState() {
			if (statesStack.Count <= 1) {
				throw new NotSupportedException("There isn't any state left to go back");
			}

			string previousState = statesStack.Pop();
			string nextState = statesStack.Peek();
			NotifyPreStateTransition(previousState, nextState);

			currentState.Exit();
			currentState = GetStateFromDefinedStatesBy(nextState);
			currentState.Enter(true, previousState);

			NotifyStateTransition(previousState, true);
			DumpStack();
		}

		public State ChangeStateWithHistory(string stateName) {
			CheckStateIsDefined(stateName);

			State state = GetStateFromDefinedStatesBy(stateName);
			ChangeStateWithHistory(state);
			return state;
		}

		public void ReplaceCurrentStateBy(string stateName) {
			CheckStateIsDefined(stateName);
			//			if (currentState.Name() == stateName) return;
			CheckStateTransitionLegal(stateName);
			if (statesStack.Count < 1) {
				throw new NotSupportedException(
					"There is no current state to replace (StateStack is empty)"
				);
			}
			string previousStateName = statesStack.Pop();
			NotifyPreStateTransition(previousStateName, stateName);
			State previousState = GetStateFromDefinedStatesBy(previousStateName);
			previousState.Exit();

			statesStack.Push(stateName);
			State newState = GetStateFromDefinedStatesBy(stateName);
			currentState = newState;
			currentState.Enter(false, previousStateName);

			NotifyStateTransition(previousStateName, false);
			DumpStack();
		}

		public Dictionary<string, State> ShowAllStates() {
			return definedStates;
		}

		private void NotifyPreStateTransition(string previousStateName, string nextStateName) {
			for (int i = 0; i < preTransitionListeners.Count; i++) {
				Action<string, string, bool> action = preTransitionListeners[i];
				if (action != null) {
					action.Invoke(previousStateName, nextStateName, false);
				}
			}
		}

		private void NotifyStateTransition(string previousState, bool resume) {
			foreach (Action<string, string, bool> action in listeners) {
				if (action != null) {
					action.Invoke(previousState, currentState.Name(), resume);
				}
			}
		}

		private void ChangeStateWithHistory(State newState) {
			if (currentState.Name() == newState.Name()) return;
			CheckStateTransitionLegal(newState.Name());

			ChangeStateWithoutTransitionChecking(newState);
		}

		private void ChangeStateWithoutTransitionChecking(State newState) {
			string previousStateName = StateName.UNDEFINED;
			if (statesStack.Count > 0) {
				State previousState = GetStateFromDefinedStatesBy(statesStack.Peek());
				previousState.Exit();
				previousStateName = previousState.Name();
			}
			currentState = newState;
			NotifyPreStateTransition(previousStateName, currentState.Name());

			statesStack.Push(currentState.Name());
			newState.Enter(false, previousStateName);

			NotifyStateTransition(previousStateName, false);
			DumpStack();
		}

		private void CheckStateIsDefined(string stateName) {
			if (!definedStates.ContainsKey(stateName)) {
				throw new NotSupportedException(
					String.Format(
						"State '{0}' is not defined", stateName
					)
				);
			}
		}

		private State GetStateFromDefinedStatesBy(string stateName) {
			return definedStates[stateName];
		}

		private void CheckStateTransitionLegal(string newStateName) {
			if (!IsTransitionLegal(newStateName)) {
				throw new NotSupportedException(
					String.Format(
						"Illegal state transition from {0} to {1}",
						currentState.Name(), newStateName
					)
				);
			}
		}

		private bool IsTransitionLegal(string newStateName) {
			if (!ListOfStatesCanBeTransitFromCurrentState().Contains(newStateName)) {
				return false;
			}

			return true;
		}

		private List<string> ListOfStatesCanBeTransitFromCurrentState() {
			string stateName = currentState.Name();
			if (!stateTransitionTable.ContainsKey(stateName)) {
				throw new KeyNotFoundException(
					String.Format(
						"Transition table of state '{0}' is not defined", stateName
					)
				);
			}
			return stateTransitionTable[stateName];
		}

		public string DumpStack() {
			if (!debug) return string.Empty;

			Stack<string> reversedStack = new Stack<string>();
			foreach (string state in statesStack) {
				reversedStack.Push(state);
			}
			StringBuilder sb = new StringBuilder();
			foreach (string state in reversedStack) {
				sb.Append(state).Append(", ");
			}
//			DLog.Log("STATE STACK: " + sb);
			return sb.ToString();
		}

		public void SetDebug(bool value) {
			debug = value;
		}

		public string GetCurrentStateName()
		{
			return currentState.Name();
		}

		public State FindStateByName(string name) {
			return GetStateFromDefinedStatesBy(name);
		}

		public string[] ShowStatesStack() {
			return statesStack.ToArray();
		}
	}
}
