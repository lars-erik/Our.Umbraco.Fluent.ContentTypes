using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class DocumentTypeDiffgram
    {
        private readonly IContentTypeService contentTypeService;
        private readonly DocumentTypeConfigurator configurator;
        private readonly ServiceContext serviceContext;
        private DocumentTypeConfiguration configuration;
        public string Alias => configurator.Alias;
        public bool IsNew { get; set; }
        public Dictionary<string, TabDiffgram> Tabs { get; }

        public DocumentTypeDiffgram(DocumentTypeConfigurator configurator, ServiceContext serviceContext)
        {
            this.contentTypeService = serviceContext.ContentTypeService;
            this.configurator = configurator;
            this.configuration = configurator.Configuration;
            this.serviceContext = serviceContext;
            Tabs = new Dictionary<string, TabDiffgram>();
        }

        public void Compare()
        {
            var umbContentType = contentTypeService.GetContentType(configurator.Alias);

            IsNew = umbContentType == null;

            foreach (var tab in configurator.Tabs.Values)
            {
                var tabDiff = new TabDiffgram(tab, umbContentType?.PropertyGroups ?? new PropertyGroupCollection(new PropertyGroup[0]), serviceContext);
                Tabs.Add(tabDiff.Name, tabDiff);
                tabDiff.Compare();
            }
        }
    }
}