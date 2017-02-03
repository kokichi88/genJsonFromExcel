namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public interface ActivationRule {
        bool IsActive(int propertyValue, int activationValue);
        bool IsNewValueAllowed(int currentValue, int newValue);
    }
}