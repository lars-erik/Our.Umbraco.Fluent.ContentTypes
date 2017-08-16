using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class TemplateEnsurer
    {
        private readonly TemplateDiffgram templateDiffgram;
        private readonly ServiceContext serviceContext;

        public TemplateEnsurer(TemplateDiffgram templateDiffgram, ServiceContext serviceContext)
        {
            this.templateDiffgram = templateDiffgram;
            this.serviceContext = serviceContext;
        }

        public void Ensure()
        {
            templateDiffgram.AssertSafe();

            if (templateDiffgram.IsNew)
            {
                serviceContext.FileService.SaveTemplate(new Template(templateDiffgram.Configuration.Name, templateDiffgram.Key));
            }
        }
    }
}