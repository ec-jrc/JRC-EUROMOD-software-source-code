using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    /// <summary> class providing for keeping actions, filters and variables local, !*!*!*! UNDER CONSTRUCTION !*!*!*! </summary>
    public class LocalMap
    {
        private readonly string pageId = null;
        private readonly string tableId = null;

        public LocalMap(string _pageId, string _tableId = null) { pageId = _pageId; tableId = _tableId; }

        internal static bool AllowsFor(LocalMap localMapVar, LocalMap localMapRequestor)
        {
            if (localMapVar == null) return true; // global variables are available for any requestor
            if (localMapRequestor == null) return false; // global requester has no access to any local variables

            // page variables are available for page-actions, page-filters and table-elements (actions, filters, cellActions) of the matching page
            if (localMapVar.tableId == null) return localMapRequestor.pageId == localMapVar.pageId;
            // table variables are available for table-actions, table-filters and table-cellActions of the matching table
            else return localMapRequestor.pageId == localMapVar.pageId && localMapRequestor.tableId == localMapVar.tableId;
        }

        internal static LocalMap NewPageLocalMap() { return new LocalMap(Guid.NewGuid().ToString(), null); }
        internal static LocalMap NewTableLocalMap(LocalMap pageLocalMap) { return new LocalMap(pageLocalMap.pageId, Guid.NewGuid().ToString()); }

        internal string GetPageId() { return pageId; }

        internal static Template.Filter GetNamedFilter(Template template, string filterName, LocalMap localMap = null)
        {
            List<Template.Filter> matches = GetNamedFilters(template, filterName, localMap);
            return matches.Any() ? matches.First() : null;
        }

        internal static List<Template.Filter> GetNamedFilters(Template template, string filterName, LocalMap localMapRequestor)
        {
            List<Template.Filter> matches = new List<Template.Filter>();
            
            if (string.IsNullOrEmpty(filterName)) return matches;

            // global filters are available for any requestor
            Template.Filter filter = template.globalFilters.FirstOrDefault(f => f.name.ToLower() == filterName.ToLower());
            if (filter != null) matches.Add(filter);

            if (localMapRequestor != null) // request from page- or table-action which can, in addition, use their own filters
            {
                foreach (Template.Page page in from p in template.pages
                                               where p.active && localMapRequestor.pageId == p.localMap.pageId select p)
                {
                    filter = page.filters.FirstOrDefault(f => f.name.ToLower() == filterName.ToLower());
                    if (filter != null) matches.Insert(0, filter); // make sure lowest level is always first in list

                    if (!string.IsNullOrEmpty(localMapRequestor.tableId)) // request from table-action
                    {
                        foreach (Template.Page.Table table in from t in page.tables
                                                              where t.active && localMapRequestor.tableId == t.localMap.tableId select t)
                        {
                            filter = table.filters.FirstOrDefault(f => f.name.ToLower() == filterName.ToLower());
                            if (filter != null) matches.Insert(0, filter); // make sure lowest level is always first in list
                        }
                    }
                }
            }
            return matches;
        }
    }
}
