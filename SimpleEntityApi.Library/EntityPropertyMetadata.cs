using System;
using System.ComponentModel;
using System.Data.Metadata.Edm;
using System.Reflection;

namespace SimpleEntityApi
{
    public class EntityPropertyMetadata
    {
        public EntityPropertyMetadata()
        {

        }
        public EntityPropertyMetadata(EdmProperty property, Type entityType)
        {

            this.Name = property.Name;
            this.Nullable = property.Nullable;
            this.Documentation = property.Documentation != null ? property.Documentation.LongDescription : null;
            this.TypeUsageName = property.TypeUsage.EdmType.Name;
            var displayName = entityType.GetProperty(property.Name).GetCustomAttribute<DisplayNameAttribute>();
            if (displayName != null) this.DisplayName = displayName.DisplayName;

        }

        public string DisplayName { get; set; }

        public bool Nullable { get; set; }        
        public string TypeUsageName { get; set; }      
        public string Name { get; set; }
        public string Documentation { get; set; }
    }
}