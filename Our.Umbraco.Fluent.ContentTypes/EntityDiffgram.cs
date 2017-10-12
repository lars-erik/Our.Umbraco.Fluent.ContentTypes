using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    [DebuggerDisplay("Diff of {Key}: IsNew:{IsNew}, IsModified:{IsModified}, IsUnsafe:{IsUnsafe}")]
    public abstract class EntityDiffgram<TConfiguration, TEntity>
    {
        protected readonly ServiceContext ServiceContext;
        public readonly TConfiguration Configuration;
        protected TEntity Existing;

        public bool IsNew { get; protected set; }
        public bool IsModified { get; protected set; }
        public bool IsUnsafe { get; protected set; }

        public abstract string Key { get; }

        public List<Comparison> Comparisons { get; private set; }

        protected EntityDiffgram(TConfiguration configuration, ServiceContext serviceContext)
        {
            Configuration = configuration;
            ServiceContext = serviceContext;
            Comparisons = new List<Comparison>();
        }

        protected abstract TEntity FindExisting();

        public void Compare()
        {
            Existing = FindExisting();
            IsNew = Existing == null;

            if (!ValidateConfiguration())
            {
                IsUnsafe = true; 
                return;
            }

            if (IsNew)
            {
                CreatePropertyComparisonsForNew();
            }
            else
            {
                CompareToExisting();
            }

            CompareChildren();
        }

        protected virtual void CompareChildren()
        {
        }

        protected virtual bool ValidateConfiguration()
        {
            return true;
        }

        protected virtual void CompareToExisting()
        {
            var modified = DeterminePropertiesModified();

            IsModified = modified;
            IsUnsafe = DetermineIsUnsafe();
        }

        private bool DeterminePropertiesModified()
        {
            var modified = false;
            var targetProperties = GetPublicProperties(typeof(TEntity));
            foreach (var property in typeof(TConfiguration).GetProperties())
            {
                var thisModified = true;
                var targetProps = targetProperties
                    .Where(p => p.Name == property.Name && p.PropertyType.IsAssignableFrom(property.PropertyType))
                    .ToArray();
                foreach(var targetProp in targetProps)
                { 
                    var sourceValue = property.GetValue(Configuration);
                    var targetValue = targetProp?.GetValue(Existing);

                    var untouched = false;
                    if (sourceValue is string)
                    {
                        untouched = (String.IsNullOrWhiteSpace(sourceValue as string)
                                    && String.IsNullOrWhiteSpace(targetValue as string))
                                    || (sourceValue != null
                                    &&  sourceValue.Equals(targetValue));
                    }
                    else
                    {
                        untouched = ((sourceValue != null && sourceValue.Equals(targetValue))
                                     || (sourceValue == null && targetValue == null));
                    }

                    if (untouched)
                    {
                        thisModified = false;
                        break;
                    }
                }
                if (targetProps.Any())
                { 
                    Comparisons.Add(new Comparison(property.Name, thisModified ? ComparisonResult.Modified : ComparisonResult.Unchanged));
                    modified |= thisModified;
                }
            }
            return modified;
        }

        private void CreatePropertyComparisonsForNew()
        {
            var targetProperties = GetPublicProperties(typeof(TEntity));
            foreach (var property in typeof(TConfiguration).GetProperties())
            {
                var targetProp = targetProperties.FirstOrDefault(p => p.Name == property.Name);
                if (targetProp != null && targetProp.PropertyType.IsAssignableFrom(property.PropertyType))
                    Comparisons.Add(new Comparison(property.Name, ComparisonResult.New));
            }
            IsModified = false;
            IsUnsafe = false;
        }

        protected virtual bool DetermineIsUnsafe()
        {
            return IsModified;
        }

        public static PropertyInfo[] GetPublicProperties(System.Type type)
        {
            // Naively copied from https://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<System.Type>();
                var queue = new Queue<System.Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance);
        }

        public void AssertSafe()
        {
            if (this.IsUnsafe)
                throw new Exception("Diffgram is unsafe");
        }
    }
}