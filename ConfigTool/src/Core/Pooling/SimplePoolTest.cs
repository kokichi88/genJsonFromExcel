using System;
using UnityEngine;

namespace Core.Pooling {
	public class SimplePoolTest : MonoBehaviour {
		private void Start() {
			AddDefinitionWithNullOrEmptyPathShouldThrowException();
			AddDefinitionWithNullPrefabShouldThrowException();
			ObtainUndefinedShouldThrowException();
			ObtainAlreadyDefinedShouldReturnItem();
			Obtain2TimesShouldReturnDifferentItems();
			ObtainAndReturn();
			ReObtainShouldBeTheSame();
		}

		public void AddDefinitionWithNullOrEmptyPathShouldThrowException() {
			SimplePool sp = new SimplePool();
			try {
				sp.AddDefinition(new SimplePool.Definition(null, null));
			}
			catch (Exception e) {
				DLog.Log("null path passed");
			}

			try {
				sp.AddDefinition(new SimplePool.Definition("", null));
			}
			catch (Exception e) {
				DLog.Log("empty path passed");
			}
		}

		public void AddDefinitionWithNullPrefabShouldThrowException() {
			SimplePool sp = new SimplePool();
			try {
				sp.AddDefinition(new SimplePool.Definition("path/to/prefab", null));
			}
			catch (Exception e) {
				DLog.Log("null prefab passed");
			}
		}

		public void ObtainUndefinedShouldThrowException() {
			SimplePool sp = new SimplePool();
			try {
				sp.Obtain(null);
			}
			catch (Exception e) {
				DLog.Log("undefined-null passed");
			}
			try {
				sp.Obtain("");
			}
			catch (Exception e) {
				DLog.Log("undefined-empty passed");
			}
			try {
				sp.Obtain("path/to/prefab");
			}
			catch (Exception e) {
				DLog.Log("undefined passed");
			}
		}

		public void ObtainAlreadyDefinedShouldReturnItem() {
			SimplePool sp = new SimplePool();
			string path = "path";
			GameObject prefab = new GameObject("Prefab");
			sp.AddDefinition(new SimplePool.Definition(path, prefab));

			SimplePoolItem item = sp.Obtain(path);
			if (item != null && item.gameObject.activeInHierarchy) {
				DLog.Log("already defined passed");
			}
		}

		public void Obtain2TimesShouldReturnDifferentItems() {
			SimplePool sp = new SimplePool();
			string path = "path";
			GameObject prefab = new GameObject("Prefab");
			sp.AddDefinition(new SimplePool.Definition(path, prefab));

			SimplePoolItem item1 = sp.Obtain(path);
			SimplePoolItem item2 = sp.Obtain(path);
			if (item1 != item2 && item1.gameObject != item2.gameObject) {
				DLog.Log("Obtain2TimesShouldReturnDifferentItems passed");
			}
		}

		public void ObtainAndReturn() {
			SimplePool sp = new SimplePool();
			string path = "path";
			GameObject prefab = new GameObject("Prefab");
			sp.AddDefinition(new SimplePool.Definition(path, prefab));

			SimplePoolItem item = sp.Obtain(path);
			sp.Return(item);
			if (item.gameObject.activeInHierarchy == false) {
				DLog.Log("ObtainAndReturn passed");
			}
		}

		public void ReObtainShouldBeTheSame() {
			SimplePool sp = new SimplePool();
			string path = "path";
			GameObject prefab = new GameObject("Prefab");
			sp.AddDefinition(new SimplePool.Definition(path, prefab));

			SimplePoolItem item = sp.Obtain(path);
			sp.Return(item);
			SimplePoolItem reobtainItem = sp.Obtain(path);
			if (item == reobtainItem && item.gameObject == reobtainItem.gameObject) {
				DLog.Log("ReObtainShouldBeTheSame passed");
			}
		}
	}
}