using System;
using System.Collections.Generic;
using Checking;
using Core.Commons;
using Core.DungeonLogic.Stages.Challenges;
using UnityEngine;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Waves {
	public class DefaultWaveLogic : WaveLogic {
		private static string unknown = "Unknown";

		private int waveOrder;
		private List<Challenge> challenges = new List<Challenge>();
		private NotNullReference notNullReference = new NotNullReference();

		private List<SsarTuple<CharacterId, int>> monsterIdsAndSpawnCount;

		public DefaultWaveLogic(int waveOrder) {
			this.waveOrder = waveOrder;
			monsterIdsAndSpawnCount = new List<SsarTuple<CharacterId, int>>();
		}

		public void AddChallenge(Challenge challenge) {
			notNullReference.Check(challenge, "challenge");

			challenges.Add(challenge);
			monsterIdsAndSpawnCount.Add(challenge.ShowMonsterIdAndSpawnCount());
		}

		public bool IsFinished() {
			for (int kIndex = 0; kIndex < challenges.Count; kIndex++) {
				if (!challenges[kIndex].IsFinished()) return false;
			}

			return true;
		}

		public void Update(float dt) {
//			DLog.Log("Wave " + waveOrder + " update()");
			for (int kIndex = 0; kIndex < challenges.Count; kIndex++) {
				challenges[kIndex].Update(dt, waveOrder);
			}

//			if (!IsFinished()) {
//				DLog.Log("unfinish reason " + UnfinishedReason());
//			}
		}

		public string UnfinishedReason() {
			for (int kIndex = 0; kIndex < challenges.Count; kIndex++) {
				if (!challenges[kIndex].IsFinished()) return challenges[kIndex].UnfinishedReason();
			}

			return unknown;
		}

		public List<SsarTuple<CharacterId, int>> ShowMonsterIdAndSpawnCount() {
			return monsterIdsAndSpawnCount;
		}
	}
}