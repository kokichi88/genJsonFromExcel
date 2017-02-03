using System;
using Assets.Scripts.Core.UnlockableSystem.Cookies;

namespace Assets.Scripts.Core.UnlockableSystem {
    public class SyncedValueProperty : Property {
        private int id;
        private Value syncedValue;
        private ActivationRule activationRule;
        private int activationValue;
        private int initialValue;
        private string[] tags;
        private object[] cookies;

        public SyncedValueProperty(int id, Value syncedValue, ActivationRule activationRule, int activationValue,
                                   int initialValue, string[] tags, object[] cookies) {
            this.id = id;
            if (syncedValue == null) throw new NullReferenceException("Synced value cannot be null");
            this.syncedValue = syncedValue;
            if (activationRule == null) throw new NullReferenceException("Activation rule cannot be null");
            this.activationRule = activationRule;
            this.activationValue = activationValue;
            this.initialValue = initialValue;
            if (tags == null) throw new NullReferenceException("Tags cannot be null");
            this.tags = tags;
            if (cookies == null) throw new NullReferenceException("Cookies cannot be null");
            this.cookies = cookies;
        }

        public int getId() {
            return id;
        }

        public bool isActive() {
            return activationRule.IsActive(syncedValue.get(), activationValue);
        }

        public int getValue() {
            return syncedValue.get();
        }

        public void setValue(int value) {
            syncedValue.set(value);
        }

        public string[] getTags() {
            return tags;
        }

        public object[] getCookies() {
            return cookies;
        }

        public void reset() {
            syncedValue.set(initialValue);
        }

        public ActivationRule getActivationRule() {
            return activationRule;
        }

        public string tostring() {
            return "Property{" +
                   "id=" + id +
                   ", syncedValue=" + syncedValue +
                   ", activationRule=" + activationRule +
                   ", activationValue=" + activationValue +
                   ", initialValue=" + initialValue +
                   ", tags=" + tags +
                   ", cookies=" + cookies +
                   '}';
        }

        public interface Value {
            int get();

            void set(int value);
        }
    }
}