using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public abstract class EntityDiffgram<TConfiguration, TEntity>
    {
        protected readonly ServiceContext ServiceContext;
        protected readonly TConfiguration Configuration;
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
                IsModified = false;
                IsUnsafe = false;
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
            var targetProperties = typeof(TEntity).GetProperties();
            foreach (var property in typeof(TConfiguration).GetProperties())
            {
                var thisModified = false;
                var targetProp = targetProperties.FirstOrDefault(p => p.Name == property.Name);
                if (targetProp != null)
                {
                    var sourceValue = property.GetValue(Configuration);
                    var targetValue = targetProp?.GetValue(Existing);
                    if (sourceValue == null)
                        thisModified = targetValue != null;
                    else
                        thisModified = !sourceValue.Equals(targetValue);

                    Comparisons.Add(new Comparison(property.Name, thisModified ? ComparisonResult.Modified : ComparisonResult.Unchanged));
                }

                modified |= thisModified;
            }
            return modified;
        }

        protected virtual bool DetermineIsUnsafe()
        {
            return IsModified;
        }
    }
}