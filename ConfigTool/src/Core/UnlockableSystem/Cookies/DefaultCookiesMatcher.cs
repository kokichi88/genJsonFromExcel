namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public class DefaultCookiesMatcher : CookiesMatcher {
        public bool Match(int propertyId, object[] propertyCookies, int metricId, object[] metricCookies) {
            //check dimensions
            if (propertyCookies.Length != metricCookies.Length) {
                return false;
            }
            bool isMatched = true;
            //check values
            for (int i = 0; i < propertyCookies.Length; i++) {
                isMatched &= propertyCookies[i].Equals(metricCookies[i]);
            }

            return isMatched;
        }
    }
}