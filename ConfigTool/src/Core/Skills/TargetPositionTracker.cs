using System;
using System.Collections.Generic;
using System.Text;
using Core.Skills;
using UnityEngine;

namespace Core.Skills {
    public class TargetPositionTracker {
        private const int history_size = 300;
        private Character target;

        private float elapsed;
        private Dictionary<int, Vector3> positionsByFrames = new Dictionary<int, Vector3>();

        public void Update(float dt) {
            if(target == null) return;

            elapsed += dt;
            int frame = (int) (elapsed * 30);
            int frameToRemove = frame - history_size;
            if (positionsByFrames.ContainsKey(frameToRemove)) {
                positionsByFrames.Remove(frameToRemove);
            }
            positionsByFrames[frame] = target.Position();
            /*DLog.Log(frame + " size " + positionsByFrames.Keys.Count);
            StringBuilder sb=new StringBuilder();
            foreach (int key in positionsByFrames.Keys) {
                sb.Append(key + " ");
            }
            DLog.Log(sb);*/
        }

        public void StartTracking(Character target) {
            this.target = target;
        }

        public Vector3 PositionAt(int frame) {
            if(!positionsByFrames.ContainsKey(frame))
                throw new Exception(string.Format("Position at frame '{0}' is not tracked", frame));

            return positionsByFrames[frame];
        }
    }
}