using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Ssar.Dungeon.Model;
using Com.LuisPedroFonseca.ProCamera2D;
using Core.Commons;
using Core.Skills;
using Ssar.Combat.Skills.Events;
using UnityEngine;

namespace Core.Skills {
	public interface Environment {
		List<Character> FindNearbyCharacters(Character subject, Vector3 relativePosition, float distance, params FindingFilter[] filter);
		List<Character> FindNearbyCharacters(Character subject, Vector3 pivotPosition,
		                                     Vector3 relativePosition, float distance,
		                                     params FindingFilter[] filter);

		float MostLeftOfMap();
		float MostRightOfMap();
		float CeilOfMap();
		float GroundOfMap(float positionOnXAxis);
		void PlayCameraFx(Character caster, BaseEvent ef);
		void ShakeCamera(Vector2 strength, float duration, int vibrato, float smoothness, float randomness, bool useRandomInitialAngel, Vector3 rotation);

		void PlaySfx(string audioClipPath, float volume = 1f, bool loop = false, float pitch = 1f,
		             Action<string, AudioClip> callback = null);
		void StopSfx(AudioClip audioClip);
		GameObject InstantiateGameObject(GameObject prefab);
		GameObject InstantiateGameObject(string prefabPath);
		GameObject InstantiateGameObject(string prefabPath, GameObject prefab);
		void RecycleVfx(GameObject vfx);
		Vector4 ViewPortBoundaryInWorldPosition();
		void StopCameraFromTrackingTargets();
		void StartCameraToTrackPosition(Vector2 position);
		void StartCameraToTrackEntity(int entityId);
		bool IsHoldingAttack();
		void StopCameraFromTrackingOnYAxis();
		void StartCameraToTrackingOnYAxis();
		void FadeCamera(float duration, Color color, AnimationCurve alphaCurve);
		Vector2 GetPositionOnMap(string name);
		Vector2 MostLeftAndMostRightOfCurrentStage();
		CharacterId CharacterToControl();
		CinematicTarget PerformCameraCinematicZoom(Vector2 position, float easeDuration, EaseType easeType,
		                                float holdDuration, float zoomLevel);
		void PerformCameraSlowMotion(float timeScale, float duration);
		bool IsBoss(int entityId);
		MapColliderBoundariesConfig MapColliders();
		Camera GetCamera();
		GameObject GetUICamera();
		Material FindMaterialById(CharacterId characterId, int visualId, int id);
		Material FindDefaultMaterialById(CharacterId characterId, int id);
		GameObject FindPrefabById(CharacterId characterId, int visualId, int id);
		GameObject FindDefaultPrefabById(CharacterId characterId, int id);
		GameObject FindVfxProxyPrefabById(CharacterId characterId, int id, int[] typesOfOngoingModifiers, int[] weaponVisualIds);
	}

	public enum FindingFilter {
		ExcludeMe,
		ExcludeAllies,
		ExcludeEnemies,
		ExcludeDead
	}
}
