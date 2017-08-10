using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
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

        protected override bool ValidateConfiguration()
        {
            return true;
        }

        protected override void CompareToExisting()
        {
            base.CompareToExisting();

            CompareCompositions();

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

        private void CompareCompositions()
        {
            var existingCompositions = Existing.ContentTypeComposition.Select(c => c.Alias).ToArray();
            var matchingCompositions = Configuration.Compositions.Intersect(existingCompositions);
            var newCompositions = Configuration.Compositions.Except(existingCompositions).ToArray();
            var validNewContentTypes = newCompositions
                .Select(alias => ServiceContext.ContentTypeService.GetContentType(alias)?.Alias)
                .Where(alias => alias != null)
                .Union(diffgram.DocumentTypes.Select(t => t.Key).Intersect(Configuration.Compositions))
                .Distinct()
                .ToArray();
            var invalidContentTypes = newCompositions.Except(validNewContentTypes);

            Comparisons.AddRange(matchingCompositions.Select(c => new Comparison("Compositions", c, ComparisonResult.Unchanged)));
            Comparisons.AddRange(validNewContentTypes.Select(c => new Comparison("Compositions", c, ComparisonResult.New)));
            Comparisons.AddRange(invalidContentTypes.Select(c => new Comparison("Compositions", c, ComparisonResult.Invalid)));

            IsModified |= Comparisons.Any(c => c.Key == "Compositions" && c.Result != ComparisonResult.Unchanged);
            IsUnsafe |= Comparisons.Any(c => c.Key == "Compositions" && c.Result == ComparisonResult.Invalid);
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