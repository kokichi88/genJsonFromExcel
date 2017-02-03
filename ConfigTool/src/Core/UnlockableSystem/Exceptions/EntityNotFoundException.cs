using System;

namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class EntityNotFoundException : Exception {
        public EntityNotFoundException(string message) : base(message) {
        }
    }
}