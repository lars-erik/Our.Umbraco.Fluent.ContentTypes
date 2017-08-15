using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class TemplateConfigurator
    {
        private readonly FluentContentTypeConfiguration parent;
        private readonly TemplateConfiguration configuration;

        public TemplateConfiguration Configuration => configuration;

        public TemplateConfigurator(FluentContentTypeConfiguration parent, string @alias)
        {
            this.parent = parent;
            configuration = new TemplateConfiguration(alias);
        }

        public TemplateConfigurator Name(string name)
        {
            configuration.Name = name;
            return null;
        }
    }

    public class TemplateConfiguration
    {
        public string Alias { get; }

        public string Name { get; set; }

        public TemplateConfiguration(string @alias)
        {
            Alias = alias;
        }
    }
}
