using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DependencyComparer : IComparer<DocumentTypeDiffgram>
    {
        public DependencyComparer()
        {
        }

        public int Compare(DocumentTypeDiffgram x, DocumentTypeDiffgram y)
        {
            if (x == y) return 0;
            if (x == null || y == null) throw new Exception("Can't sort null diffgrams");

            var yDependsOnX = y.DependsOn(x);
            var xDependsOnY = x.DependsOn(y);

            if (xDependsOnY && yDependsOnX)
                throw new Exception("This looks like a circular reference. :(");

            var retVal = 0;
            if (yDependsOnX)
            {
                retVal = -1;
            }
            else if (xDependsOnY)
            {
                retVal = 1;
            }
            else
            {
                var dependencyComparison = x.GetDependencies().Count().CompareTo(y.GetDependencies().Count());
                retVal = dependencyComparison;
            }

            return retVal;
        }

        public static IEnumerable<DocumentTypeDiffgram> OrderByDependencies(IEnumerable<DocumentTypeDiffgram> documentTypesValues)
        {
            var comparer = new DependencyComparer();
            var list = documentTypesValues.ToList();
            list.Sort(comparer);
            return list;
        }
    }
}
