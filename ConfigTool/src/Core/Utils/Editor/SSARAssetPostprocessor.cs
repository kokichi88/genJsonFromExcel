using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.Config;
using Core.DungeonLogic.Configs.Editor;
using LitJson;
using NUnit.Framework;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace Core.Utils.Editor {
	public class SSARAssetPostprocessor : AssetPostprocessor {
		void OnPreprocessModel() {
			ModelImporter modelImporter = assetImporter as ModelImporter;
			modelImporter.importMaterials = false;

			if (assetPath.Contains("Characters")) {
				modelImporter.importNormals = ModelImporterNormals.Calculate;
				modelImporter.normalSmoothingAngle = 180;
				modelImporter.globalScale = 1;
				string[] arrStrs = assetPath.Split("/"[0]);
				string assetName = arrStrs [arrStrs.Length - 1];
				if (assetName.Contains(".fbx")) {
					modelImporter.animationType = ModelImporterAnimationType.Legacy;
					modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
					modelImporter.animationRotationError = 0.05f;
					modelImporter.animationPositionError = 0.05f;
					modelImporter.animationScaleError = 0.05f;
					if (assetName.StartsWith("@Move") ||
						assetName.StartsWith("@Idle") ||
						assetName.StartsWith("@Stun") ||
						assetName.StartsWith("@Shackle") ||
						assetName.StartsWith("@Ragdoll") ||
						assetName.EndsWith("Loop.fbx")
					) 
					{
						modelImporter.animationWrapMode = WrapMode.Loop;
					}
				}
				else {
					modelImporter.animationType = ModelImporterAnimationType.None;
				}
			}
		}

		void OnPreprocessTexture() {
			TextureImporter textureImporter = (TextureImporter) assetImporter;
			textureImporter.mipmapEnabled = false;
			textureImporter.compressionQuality = (int) TextureCompressionQuality.Normal;
			if (textureImporter.assetPath.Contains("A_Texture")||textureImporter.assetPath.Contains("B_RawTexture"))
			{
				textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
				textureImporter.alphaIsTransparency = true;
			}

			if (textureImporter.assetPath.Contains("Equipment")&&textureImporter.assetPath.Contains("Characters"))
			{
				if(textureImporter.maxTextureSize>1024)
					textureImporter.maxTextureSize = 1024;
			}

			if (textureImporter.assetPath.Contains("Characters")) {
				textureImporter.textureType = TextureImporterType.Default;
			}
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			/*foreach (string str in importedAssets)
			{
				Debug.Log("Reimported Asset: " + str);
			}
			foreach (string str in deletedAssets)
			{
				Debug.Log("Deleted Asset: " + str);
			}

			foreach (string str in movedAssets) {
				DLog.Log("Moved asset: " + str);
			}

			foreach (string str in movedFromAssetPaths) {
				DLog.Log("Moved from asset path: " + str);
			}*/

			List<MovedAsset> moved = new List<MovedAsset>();
			for (int kIndex = 0; kIndex < movedAssets.Length; kIndex++) {
				string movedFromAssetPath = movedFromAssetPaths[kIndex];
				try {
					if (movedFromAssetPath.EndsWith(".mat") || movedFromAssetPath.EndsWith(".prefab")) {
						string movedFromResourcePath = new AssetFile(movedFromAssetPath).ShowResourcePath();
						string movedToResourcePath = new AssetFile(movedAssets[kIndex]).ShowResourcePath();
						string movedFromParentFolder =
							movedFromResourcePath.Substring(0, movedFromResourcePath.LastIndexOf("/"));
						string movedToParentFolder = movedToResourcePath.Substring(0, movedToResourcePath.LastIndexOf("/"));
						/*bool isRename = movedFromParentFolder.Equals(movedToParentFolder);
						if (isRename) {
							DLog.Log("SSARAssetPostprocessor: Renaming detected, skip processing for asset: " + movedFromResourcePath);
							continue;
						}*/

						moved.Add(new MovedAsset(
							movedFromResourcePath,
							movedToResourcePath
						));
						if (movedAssets[kIndex].EndsWith(".asset")) {
							ScriptableObject so = Resources.Load<ScriptableObject>(moved[kIndex].newPath);
							if (so is SkillFrameConfig) {
								DLog.Log("Rename skill config, skip detection");
								return;
							}
						}
					}
				}
				catch (Exception e) {
					DLog.LogException(new Exception(movedFromAssetPath, e));
				}
			}

			ContextMenuExtension.MoveOrRenameAsset(moved);

			var editors = Resources.FindObjectsOfTypeAll<DungeonSpawnConfigEditor>();

			foreach (string importedAsset in importedAssets) {
				/*if (importedAsset.EndsWith(".prefab")) {
					GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(importedAsset);
					DungeonSpawnConfig dungeonSpawnConfig = prefab.GetComponent<DungeonSpawnConfig>();
					if (!dungeonSpawnConfig) continue;

					string join = string.Join("", dungeonSpawnConfig.config);
					dungeonSpawnConfig.configObjectForEditor = new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Config>();
					/*foreach (DungeonSpawnConfigEditor editor in editors) {
						var prefabRoot = PrefabUtility.FindPrefabRoot(((DungeonSpawnConfig)editor.target).gameObject);
						var prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
						string prefabPath = AssetDatabase.GetAssetPath(prefabParent);
						if (string.IsNullOrEmpty(prefabPath)) {
							prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
						}
						if(!prefabPath.Equals(importedAsset)) continue;

						DLog.Log("PopulateUsingDataReadFromDisk for asset " + importedAsset);
						editor.PopulateUsingDataReadFromDisk(dungeonSpawnConfig.config, dungeonSpawnConfig.configObjectForEditor);
					}#1#

					foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>()) {
						DungeonSpawnConfig otherDsc = go.GetComponent<DungeonSpawnConfig>();
						if (!otherDsc) continue;
						var prefabRoot = PrefabUtility.FindPrefabRoot(otherDsc.gameObject);
						var prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
						string prefabPath = AssetDatabase.GetAssetPath(prefabParent);
						if (string.IsNullOrEmpty(prefabPath)) {
							prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
						}
						if(!prefabPath.Equals(importedAsset)) continue;

						// DLog.Log("PopulateUsingDataReadFromDisk for asset " + importedAsset);
						// otherDsc.PopulateUsingDataReadFromDisk(dungeonSpawnConfig.config, dungeonSpawnConfig.configObjectForEditor);
						otherDsc.config = dungeonSpawnConfig.config;
						otherDsc.configObjectForEditor = dungeonSpawnConfig.configObjectForEditor;
					}
				}*/
			}
		}

		static void _OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
		                                    string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string ia in importedAssets) {
				if (!ia.EndsWith(".prefab")) continue;
				AssetFile af = new AssetFile(ia);
				if(!af.ShowResourceName().ToLower().Contains("terrain.")) continue;

				GameObject terrainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ia);
				GameObject instance = GameObject.Instantiate(terrainPrefab);
				List<Transform> waters = new List<Transform>();
				foreach (Transform transform in instance.GetComponentsInChildren<Transform>()) {
					if (transform.name.StartsWith("Water")) {
						waters.Add(transform);
					}
				}
				int waveCount = 0;

				foreach (Transform water in waters) {
					int count = water.childCount;
					waveCount += count;
					List<Transform> children = new List<Transform>();
					for (int i = 0; i < count; i++) {
						children.Add(water.GetChild(i));
					}

					foreach (Transform transform in children) {
						GameObject.DestroyImmediate(transform.gameObject);
					}
				}

				if (waveCount > 0) {
					bool success;
					PrefabUtility.SaveAsPrefabAsset(instance, ia, out success);
					DLog.Log(string.Format("Cleanup Waves from '{0}': {1}", ia, success));
				}
				GameObject.DestroyImmediate(instance);
			}

			CheckForMissingKeyFrameForJointOfAnimationClip(importedAssets);
		}

		private static void CheckForMissingKeyFrameForJointOfAnimationClip(string[] importedAssets) {
			foreach (string importedAsset in importedAssets) {
				if (!importedAsset.Contains("Characters")) continue;
				if (!importedAsset.Contains("Animations")) continue;
				if (!importedAsset.EndsWith(".anim")) continue;

				string animationFolderPath = importedAsset.Substring(0, importedAsset.LastIndexOf("/"));
				string characterFolderPath = animationFolderPath.Substring(0, animationFolderPath.LastIndexOf("/"));
				//Debug.Log("animationFolder: " + animationFolder);
				//Debug.Log("characterFolder: " + characterFolder);
				string characterFolderName = characterFolderPath.Substring(
					characterFolderPath.LastIndexOf("/") + 1,
					characterFolderPath.Length - characterFolderPath.LastIndexOf("/") - 1
				);
				//DLog.Log("characterFolderName: " + characterFolderName);
				int characterGroupId = Convert.ToInt32(characterFolderName.Split('_')[1]);
				//DLog.Log("Character group id " + characterGroupId);
				GameObject characterPrefab;
				Transform allJoint = null;
				foreach (string child in Directory.GetDirectories(new AssetFile(characterFolderPath).ShowAbsolutePath())) {
					string childAbsolutePath = child.Replace("\\", "/");
					//DLog.Log(childAbsolutePath);
					string childRelativePath = new AbsoluteFile(childAbsolutePath + "/" + characterGroupId + "_1.prefab")
						.ShowRelativePathAsAsset();
					//DLog.Log("chileRelativePath: " + childRelativePath);
					characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
						childRelativePath
					);
					allJoint = characterPrefab.transform.FindDeepChild("All_jnt");
					if (characterPrefab != null && allJoint != null) {
						//DLog.Log(characterPrefab.name);
						break;
					}
				}

				Transform[] joints = allJoint.GetComponentsInChildren<Transform>();
				string[] jointNamesFromRig = joints.Select(transform => transform.name).ToArray();
				HashSet<string> jointNamesFromClip = new HashSet<string>();
				AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(importedAsset);
				foreach (EditorCurveBinding ecb in AnimationUtility.GetCurveBindings(clip)) {
					//DLog.Log(ecb.path);
					string jointName = ecb.path.Substring(
						ecb.path.LastIndexOf("/") + 1, ecb.path.Length - ecb.path.LastIndexOf("/") - 1
					);
					//DLog.Log(jointName);
					jointNamesFromClip.Add(jointName);
				}

				foreach (string jointFromRig in jointNamesFromRig) {
					bool found = false;
					foreach (string jointFromClip in jointNamesFromClip) {
						if (jointFromRig.Equals(jointFromClip)) {
							found = true;
							break;
						}
					}

					if (!found) {
						DLog.LogError(string.Format(
							"Animation clip '{0}' does not contain KeyFrame for joint of name '{1}'",
							clip.name, jointFromRig
						));
					}
				}
			}
		}

		public class MovedAsset {
			public string oldPath;
			public string newPath;

			public MovedAsset(string oldPath, string newPath) {
				this.oldPath = oldPath;
				this.newPath = newPath;
			}
		}
	}
}