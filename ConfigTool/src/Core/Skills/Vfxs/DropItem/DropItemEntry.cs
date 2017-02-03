using System;
using System.Collections.Generic;
using Equipment;

namespace Core.Skills.Vfxs.DropItem
{
    [Serializable]
    public class DropItemEntry
    {
        // public string key;
        public string dropPrefabPath = string.Empty;
        public string trackPrefabPath = string.Empty;
        public string impactPrefabPath = string.Empty;
        public string category;
        public Rarity rarity;

        public DropItemEntry(string category, Rarity rarity)
        {
            this.category = category;
            this.rarity = rarity;
        }

        public List<string> ListAllPrefabPaths()
        {
            return new List<string>(new[] {dropPrefabPath});
        }

#if UNITY_EDITOR
        public void Paste(DropItemEntry otherEntry)
        {
            dropPrefabPath = otherEntry.dropPrefabPath;
            trackPrefabPath = otherEntry.trackPrefabPath;
            impactPrefabPath = otherEntry.impactPrefabPath;
        }
#endif
    }
}