using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class LightningTrail : MonoBehaviour {
		public LightningSplineScript lightningSpline;
		public float interval = 0.033f;
		public float ttl = 1;
		public bool independentMode;
		public float nodeProducingDuration = 5;
		public float nodeStandStillDuration = 1;
		public float lerpSpeed = 3;

		public Transform start;
		public float distanceOffsetFromStart;
		public Transform end;
		public int density;

		// public Transform parent;

		private List<Spline> splines = new List<Spline>();
		private List<GameObject> lightningJoints = new List<GameObject>();

		private void Awake() {
			if (independentMode) {
				Init();
			}
		}

		public void Init() {
			foreach (GameObject node in lightningSpline.LightningPath) {
				node.transform.localPosition = Vector3.zero;
			}

			Vector3 directionFromStartToEnd = (end.position - start.position).normalized;
			Vector3 offsetStartPosition = start.position + directionFromStartToEnd * distanceOffsetFromStart;
			Vector3 diffFromOffsetStartToEnd = end.position - offsetStartPosition;
			Vector3 halfDiffFromStartToEnd = diffFromOffsetStartToEnd / 2f;
			float[] xValues = UniformDistribution(halfDiffFromStartToEnd.x, diffFromOffsetStartToEnd.x, density);
			float[] yValues = UniformDistribution(halfDiffFromStartToEnd.y, diffFromOffsetStartToEnd.y, density);
			float[] zValues = UniformDistribution(halfDiffFromStartToEnd.z, diffFromOffsetStartToEnd.z, density);
			for (int i = 0; i < density; i++) {
				Vector3 rel = new Vector3(xValues[i], yValues[i], zValues[i]);
				Vector3 worldPos = offsetStartPosition + rel;
				GameObject lightningJoint = new GameObject("LightningJoint" + i);
				lightningJoint.transform.parent = independentMode ? transform : transform.parent;
				lightningJoint.transform.position = worldPos;
				lightningJoints.Add(lightningJoint);
				GameObject lightningSplineGo = i == 0 ? lightningSpline.gameObject : Instantiate(lightningSpline.gameObject);
				lightningSplineGo.transform.parent = transform;
				splines.Add(
					new Spline(
						interval, lightningSplineGo.GetComponent<LightningSplineScript>(), ttl,
						lightningJoint.transform, nodeProducingDuration, nodeStandStillDuration, lerpSpeed
					)
				);
			}

			transform.parent = null;
		}

		private void LateUpdate() {
			bool isAllSplineFinish = true;
			foreach (Spline s in splines) {
				s.LateUpdate(Time.deltaTime);
				isAllSplineFinish &= s.IsFinish;
			}

			if (isAllSplineFinish) {
				foreach (GameObject lightningJoint in lightningJoints) {
					Destroy(lightningJoint);
				}
			}
		}

		public static float[] UniformDistribution(float pivot, float amplitude, int density) {
			float left = pivot - amplitude / 2f;
			float[] values = new float[density];
			for (int i = 0; i < values.Length; i++) {
				float delta = 0;
				if (density == 1) {
					delta = amplitude / 2;
				}
				else {
					delta = amplitude / (density - 1);
				}

				if (density == 1) {
					values[i] = left + delta;
				}
				else {
					values[i] = left + i * delta;
				}
			}

			return values;
		}

		private class Spline {
			private readonly float interval;
			private readonly LightningSplineScript lightningSpline;
			private readonly float ttl;
			private readonly Transform parent;
			private readonly float nodeProducingDuration;
			private readonly float nodeStandStillDuration;
			private readonly float lerpSpeed;

			private float elapsed;
			private float intervalElapsed;
			private int count = 0;
			private List<float> remainingTtls = new List<float>();
			private List<int> indexToRemove = new List<int>();
			private List<GameObject> nodePool = new List<GameObject>();
			private List<GameObject> activeNodes = new List<GameObject>();
			private List<GameObject> inactiveNodes = new List<GameObject>();
			private bool isFinish;

			public Spline(float interval, LightningSplineScript lightningSpline, float ttl,
			              Transform parent, float nodeProducingDuration, float nodeStandStillDuration,
			              float lerpSpeed) {
				this.interval = interval;
				this.lightningSpline = lightningSpline;
				this.ttl = ttl;
				this.parent = parent;
				this.nodeProducingDuration = nodeProducingDuration;
				this.nodeStandStillDuration = nodeStandStillDuration;
				this.lerpSpeed = lerpSpeed;
				/*nodePool.AddRange(lightningSpline.LightningPath);
				activeNodes.AddRange(lightningSpline.LightningPath);
				for (int i = 0; i < lightningSpline.LightningPath.Count; i++) {
					remainingTtls.Add(ttl);
				}*/
			}

			public bool IsFinish => isFinish;

			public void LateUpdate(float dt) {
				elapsed += dt;
				intervalElapsed += Time.deltaTime;
				if (elapsed < nodeProducingDuration && intervalElapsed >= interval) {
					intervalElapsed -= interval;
					count++;

					if (parent != null) {
						GameObject node = null;
						if (count > lightningSpline.LightningPath.Count) {
							if (inactiveNodes.Count > 0) {
								node = inactiveNodes[0];
								inactiveNodes.RemoveAt(0);
								activeNodes.Add(node);
							}
							else {
								node = new GameObject("Node" + count);
								nodePool.Add(node);
								activeNodes.Add(node);
							}
						}
						else {
							node = lightningSpline.LightningPath[count - 1];
							nodePool.Add(node);
							activeNodes.Add(node);
							for (int i = count; i < lightningSpline.LightningPath.Count; i++) {
								lightningSpline.LightningPath[i].transform.parent = parent;
								lightningSpline.LightningPath[i].transform.localPosition = Vector3.zero;
							}
						}
						node.transform.parent = null;
						node.transform.position = parent.position;
						if (count > lightningSpline.LightningPath.Count) {
							lightningSpline.LightningPath.Add(node);
						}
						remainingTtls.Add(ttl);

						if (count > nodePool.Count) {
							count = nodePool.Count;
						}
					}
				}

				indexToRemove.Clear();
				for (int i = 0; i < remainingTtls.Count; i++) {
					remainingTtls[i] = remainingTtls[i] - Time.deltaTime;
					if (remainingTtls[i] <= 0) {
						indexToRemove.Add(i);
					}
				}

				for (int i = indexToRemove.Count - 1; i >= 0; i--) {
					int index = indexToRemove[i];
					GameObject node = lightningSpline.LightningPath[index];
					activeNodes.Remove(node);
					inactiveNodes.Add(node);
					lightningSpline.LightningPath.RemoveAt(index);
					remainingTtls.RemoveAt(index);
				}

				if (elapsed >= nodeStandStillDuration && activeNodes.Count > 1) {
					Vector3 direction = activeNodes[1].transform.position - activeNodes[0].transform.position;
					direction = direction.normalized;
					Vector3 velocity = direction * lerpSpeed;
					activeNodes[0].transform.position += dt * velocity;
					Vector3 direction2 = activeNodes[1].transform.position - activeNodes[0].transform.position;
					Vector3 diff = direction2;
					float dotProduct = Vector3.Dot(direction, direction2);
					if (dotProduct < 0 || diff == Vector3.zero) {
						GameObject node = activeNodes[0];
						activeNodes.Remove(node);
						nodePool.Remove(node);
						int index = lightningSpline.LightningPath.IndexOf(node);
						lightningSpline.LightningPath.RemoveAt(index);
						remainingTtls.RemoveAt(index);
					}
				}

				/*if (elapsed >= nodeStandStillDuration && activeNodes.Count > 1) {
					float progress = (elapsed - nodeStandStillDuration) / (ttl - nodeStandStillDuration);
					Vector3 nodePos = activeNodes[i].transform.position;
					activeNodes[i].transform.position =
						Vector3.Lerp(nodePos, activeNodes[i + 1].transform.position, progress);
				}*/
				/*for (int i = 0; i < remainingTtls.Count; i++) {
					float nodeElapsed = ttl - remainingTtls[i];
					if (nodeElapsed >= nodeStandStillDuration && activeNodes.Count > i) {
						float progress = (nodeElapsed - nodeStandStillDuration) / (ttl - nodeStandStillDuration);
						Vector3 nodePos = activeNodes[i].transform.position;
						activeNodes[i].transform.position =
							Vector3.Lerp(nodePos, activeNodes[i + 1].transform.position, progress);
					}
				}*/

				if (elapsed >= nodeProducingDuration) {
					if (activeNodes.Count < PathGenerator.MinPointsForSpline) {
						lightningSpline.enabled = false;
						if (nodePool.Count > 0) {
							int index = nodePool.Count - 1;
							GameObject.Destroy(nodePool[index]);
							nodePool.RemoveAt(index);
						}
					}

					if (nodePool.Count < 1) {
						isFinish = true;
					}
				}
			}
		}
	}
}