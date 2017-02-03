namespace Assets.Scripts.Core.UnlockableSystem.Cookies {
    public interface CookiesMatcher {
        bool Match(int propertyId, object[] propertyCookies, int metricId, object[] metricCookies);
    }
}