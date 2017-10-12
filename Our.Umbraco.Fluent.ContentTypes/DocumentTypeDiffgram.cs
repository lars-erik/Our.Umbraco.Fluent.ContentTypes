using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DocumentTypeDiffgram : EntityDiffgram<DocumentTypeConfiguration, IContentType>
    {
        private readonly Diffgram diffgram;
        //private readonly DocumentTypeConfigurator configurator;
        private readonly IContentTypeService contentTypeService;

        public override string Key => Configuration.Alias;
        public Dictionary<string, TabDiffgram> Tabs { get; }

        public DocumentTypeDiffgram(Diffgram diffgram, DocumentTypeConfiguration configuration, ServiceContext serviceContext)
            : base(configuration, serviceContext)
        {
            this.diffgram = diffgram;
            contentTypeService = serviceContext.ContentTypeService;
            Tabs = new Dictionary<string, TabDiffgram>();
        }

        public IEnumerable<string> GetDependencies(int currentLevel = 0)
        {
            if (currentLevel > 50)
                throw new Exception("Seems like we've hit a rift in time-space continuum. (Circular reference)");

            var theseDependencies = Configuration.AllowedChildren.Union(Configuration.Compositions);
            if (Configuration.Parent != null)
                theseDependencies = theseDependencies.Union(new[] {Configuration.Parent});
            var nextLevel = diffgram.DocumentTypes.Where(kvp => theseDependencies.Contains(kvp.Key)).SelectMany(kvp => kvp.Value.GetDependencies(currentLevel + 1));
            var all = theseDependencies.Union(nextLevel).Distinct();
            return all;
        }

        protected override bool ValidateConfiguration()
        {
            return true;
        }

        protected override void CompareToExisting()
        {
            base.CompareToExisting();

            CompareCompositions();
            CompareParent();
            CompareAllowedChildren();
            CompareAllowedTemplates();
        }

        private void CompareAllowedChildren()
        {
            var extChildren = Existing.AllowedContentTypes.Select(t => t.Alias).ToArray();
            var newChildren = Configuration.AllowedChildren.Except(extChildren).ToArray();

            var validNewChildren = newChildren
                .Where(c => ServiceContext.ContentTypeService.GetContentType(c) != null)
                .Union(newChildren.Where(c => diffgram.DocumentTypes.ContainsKey(c)))
                .ToArray();
            var invalidNewChildren = newChildren.Except(validNewChildren);

            Comparisons.AddRange(validNewChildren.Select(c => new Comparison("AllowedChildren", c, ComparisonResult.New)));
            Comparisons.AddRange(newChildren.Intersect(extChildren)
                .Select(c => new Comparison("AllowedChildren", c, ComparisonResult.Unchanged)));
            Comparisons.AddRange(invalidNewChildren.Select(c => new Comparison("AllowedChildren", c, ComparisonResult.Invalid)));

            IsModified |= Comparisons.Any(c => c.Key == "AllowedChildren" && c.Result != ComparisonResult.Unchanged);
            IsUnsafe |= Comparisons.Any(c => c.Key == "AllowedChildren" && c.Result == ComparisonResult.Invalid);
        }

        private void CompareParent()
        {
            var extParent = Existing.ParentId > 0
                ? ServiceContext.ContentTypeService.GetContentType(Existing.ParentId)
                : null;

            var newParent = !String.IsNullOrEmpty(Configuration.Parent)
                ? ServiceContext.ContentTypeService.GetContentType(Configuration.Parent)
                : null;

            if (extParent == null && newParent == null)
            {
                if (diffgram.DocumentTypes.Any(t => t.Key == Configuration.Parent))
                {
                    Comparisons.Add(new Comparison("Parent", ComparisonResult.New));
                    IsModified = true;
                }
                else
                {
                    Comparisons.Add(new Comparison("Parent", ComparisonResult.Unchanged));
                }
            }
            else
            {
                if (extParent == null)
                {
                    Comparisons.Add(new Comparison("Parent", ComparisonResult.New));
                    IsModified = true;
                }
                else if (extParent.Alias != newParent?.Alias)
                {
                    Comparisons.Add(new Comparison("Parent", ComparisonResult.Invalid));
                    IsModified = true;
                    IsUnsafe = true;
                }
                else
                {
                    Comparisons.Add(new Comparison("Parent", ComparisonResult.Unchanged));
                }
            }
        }

        // TODO: Generalize string list comparisons
        private void CompareCompositions()
        {
            var existingCompositions = Existing.ContentTypeComposition.Select(c => c.Alias).ToArray();
            var matchingCompositions = Configuration.Compositions.Intersect(existingCompositions).ToArray();
            var newCompositions = Configuration.Compositions.Except(existingCompositions).ToArray();
            var validNewContentTypes = newCompositions
                .Select(alias => ServiceContext.ContentTypeService.GetContentType(alias)?.Alias)
                .Where(alias => alias != null)
                .Union(diffgram.DocumentTypes.Select(t => t.Key).Intersect(Configuration.Compositions))
                .Distinct()
                .Except(matchingCompositions)
                .ToArray();
            var invalidContentTypes = newCompositions.Except(validNewContentTypes);

            Comparisons.AddRange(matchingCompositions.Select(c => new Comparison("Compositions", c, ComparisonResult.Unchanged)));
            Comparisons.AddRange(validNewContentTypes.Select(c => new Comparison("Compositions", c, ComparisonResult.New)));
            Comparisons.AddRange(invalidContentTypes.Select(c => new Comparison("Compositions", c, ComparisonResult.Invalid)));

            IsModified |= Comparisons.Any(c => c.Key == "Compositions" && c.Result != ComparisonResult.Unchanged);
            IsUnsafe |= Comparisons.Any(c => c.Key == "Compositions" && c.Result == ComparisonResult.Invalid);
        }

        private void CompareAllowedTemplates()
        {
            var existingTemplates = Existing.AllowedTemplates.Select(t => t.Alias).ToArray();
            var matchingTemplates = Configuration.AllowedTemplates.Intersect(existingTemplates).ToArray();
            var newTemplates = Configuration.AllowedTemplates.Except(existingTemplates).ToArray();
            var validNewTemplates = newTemplates
                .Select(alias => ServiceContext.FileService.GetTemplate(alias)?.Alias)
                .Where(alias => alias != null)
                .Union(diffgram.Templates.Select(t => t.Key).Intersect(Configuration.AllowedTemplates))
                .Distinct()
                // TODO: Test both configured and existing dependency, also for compositions.
                .Except(matchingTemplates)
                .ToArray();
            var invalidTemplates = newTemplates.Except(validNewTemplates);

            Comparisons.AddRange(matchingTemplates.Select(c => new Comparison("AllowedTemplates", c, ComparisonResult.Unchanged)));
            Comparisons.AddRange(validNewTemplates.Select(c => new Comparison("AllowedTemplates", c, ComparisonResult.New)));
            Comparisons.AddRange(invalidTemplates.Select(c => new Comparison("AllowedTemplates", c, ComparisonResult.Invalid)));

            IsModified |= Comparisons.Any(c => c.Key == "AllowedTemplates" && c.Result != ComparisonResult.Unchanged);
            IsUnsafe |= Comparisons.Any(c => c.Key == "AllowedTemplates" && c.Result == ComparisonResult.Invalid);
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

        public bool DependsOn(DocumentTypeDiffgram y)
        {
            var dependencies = GetDependencies().ToArray();
            var dependsOn = dependencies.Contains(y.Key);
            return dependsOn;
        }

        public void Ensure()
        {
            var ensurer = new DocumentTypeEnsurer(this, ServiceContext);
            ensurer.Ensure();
        }
    }

    public class DocumentTypeEnsurer
    {
        private readonly DocumentTypeDiffgram diff;
        private readonly ServiceContext serviceContext;

        public DocumentTypeEnsurer(DocumentTypeDiffgram diff, ServiceContext serviceContext)
        {
            this.diff = diff;
            this.serviceContext = serviceContext;
        }

        public void Ensure()
        {
            if (diff.IsUnsafe)
                throw new Exception("Won't ensure unsafe diffgram");

            var config = diff.Configuration;

            var contentTypeService = serviceContext.ContentTypeService;
            var dataTypeService = serviceContext.DataTypeService;

            IContentType docType;
            if (diff.IsNew)
            {
                var parent = config.Parent != null
                    ? contentTypeService.GetContentType(config.Parent)
                    : null;

                // TODO: Automap?
                if (parent != null)
                    docType = new ContentType(parent, config.Alias);
                else
                    docType = new ContentType(-1)
                    {
                        Alias = config.Alias
                    };
                docType.Name = config.Name;
                docType.Description = config.Description;
                docType.Icon = config.Icon;
                docType.AllowedAsRoot = config.AllowedAsRoot;
            }
            else
            {
                docType = contentTypeService.GetContentType(config.Alias);
            }

            if (diff.IsNew || diff.IsModified)
            {
                docType.AllowedTemplates = docType.AllowedTemplates.Union(
                    config.AllowedTemplates
                        .Except(docType.AllowedTemplates.Select(t => t.Alias))
                        .Select(alias => serviceContext.FileService.GetTemplate(alias))
                );

                var extAllowedChildren = docType.AllowedContentTypes.Select(t => t.Alias);
                var newAllowedChildren = config.AllowedChildren.Except(extAllowedChildren)
                    .Select(a => contentTypeService.GetContentType(a))
                    .Select(t => new ContentTypeSort(new Lazy<int>(() => t.Id), 0, t.Name));
                docType.AllowedContentTypes = docType
                    .AllowedContentTypes
                    .Union(newAllowedChildren);

                var compositions = config.Compositions
                    .Except(docType.CompositionAliases())
                    .Select(alias => contentTypeService.GetContentType(alias));

                foreach (var composition in compositions)
                    docType.AddContentType(composition);

                foreach (var groupDiff in diff.Tabs.Values)
                {
                    if (groupDiff.IsNew)
                    {
                        docType.AddPropertyGroup(groupDiff.Key);
                    }

                    var group = docType.PropertyGroups[groupDiff.Key];

                    foreach (var propDiff in groupDiff.Properties.Values)
                    {
                        var propConfig = propDiff.Configuration;
                        if (propDiff.IsNew)
                        {
                            var dataType = dataTypeService.GetDataTypeDefinitionByName(propConfig.DataType);
                            var propertyType = new PropertyType(dataType, propConfig.Alias)
                            {
                                Name = propConfig.Name,
                                Description = propConfig.Description,
                                DataTypeDefinitionId = dataType.Id
                            };
                            group.PropertyTypes.Add(propertyType);
                        }
                    }
                }
            }

            contentTypeService.Save(docType);
        }
    }
}