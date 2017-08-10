using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class DocumentTypeDiffgram : EntityDiffgram<DocumentTypeConfiguration, IContentType>
    {
        //private readonly DocumentTypeConfigurator configurator;
        private readonly IContentTypeService contentTypeService;

        public override string Key => Configuration.Alias;
        public Dictionary<string, TabDiffgram> Tabs { get; }

        public DocumentTypeDiffgram(DocumentTypeConfiguration configuration, ServiceContext serviceContext)
            : base(configuration, serviceContext)
        {
            contentTypeService = serviceContext.ContentTypeService;
            Tabs = new Dictionary<string, TabDiffgram>();
        }

        protected override bool ValidateConfiguration()
        {
            return true;
        }

        protected override void CompareChildren()
        {
            foreach (var tab in Configuration.Tabs.Values)
            {
                var tabDiff = new TabDiffgram(tab, Existing?.PropertyGroups, ServiceContext);
                Tabs.Add(tabDiff.Key, tabDiff);
                tabDiff.Compare();
            }
        }

        protected override IContentType FindExisting()
        {
            return contentTypeService.GetContentType(Configuration.Alias);
        }
    }
}