using Assets.Scripts.Core.UnlockableSystem.Cookies;

namespace Assets.Scripts.Core.UnlockableSystem {
    public interface Property {
        int getId();

        bool isActive();

        int getValue();

        void setValue(int value);

        string[] getTags();

        object[] getCookies();

        void reset();

        ActivationRule getActivationRule();
    }
}