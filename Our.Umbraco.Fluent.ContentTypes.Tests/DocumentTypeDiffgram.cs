using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class DocumentTypeDiffgram
    {
        private readonly IContentTypeService contentTypeService;
        private readonly DocumentTypeConfigurator configurator;
        public string Alias => configurator.Alias;
        public bool IsNew { get; set; }
        public Dictionary<string, TabDiffgram> Tabs { get; }

        public DocumentTypeDiffgram(DocumentTypeConfigurator configurator, IContentTypeService contentTypeService)
        {
            this.contentTypeService = contentTypeService;
            this.configurator = configurator;
            Tabs = new Dictionary<string, TabDiffgram>();
        }

        public void Compare()
        {
            var umbContentType = contentTypeService.GetContentType(configurator.Alias);

            IsNew = umbContentType == null;

            foreach (var tab in configurator.Tabs.Values)
            {
                var tabDiff = new TabDiffgram(tab, umbContentType.PropertyGroups);
                Tabs.Add(tabDiff.Name, tabDiff);
                tabDiff.Compare();
            }
        }
    }
}