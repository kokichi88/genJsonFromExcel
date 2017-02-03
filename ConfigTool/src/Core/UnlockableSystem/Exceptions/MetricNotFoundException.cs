using System;

namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class MetricNotFoundException : Exception {
        public MetricNotFoundException(string message) : base(message) {
        }
    }
}