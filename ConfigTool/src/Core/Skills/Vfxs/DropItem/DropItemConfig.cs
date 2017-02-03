using System;
using System.Collections.Generic;
using Core.Utils.Extensions;
using Equipment;
using Ssar.Combat.UI;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Core.Skills.Vfxs.DropItem
{
    public class DropItemConfig : ScriptableObject
    {
        public List<DropItemEntry> entries = new List<DropItemEntry>();

        #region EDITOR
#if UNITY_EDITOR
        private HUDConfig hudConfig;
        private List<string> categoryList = new List<string>();
        private List<Rarity> rarityList = new List<Rarity>();
        private string selectedCategory;
        private Rarity selectedRarity = Rarity.Common;
        private Dictionary<string, bool> foldCategories = new Dictionary<string, bool>();
        private Dictionary<DropItemEntry, bool> foldEntries = new Dictionary<DropItemEntry, bool>();
        private int upIndex = -1;
        private int downIndex = -1;
        private int removeIndex = -1;
        private DropItemEntry copiedEntry;
        
        public void OnGUI()
        {
            if (hudConfig == null)
            {
                LoadHudConfig();
            }
            
            if (categoryList == null || categoryList.Count < 1)
            {
                LoadCategoryList();
            }

            if (rarityList == null || rarityList.Count < 1)
            {
                LoadRarityList();
            }
            
            EditorGUI.BeginChangeCheck();
            
            DrawControlGroup();
            DrawEntries();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
                // SaveFoldout(this, foldout);
            }
        }

        private void LoadHudConfig()
        {
            hudConfig = Resources.Load<HUDConfig>(ScriptableObjectData.HUD_CONFIG_PATH);
        }

        private void LoadCategoryList()
        {
            categoryList.Clear();
            categoryList.Add(EquipmentCategory.Weapon.ToString());
            categoryList.Add(EquipmentCategory.Armor.ToString());
            categoryList.Add(EquipmentCategory.Accessory.ToString());

            selectedCategory = categoryList[0];
        }

        private void LoadRarityList()
        {
            rarityList = new List<Rarity>((Rarity[]) Enum.GetValues(typeof(Rarity)));
        }

        private void DrawControlGroup()
        {
            EditorGUILayout.Space();
            selectedRarity = (Rarity) EditorGUILayout.EnumPopup("Rarity", selectedRarity);
            int index = categoryList.IndexOf(selectedCategory);
            if (index < 0) index = 0;
            index = EditorGUILayout.Popup("Category", index, categoryList.ToArray());
            selectedCategory = categoryList[index];

            if (GUILayout.Button("Add"))
            {
                entries.Add(new DropItemEntry(selectedCategory, selectedRarity));
                entries.Sort(EntryComparison);
            }
        }

        private void DrawEntries()
        {
            EditorGUILayout.Space();
            
            upIndex = -1;
            downIndex = -1;
            removeIndex = -1;
            
            Dictionary<string, List<EntryData>> dict = new Dictionary<string, List<EntryData>>();
            for (int i = 0; i < entries.Count; i++)
            {
                DropItemEntry entry = entries[i];
                if (!dict.ContainsKey(entry.category))
                {
                    dict.Add(entry.category, new List<EntryData>());
                }
                
                dict[entry.category].Add(new EntryData(i, entry));
            }

            foreach (KeyValuePair<string,List<EntryData>> pair in dict)
            {
                DrawCategory(pair.Key, pair.Value);
            }
            
            if (upIndex > 0)
            {
                entries.Swap(upIndex, upIndex - 1);
                upIndex = -1;
                return;
            }

            if (downIndex >= 0)
            {
                entries.Swap(downIndex, downIndex + 1);
                downIndex = -1;
                return;
            }

            if (removeIndex >= 0)
            {
                entries.RemoveAt(removeIndex);
                removeIndex = -1;
            }
        }

        private void DrawCategory(string category, List<EntryData> entryDatas)
        {
            EditorGUILayout.Space();

            using (new EditorHelper.Box(false))
            {
                bool existed = foldCategories.ContainsKey(category);
                if (!existed)
                {
                    foldCategories[category] = false;
                }

                bool foldout = foldCategories[category];
                        
                GUIStyle gs = new GUIStyle(EditorStyles.foldout);
                gs.fontStyle = FontStyle.Bold;
                gs.fontSize = 14;
                
                foldout = GUILayout.Toggle(foldout, $"{category}", gs);

                foldCategories[category] = foldout;
                
                if (!foldout) return;

                foreach (EntryData entryData in entryDatas)
                {
                    DrawEntry(entryData.index, entryData.entry);
                }
            }
        }

        private void DrawEntry(int index, DropItemEntry entry)
        {
            EditorGUILayout.Space();
            using (new EditorHelper.Box(false))
            {
                using (new EditorHelper.Horizontal())
                {
                    bool existed = foldEntries.ContainsKey(entry);
                    if (!existed)
                    {
                        foldEntries[entry] = false;
                    }

                    bool foldout = foldEntries[entry];
                        
                    GUIStyle gs = new GUIStyle(EditorStyles.foldout);
                    gs.fontStyle = FontStyle.Bold;
                    gs.fontSize = 13;
                    if (hudConfig)
                    {
                        GUIStyleState normal = new GUIStyleState();
                        normal.textColor = hudConfig.GetHUDTextInfo(entry.rarity.ToHudTextType()).color * 2;
                        gs.normal = normal;
                        gs.focused = normal;
                        gs.hover = normal;
                        gs.active = normal;
                        gs.onActive = normal;
                        gs.onFocused = normal;
                        gs.onHover = normal;
                        gs.onNormal = normal;
                    }

                    foldout = GUILayout.Toggle(foldout, $"{entry.category} - {entry.rarity}", gs);
                    foldEntries[entry] = foldout;
                    
                    if (GUILayout.Button("Copy", GUILayout.Width(70)))
                    {
                        copiedEntry = entry;
                    }
                    
                    if (GUILayout.Button("Paste", GUILayout.Width(70)))
                    {
                        if (copiedEntry != null)
                            entry.Paste(copiedEntry);
                    }
                    
                    /*using (new EditorHelper.DisabledGroup(index == 0))
                    {
                        if (GUILayout.Button("Up", GUILayout.Width(70)))
                        {
                            upIndex = index;
                        }
                    }
                
                    using (new EditorHelper.DisabledGroup(index == entries.Count - 1))
                    {
                        if (GUILayout.Button("Down", GUILayout.Width(70)))
                        {
                            downIndex = index;
                        }
                    }*/

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        removeIndex = index;
                    }
                        
                    if (!foldout) return;
                }
                    
                entry.dropPrefabPath = new EditorHelper.PrefabDrawer().DrawFromPathString("Drop Prefab", entry.dropPrefabPath);
                entry.trackPrefabPath = new EditorHelper.PrefabDrawer().DrawFromPathString("Track Prefab", entry.trackPrefabPath);
                entry.impactPrefabPath = new EditorHelper.PrefabDrawer().DrawFromPathString("Impact Prefab", entry.impactPrefabPath);
            }
        }
#endif
        #endregion

        public DropItemEntry GetEntry(string category, Rarity rarity)
        {
            foreach (DropItemEntry entry in entries)
            {
                if (entry.category.Equals(category) && entry.rarity == rarity)
                {
                    return entry;
                }
            }

            return null;
        }

        public List<string> ListAllPrefabPaths()
        {
            List<string> r = new List<string>();
            foreach (DropItemEntry entry in entries)
            {
                List<string> list = entry.ListAllPrefabPaths();
                foreach (string s in list)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    r.Add(s);
                }
            }

            return r;
        }

        private int EntryComparison(DropItemEntry a, DropItemEntry b)
        {
            int val1 = a.category.CompareTo(b.category);
            return val1 != 0 ? val1 : a.rarity.CompareTo(b.rarity);
        }

        #region Subclass
        public class ConfigPath
        {
            private static string dir = "Config/Combat/Vfx";
            private static string fileName = "DropItem";
            private static string extension = ".asset";
            private static string resourcePath = dir + "/" + fileName;
            private static string resourcePathWithExtension = resourcePath + extension;

            public static string ResourcePath
            {
                get { return resourcePath; }
            }

            public static string ResourcePathWithExtension
            {
                get { return resourcePathWithExtension; }
            }
        }
        
        private class EntryData
        {
            public int index;
            public DropItemEntry entry;

            public EntryData(int index, DropItemEntry entry)
            {
                this.index = index;
                this.entry = entry;
            }
        }
        #endregion
    }
}