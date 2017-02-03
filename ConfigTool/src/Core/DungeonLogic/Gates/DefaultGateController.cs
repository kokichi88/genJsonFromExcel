using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Combat.Gate;
using MEC;
using UnityEngine;
using Utils;

namespace Core.DungeonLogic.Gates {
	public class DefaultGateController : GateController 
	{
		private const int DURATION = 0;
		private const string CLOSE = "Effect/Common/gateEffect_Close";
		private const string OPEN = "Effect/Common/gateEffect_Open";
		private const string OPEN_PHASE_2 = "Effect/Common/gateEffect_Open_Phase2";

		private readonly int gateId;
		private Environment.Environment environment;

		private GameObject gate;
		private State state = State.Closed;
		private float elapsed;
		private GameObject open;
		private GameObject openPhase2;
		private GameObject close;

		#region Init
		
		public DefaultGateController(int gateId, Environment.Environment environment) {
			this.gateId = gateId;
			this.environment = environment;
			
			Init();
		}

		public void SetCookies(IEnumerable<string> cookies) {
		}

		private void Init()
		{
			gate = environment.GetGateById(gateId);
			BaseGateBehaviour gateBehaviour = gate.GetComponent<BaseGateBehaviour>();
			gateBehaviour?.SetEnvironment(environment);
		}
		
		#endregion

		protected override void OnOpen() {
			// state = State.Opening;
			state = State.WaitToOpen;
		}

		protected override void OnClose() {
			// state = State.Closing;
			state = State.WaitToClose;
		}

		public override bool IsOpened() {
			return state == State.Opened;
		}

		public override bool IsClosed() {
			return state == State.Closed;
		}

		public override bool IsOpening() {
			return state == State.Opening;
		}

		public override bool IsClosing() {
			return state == State.Closing;
		}

		public override void Update(float dt)
		{
			if (IsWaitToOpen() || IsWaitToClose())
			{
				elapsed += dt;
				if (elapsed >= DURATION)
				{
					elapsed = 0;
					if (IsWaitToOpen())
					{
						ProcessOpen();
					}

					if (IsWaitToClose())
					{
						ProcessClose();
					}
				}
			}
		}

		private void SetState(State state)
		{
			this.state = state;
		}

		private bool IsWaitToOpen()
		{
			return state == State.WaitToOpen;
		}
		
		private bool IsWaitToClose()
		{
			return state == State.WaitToClose;
		}

		private void ProcessOpen()
		{
			SetState(State.Opening);
			// GameObject gate = environment.GetGateById(gateId);
			BaseGateBehaviour gateBehaviour = gate.GetComponent<BaseGateBehaviour>();

			if (gateBehaviour == null)
			{
				SetState(State.Opened);
				PlayFxOpen(gate);
			}
			else
			{
				gateBehaviour.Open(() =>
				{
					SetState(State.Opened);
				});
			}
		}

		private void ProcessClose()
		{
			SetState(State.Closing);
			// GameObject gate = environment.GetGateById(gateId);
			BaseGateBehaviour gateBehaviour = gate.GetComponent<BaseGateBehaviour>();

			if (gateBehaviour == null)
			{
				SetState(State.Closed);
				PlayFxClose(gate);
			}
			else
			{
				gateBehaviour.Close(() =>
				{
					SetState(State.Closed);
				});
			}
		}

		private void PlayFxOpen(GameObject gate)
		{
			gate.SetActive(false);

			GameObject openPrefab = Resources.Load<GameObject>(OPEN);
			open = GameObject.Instantiate(openPrefab);
			open.transform.position = gate.transform.position;
			GameObject openPhase2Prefab = Resources.Load<GameObject>(OPEN_PHASE_2);
			openPhase2 = GameObject.Instantiate(openPhase2Prefab);
			openPhase2.transform.position = gate.transform.position;
			float length = open.GetComponent<GateOpeningOrClosingDuration>().duration;
			Timing.RunCoroutine(WaitThen(
				length,
				() => { open.active = false; }
			));
		}

		private IEnumerator<float> WaitThen(float waitTime, Action action) {
			yield return Timing.WaitForSeconds(waitTime);
			action();
		}

		private void PlayFxClose(GameObject gate) {
			gate.SetActive(true);
			gate.GetComponent<RotateToFaceCamera>().enabled = false;
			Vector3 euler = gate.transform.rotation.eulerAngles;
			gate.transform.eulerAngles = new Vector3(euler.x, -euler.y, euler.z);
			gate.transform.localScale = new Vector3(-1, gate.transform.localScale.y, gate.transform.localScale.z);
			DisableGateVisual(gate);

			GameObject closePrefab = Resources.Load<GameObject>(CLOSE);
			close = GameObject.Instantiate(closePrefab);
			close.transform.position = gate.transform.position;
			GateOpeningOrClosingDuration durationConfig = close.GetComponent<GateOpeningOrClosingDuration>();
			Timing.RunCoroutine(WaitThen(
				durationConfig.waitTimeToShowIdleGate,
				() => { EnableGateVisual(gate); }
			));
			Timing.RunCoroutine(WaitThen(
				durationConfig.duration,
				() => {
					close.active = false;
					FadeoutOpenPhase2();
				}
			));
		}

		private static void EnableGateVisual(GameObject gate) {
			for (int kIndex = 0; kIndex < gate.transform.childCount; kIndex++) {
				Transform child = gate.transform.GetChild(kIndex);
				if (!child.GetComponent<BoxCollider>()) {
					child.gameObject.active = true;
				}
			}
		}

		private static void DisableGateVisual(GameObject gate) {
			for (int kIndex = 0; kIndex < gate.transform.childCount; kIndex++) {
				Transform child = gate.transform.GetChild(kIndex);
				if (!child.GetComponent<BoxCollider>()) {
					child.gameObject.active = false;
				}
			}
		}

		private void FadeoutOpenPhase2() {
			GameObject.Destroy(openPhase2.GetComponentInChildren<Animator>());
			Timing.RunCoroutine(_FadeoutOpenPhase2(), Segment.LateUpdate);
		}

		private IEnumerator<float> _FadeoutOpenPhase2() {
			MeshRenderer mr = openPhase2.GetComponentInChildren<MeshRenderer>();
			float duration = 0.5f;
			float target = -.9f;
			float current = mr.material.GetFloat("_Scale");
			float elapsed = 0;
			while (true) {
				yield return Timing.WaitForOneFrame;
				elapsed += Time.deltaTime;
				float progress = elapsed / duration;
				mr.material.SetFloat("_Scale", Mathf.Lerp(current, target, progress));
				if (elapsed >= duration) {
					break;
				}
			}
		}

		private enum State
		{
			WaitToOpen,
			Opening,
			Opened,
			WaitToClose,
			Closing,
			Closed,
		}
	}
}
