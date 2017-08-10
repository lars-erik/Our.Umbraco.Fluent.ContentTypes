using System;
using System.Linq;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
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

        protected EntityDiffgram(TConfiguration configuration, ServiceContext serviceContext)
        {
            Configuration = configuration;
            ServiceContext = serviceContext;
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
            var modified = false;
            var targetProperties = typeof(TEntity).GetProperties();
            foreach (var property in typeof(TConfiguration).GetProperties())
            {
                var targetProp = targetProperties.FirstOrDefault(p => p.Name == property.Name);
                if (targetProp != null)
                { 
                    var sourceValue = property.GetValue(Configuration);
                    var targetValue = targetProp?.GetValue(Existing);
                    if (sourceValue == null)
                    {
                        modified |= targetValue != null;
                    }
                    else
                    {
                        modified |= !sourceValue.Equals(targetValue);
                    }
                }
            }

            IsModified = modified;
            IsUnsafe = IsModified;
        }
    }
}