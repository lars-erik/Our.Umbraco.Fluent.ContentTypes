using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class TemplateDiffgram : EntityDiffgram<TemplateConfiguration, ITemplate>
    {
        public TemplateDiffgram(Diffgram diffgram, TemplateConfiguration configuration, ServiceContext serviceContext) : base(configuration, serviceContext)
        {
        }

        public override string Key => Configuration.Alias;
        protected override ITemplate FindExisting()
        {
            return ServiceContext.FileService.GetTemplate(Configuration.Alias);
        }
    }
}