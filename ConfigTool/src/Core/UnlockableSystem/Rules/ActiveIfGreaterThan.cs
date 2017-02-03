namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class ActiveIfGreaterThan : ActivationRule {
        public bool IsActive(int propertyValue, int activationValue) {
            return propertyValue > activationValue;
        }

        public bool IsNewValueAllowed(int currentValue, int newValue) {
            return newValue >= currentValue;
        }

        public string toString() {
            return "ActiveIfGreaterThan{}";
        }
    }
}