using System;

namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class NotAllowedNewValueException : Exception {
        private int propertyId;
        private int currentValue;
        private int newValue;

        public NotAllowedNewValueException(string message, int propertyId, int currentValue, int newValue) :
            base(message) {
            this.propertyId = propertyId;
            this.currentValue = currentValue;
            this.newValue = newValue;
        }

        public int getPropertyId() {
            return propertyId;
        }

        public int getCurrentValue() {
            return currentValue;
        }

        public int getNewValue() {
            return newValue;
        }
    }
}