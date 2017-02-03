namespace Assets.Scripts.Core.UnlockableSystem {
    public class Entity {
        private int id;
        private int[] properties;
        private bool unlocked;

        public Entity(int id, int[] properties, bool unlocked) {
            this.id = id;
            this.properties = properties;
            this.unlocked = unlocked;
        }

        public int Id {
            get { return id; }
        }

        public bool IsUnlocked() {
            return unlocked;
        }

        public void SetUnlocked(bool unlocked) {
            this.unlocked = unlocked;
        }

        public int[] Properties {
            get { return properties; }
        }

        public string toString() {
            return "Entity{" +
                   "id=" + id +
                   ", properties=" + properties +
                   ", unlocked=" + unlocked +
                   '}';
        }
    }
}