using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UnlockableSystem.Cookies;

namespace Assets.Scripts.Core.UnlockableSystem {
    public class UnlockableSystem {
        private CookiesMatcher cookiesMatcher;
        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private Dictionary<int, Property> properties = new Dictionary<int, Property>();
        private Dictionary<int, Metric> metrics = new Dictionary<int, Metric>();
        private Logger logger;

        public UnlockableSystem(Logger logger) {
            this.cookiesMatcher = new DefaultCookiesMatcher();
            this.logger = logger;
        }

        public Entity getEntityById(int entityId) {
            checkEntityExisted(entityId);
            return entities[entityId];
        }

        public Dictionary<int, Entity> getEntities() {
            return entities;
        }

        public Dictionary<int, Property> getProperties() {
            return properties;
        }

        public Property getPropertyById(int propertyId) {
            checkPropertyExisted(propertyId);
            return properties[propertyId];
        }

        public Metric getMetricById(int metricId) {
            checkMetricExisted(metricId);
            return metrics[metricId];
        }

        public int getPropertyValue(int propertyId) {
            return getPropertyById(propertyId).getValue();
        }

        public void setPropertyValue(int propertyId, int value) {
            getPropertyById(propertyId).setValue(value);
        }

        public void setCookiesMatcher(CookiesMatcher cookiesMatcher) {
            this.cookiesMatcher = cookiesMatcher;
        }

        public void updateMetric(int id, int value, object[] cookies) {
            List<Property> updatedProperties = findPropertiesByMetricId(id, cookies);

            for (int i = 0; i < updatedProperties.Count; i++) {
                int propertyId = updatedProperties[i].getId();
                addPropertyValue(propertyId, value);
            }
        }

        public void updateMetricWithPreferedProperties(int metricId, int value, object[] cookies,
                                                       List<int> preferredProperties) {
            List<Property> updatedProperties = findPropertiesByMetricId(metricId, cookies);

            for (int i = 0; i < updatedProperties.Count; i++) {
                int propertyId = updatedProperties[i].getId();
                if (preferredProperties.Contains(propertyId)) {
                    addPropertyValue(propertyId, value);
                }
            }
        }

        public List<Property> findPropertiesByMetricId(int metricId, object[] cookies) {
            return findPropertiesByMetricId(metricId, cookies, cookiesMatcher);
        }

        public List<Property> findPropertiesByMetricId(int metricId, object[] cookies, CookiesMatcher cookiesMatcher) {
            List<Property> foundProperties = new List<Property>();
            Metric m = getMetricById(metricId);
            int[] relatedProperties = m.getProperties();

            for (int i = 0; i < relatedProperties.Length; i++) {
                int propertyId = relatedProperties[i];
                if (!properties.ContainsKey(propertyId)) {
                    if (logger != null)
                        logger.info(string.Format("No property found for metric id '{0}' cookies '{1}'", metricId,
                            cookies.ToString()));
                    continue;
                }
                object[] propertyCookies = getPropertyById(propertyId).getCookies();
                if (cookiesMatcher.Match(propertyId, propertyCookies, metricId, cookies)) {
                    foundProperties.Add(properties[propertyId]);
                }
            }
            return foundProperties;
        }

        public Property[] resetProperties(string[] tags) {
            if (tags == null) throw new NullReferenceException("Tags used to reset cannot be null");

            List<Property> resetedProperties = new List<Property>();
            foreach (Property p in properties.Values) {
                if (hasTag(p, tags) /* && !p.isActive()*/) {
                    p.reset();
                    resetedProperties.Add(p);
                }
            }

            Property[] array = new Property[resetedProperties.Count];
            array = resetedProperties.ToArray();
            return array;
        }

        public void addPropertiesValue(int[] propertiesIds, int value) {
            for (int i = 0; i < propertiesIds.Length; i++) {
                checkPropertyExisted(propertiesIds[i]);
            }
            for (int i = 0; i < propertiesIds.Length; i++) {
                addPropertyValue(propertiesIds[i], value);
            }
        }

        public void addPropertyValue(int propertyId, int value) {
            Property property = getPropertyById(propertyId);
            int oldValue = property.getValue();
            int newValue = oldValue + value;
            if (!property.getActivationRule().IsNewValueAllowed(oldValue, newValue)) {
                throw new NotAllowedNewValueException(string.Format("Property id '{0}' value '{1}'", propertyId, value),
                    propertyId, oldValue, newValue);
            }
            property.setValue(newValue);
        }

        public Entity defineEntity(int id, int[] properties, bool unlocked) {
            Entity entity = new Entity(id, properties, unlocked);
            entities[id] = entity;
            return entity;
        }

        /*public Property defineProperty(int id, int value, ActivationRule activationRule, int activationValue,
                                       int initialValue, string[] tags, object[] cookies) {
            Property property = new DefaultProperty(id, value, activationRule, activationValue, initialValue, tags, cookies);
            properties[id] = property;
            return property;
        }*/

        public Property defineSyncedValueProperty(int id, SyncedValueProperty.Value value,
                                                  ActivationRule activationRule, int activationValue, int initialValue,
                                                  string[] tags, object[] cookies) {
            Property property =
                new SyncedValueProperty(id, value, activationRule, activationValue, initialValue, tags, cookies);
            properties[id] = property;
            return property;
        }

        public Metric defineMetric(int id, int[] properties, string[] tags) {
            Metric metric = new Metric(id, properties, tags);
            metrics[id] = metric;
            return metric;
        }

        public Entity checkEntityById(int entityId) {
            Entity entity = getEntityById(entityId);
            if (entity.IsUnlocked()) return entity;

            int activedPropertiesCount = 0;
            int[] propertiesIds = entity.Properties;
            for (int i = 0; i < propertiesIds.Length; i++) {
                Property property = getPropertyById(propertiesIds[i]);
                if (property.isActive()) {
                    activedPropertiesCount++;
                }
            }

            if (activedPropertiesCount == propertiesIds.Length) {
                entity.SetUnlocked(true);
            }
            return entity;
        }

        public Entity[] checkEntities() {
            List<Entity> newlyUnlockedEntities = new List<Entity>();

            foreach (Entity entity in entities.Values) {
                if (entity.IsUnlocked()) continue;

                int activedPropertiesCount = 0;
                int[] propertiesIds = entity.Properties;
                for (int i = 0; i < propertiesIds.Length; i++) {
                    Property property = getPropertyById(propertiesIds[i]);
                    if (property.isActive()) {
                        activedPropertiesCount++;
                    }
                }

                if (activedPropertiesCount == propertiesIds.Length) {
                    entity.SetUnlocked(true);
                    newlyUnlockedEntities.Add(entity);
                }
            }

            Entity[] array = new Entity[newlyUnlockedEntities.Count];
            array = newlyUnlockedEntities.ToArray();
            return array;
        }

        public void dump() {
            /*System.out.println("CookieMatcher: " + cookiesMatcher);
            System.out.println("\nMetrics: -------------------------");
            Iterator iter = metrics.values().iterator();
            while (iter.hasNext()) {
                object next = iter.next();
                System.out.println(next);
            }
            System.out.println("\nProperties------------------------");
            iter = properties.values().iterator();
            while (iter.hasNext()) {
                object next = iter.next();
                System.out.println(next);
            }
            System.out.println("\nEntities--------------------------");
            iter = entities.values().iterator();
            while (iter.hasNext()) {
                object next = iter.next();
                System.out.println(next);
            }*/
        }

        private void checkEntityExisted(int entityId) {
            Entity entity = entities[entityId];
            if (entity == null) {
                throw new EntityNotFoundException("Entity id " + entityId);
            }
        }

        private void checkPropertyExisted(int propertyId) {
            Property property = properties[propertyId];
            if (property == null) {
                throw new PropertyNotFoundException("Property id " + propertyId);
            }
        }

        private void checkMetricExisted(int metricId) {
            Metric metric = metrics[metricId];
            if (metric == null) {
                throw new MetricNotFoundException("Metric id " + metricId);
            }
        }

        private bool hasTag(Property property, string[] tags) {
            bool hasTag = false;

            string[] propertyTags = property.getTags();
            for (int i = 0; i < tags.Length; i++) {
                string tag = tags[i];

                for (int k = 0; k < propertyTags.Length; k++) {
                    if (propertyTags[k].Equals(tag)) {
                        hasTag = true;
                        break;
                    }
                }
            }

            return hasTag;
        }
    }
}