using System;

namespace Assets.Scripts.Core.UnlockableSystem {
    public class Metric {
        private int id;
        private int[] properties;
        private string[] tags;

        public Metric(int id, int[] properties, string[] tags) {
            this.id = id;
            if (properties == null) throw new NullReferenceException("Metric's properties cannot be null");
            this.properties = properties;
            if (tags == null) throw new NullReferenceException("Metric's tags cannot be null");
            this.tags = tags;
        }

        public int[] getProperties() {
            return this.properties;
        }

        public string[] getTags() {
            return this.tags;
        }

        public int getId() {
            return this.id;
        }

        public string tostring() {
            return "Metric{" +
                   "id='" + id + '\'' +
                   ", properties=" + properties +
                   ", tags=" + tags +
                   '}';
        }
    }
}