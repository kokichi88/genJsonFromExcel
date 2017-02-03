using System;

namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class PropertyNotFoundException : Exception {
        public PropertyNotFoundException(string message) : base(message) {
        }
    }
}