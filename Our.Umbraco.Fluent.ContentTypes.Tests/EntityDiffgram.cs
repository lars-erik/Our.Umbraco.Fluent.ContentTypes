using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public abstract class EntityDiffgram<TConfiguration, TEntity>
    {
        protected readonly ServiceContext ServiceContext;
        protected readonly TConfiguration Configuration;
        protected readonly IConfigurator<TConfiguration> Configurator;
        protected TEntity Existing;

        public bool IsNew { get; protected set; }
        public bool IsModified { get; protected set; }
        public bool IsUnsafe { get; protected set; }

        public abstract string Key { get; }

        protected EntityDiffgram(IConfigurator<TConfiguration> configurator, ServiceContext serviceContext)
        {
            Configurator = configurator;
            Configuration = configurator.Configuration;
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

            if (!IsNew)
                CompareToExisting();

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
            IsModified = false;
            IsUnsafe = false;
        }
    }
}