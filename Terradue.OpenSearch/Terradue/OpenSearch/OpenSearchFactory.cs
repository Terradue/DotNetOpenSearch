//
//  OpenSearchFactory.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;
using System.Security;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Request;
using System.Threading;
using System.Web;

namespace Terradue.OpenSearch
{
    /// <summary>
    /// OpenSearch factory. Helper class with a set of static class with the most common manipulation
    /// functions for OpenSearch.
    /// </summary>
    public partial class OpenSearchFactory
    {


        private static log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Loads the OpenSearch description document.
        /// </summary>
        /// <returns>The OpenSearch description document.</returns>
        /// <param name="url">URL.</param>
        public static OpenSearchDescription ReadOpenSearchDescriptionDocument(IOpenSearchResponse response)
        {

            OpenSearchDescription osd = null;
            if (response.ObjectType != typeof(byte[]))
                throw new InvalidOperationException("The OpenSearch Description document did not return byte[] body");

            XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
            Stream stream = new MemoryStream((byte[])response.GetResponseObject());
            osd = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(stream));
            stream.Flush();
            stream.Close();
            // Add the root namespaces to the url
            foreach (var url in osd.Url)
                url.ExtraNamespace = osd.ExtraNamespace;
            return osd;
        }

        /// <summary>
        /// Builds the request URL from OpenSearch template URL
        /// </summary>
        /// <returns>The request URL to make the query</returns>
        /// <param name="osUrlTemplate">OpenSearch URL template.</param>
        /// <param name="searchParameters">Search parameters definition</param>
        /// <param name="querySettings">Query settings</param>
        public static OpenSearchUrl BuildRequestUrlFromTemplate(OpenSearchDescriptionUrl osUrlTemplate, NameValueCollection searchParameters, QuerySettings querySettings)
        {

            string finalUrl = osUrlTemplate.Template;

            NameValueCollection additionalParameters = new NameValueCollection();

            // For each parameter defined
            foreach (string parameter_key in searchParameters.AllKeys)
            {
                // skip if null or empty
                if (string.IsNullOrEmpty(parameter_key))
                    continue;

                string key = parameter_key;
                string value = searchParameters[parameter_key];
                Regex findParam = null;
                string replacement = null;


                // CASE #1 Keyword parameter name (e.g. 'count')
                // Is the parameter a reserved keyword?
                if (querySettings.ParametersKeywordsTable.ContainsKey(parameter_key))
                {
                    // YES!
                    // Then substitue the key with the FQDN
                    key = querySettings.ParametersKeywordsTable[parameter_key];
                }

                // CASE #2 : Fully Qualified Parameter Name (e.g. '{http://a9.com/-/opensearch/extensions/geo/1.0/}box' )
                // Regular expression finder
                // Is it a FQDN ?
                Match matchFQDN = Regex.Match(key, @"^{(?'namespace'[^}]+)}(?'name'.+)$");
                // YES!
                if (matchFQDN.Success)
                {
                    string ns = matchFQDN.Groups["namespace"].Value;
                    string name = matchFQDN.Groups["name"].Value;
                    // Find the prefix declaration for the namespace in the OSD namespaces declaration
                    XmlQualifiedName qPrefix = osUrlTemplate.ExtraNamespace.ToArray().LastOrDefault(n => n.Namespace == ns);
                    if (qPrefix == null)
                        qPrefix = osUrlTemplate.OsdExtraNamespace.ToArray().LastOrDefault(n => n.Namespace == ns);
                    // Namespace not found? Parameter is then skipped!
                    if (qPrefix == null)
                    {
                        log.WarnFormat("Parameter '{0}' has a namespace '{1}' not declared in OSD. Skipping parameter.", name, ns);
                        continue;
                    }
                    // Replacement Regular Expression using the prefix
                    findParam = new Regex(string.Format("{{{0}{1}\\??}}", string.IsNullOrEmpty(qPrefix.Name) ? "" : qPrefix.Name + ":", name));
                    replacement = HttpUtility.UrlEncode(value);
                }

                // CASE #3 Prefixed Parameter Name (e.g. 'geo:box' )
                // Regular expression finder
                // Is it a prefixed param ?
                Match matchPrefixed = Regex.Match(key, @"^(?'prefix'[a-zA-Z0-9_\-]+):(?'name'[^&]+)$");
                // YES!
                if (matchPrefixed.Success)
                {
                    string prefix = matchPrefixed.Groups["prefix"].Value;
                    string name = matchPrefixed.Groups["name"].Value;

                    // check that prefix exist in the OS declaration
                    if (!osUrlTemplate.ExtraNamespace.ToArray().Any(n => n.Name == prefix) && !querySettings.ForceUnspecifiedParameters)
                    {
                        log.WarnFormat("Parameter '{0}' prefixed '{1}' not declared in OSD. Skipping parameter.", name, prefix);
                        continue;
                    }
                    // Replacement Regular Expression using the prefix
                    findParam = new Regex(string.Format("{{{0}:{1}\\??}}", prefix, name));
                    replacement = HttpUtility.UrlEncode(value);
                }

                // Let's replace the non ambiguous if found
                if (findParam != null)
                {
                    finalUrl = findParam.Replace(finalUrl, replacement);
                }
                // force parameter anyway if forcing is specified
                else if (querySettings.ForceUnspecifiedParameters)
                {
                    log.DebugFormat("Forcing parameter '{0}'.", parameter_key);
                    additionalParameters.Set(parameter_key, searchParameters[parameter_key]);
                }

            }

            // Clean the remaining parameters templates
            Regex cleanParam = new Regex(string.Format("{{(?:(?'prefix'[^:?]+):)?(?:(?'name'[^&?]+)\\??)}}"));
            finalUrl = cleanParam.Replace(finalUrl, "");

            UriBuilder finalUri = new UriBuilder(finalUrl);

            if (additionalParameters.Count > 0)
            {
                var finalQueryParameters = HttpUtility.ParseQueryString(finalUri.Query);
                foreach (var key in additionalParameters.AllKeys)
                    finalQueryParameters.Set(key, additionalParameters[key]);
                string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, HttpUtility.UrlEncode(finalQueryParameters[key])));
                finalUri.Query = string.Join("&", queryString);
            }

            return new OpenSearchUrl(finalUri.Uri);

        }

        public static NameValueCollection BuildFqdnParameterFromTemplate(OpenSearchDescriptionUrl osUrlTemplate, NameValueCollection searchParameters, QuerySettings querySettings)
        {

            NameValueCollection fqdnParameters = new NameValueCollection();

            // For each parameter defined
            foreach (string parameter_key in searchParameters.AllKeys)
            {
                // skip if null or empty
                if (string.IsNullOrEmpty(parameter_key))
                    continue;

                string key = parameter_key;
                string value = searchParameters[parameter_key];
                Regex findParam = null;
                string ns = null;
                string name = null;

                // CASE #1 Keyword parameter name (e.g. 'count')
                // Is the parameter a reserved keyword?
                if (querySettings.ParametersKeywordsTable.ContainsKey(parameter_key))
                {
                    // YES!
                    // Then substitue the key with the FQDN
                    key = querySettings.ParametersKeywordsTable[parameter_key];
                }

                // CASE #2 : Fully Qualified Parameter Name (e.g. '{http://a9.com/-/opensearch/extensions/geo/1.0/}box' )
                // Regular expression finder
                // Is it a FQDN ?
                Match matchFQDN = Regex.Match(key, @"^{(?'namespace'[^}]+)}(?'name'.+)$");
                // YES!
                if (matchFQDN.Success)
                {
                    ns = matchFQDN.Groups["namespace"].Value;
                    name = matchFQDN.Groups["name"].Value;
                    // Find the prefix declaration for the namespace in the OSD namespaces declaration
                    XmlQualifiedName qPrefix = osUrlTemplate.ExtraNamespace.ToArray().LastOrDefault(n => n.Namespace == ns);
                    if (qPrefix == null)
                        qPrefix = osUrlTemplate.OsdExtraNamespace.ToArray().LastOrDefault(n => n.Namespace == ns);
                    // Namespace not found? Parameter is then skipped!
                    if (qPrefix == null)
                    {
                        log.WarnFormat("Parameter '{0}' has a namespace '{1}' not declared in OSD. Skipping parameter.", name, ns);
                        continue;
                    }
                    // Replacement Regular Expression using the prefix
                    findParam = new Regex(string.Format("{{{0}{1}\\??}}", string.IsNullOrEmpty(qPrefix.Name) ? "" : qPrefix.Name + ":", name));
                }

                // CASE #3 Prefixed Parameter Name (e.g. 'geo:box' )
                // Regular expression finder
                // Is it a prefixed param ?
                Match matchPrefixed = Regex.Match(key, @"^(?'prefix'[a-zA-Z0-9_\-]+):(?'name'[^&]+)$");
                // YES!
                if (matchPrefixed.Success)
                {
                    string prefix = matchPrefixed.Groups["prefix"].Value;
                    name = matchPrefixed.Groups["name"].Value;

                    var xmlns = osUrlTemplate.ExtraNamespace.ToArray().FirstOrDefault(n => n.Name == prefix);

                    // check that prefix exist in the OS declaration
                    if (ns == null && !querySettings.ForceUnspecifiedParameters)
                    {
                        log.WarnFormat("Parameter '{0}' prefixed '{1}' not declared in OSD. Skipping parameter.", name, prefix);
                        continue;
                    }
                    else
                    {
                        ns = xmlns.Namespace;
                    }
                    // Replacement Regular Expression using the prefix
                    findParam = new Regex(string.Format("{{{0}:{1}\\??}}", prefix, name));
                }

                // Let's replace the non ambiguous if found
                if (findParam != null && findParam.IsMatch(osUrlTemplate.Template))
                {
                    fqdnParameters.Set(string.Format("{{{0}}}{1}", ns, name), value);
                }
                // force parameter anyway if forcing is specified
                else if (querySettings.ForceUnspecifiedParameters)
                {
                    log.DebugFormat("Forcing parameter '{0}'.", key);
                    fqdnParameters.Set(parameter_key, value);
                }

            }

            return fqdnParameters;

        }

        /// <summary>
        /// Gets the base open search parameters keywords table.
        /// </summary>
        /// <returns>The base open search parameters keywords table.</returns>
        public static Dictionary<string, string> GetBaseOpenSearchParametersKeywordsTable()
        {
            Dictionary<string, string> table = new Dictionary<string, string>();
            table.Add("count", "{http://a9.com/-/spec/opensearch/1.1/}count");
            table.Add("startPage", "{http://a9.com/-/spec/opensearch/1.1/}startPage");
            table.Add("startIndex", "{http://a9.com/-/spec/opensearch/1.1/}startIndex");
            table.Add("q", "{http://a9.com/-/spec/opensearch/1.1/}searchTerms");
            table.Add("searchTerms", "{http://a9.com/-/spec/opensearch/1.1/}searchTerms");
            table.Add("lang", "{http://a9.com/-/spec/opensearch/1.1/}language");
            return table;
        }

        /// <summary>
        /// Gets the base open search parameter.
        /// </summary>
        /// <returns>The base open search parameter.</returns>
        public static NameValueCollection GetBaseOpenSearchParameter()
        {
            NameValueCollection nvc = new NameValueCollection();

            nvc.Add("count", "{count?}");
            nvc.Add("startPage", "{startPage?}");
            nvc.Add("startIndex", "{startIndex?}");
            nvc.Add("q", "{searchTerms?}");
            nvc.Add("lang", "{language?}");

            return nvc;
        }

        /// <summary>
        /// Gets the parameter name from identifier.
        /// </summary>
        /// <returns>The parameter name from identifier.</returns>
        /// <param name="parameters">Parameters.</param>
        /// <param name="id">Identifier.</param>
        public static string GetParamNameFromId(NameValueCollection parameters, string id)
        {

            string param = parameters[id];
            if (param == null)
                return null;

            // first find the defintion of the parameter in the url template
            Match matchParamDef = Regex.Match(param, @"^{([^?]+)\??}$");
            // If parameter does not exist, continue
            if (!matchParamDef.Success)
                return null;
            // We have the parameter defintion
            return matchParamDef.Groups[1].Value;
        }

        /// <summary>
        /// Gets the type of the open search URL by.
        /// </summary>
        /// <returns>The open search URL by type.</returns>
        /// <param name="osd">Osd.</param>
        /// <param name="type">Type.</param>
        public static OpenSearchDescriptionUrl GetOpenSearchUrlByType(OpenSearchDescription osd, string type)
        {

            MimeType mime = MimeType.CreateFromContentType(type);
            try
            {
                if (osd.Url == null) log.DebugFormat("Url Null0");
                return osd.Url.FirstOrDefault(u =>
                {
                    if (u == null) return false;
                    return MimeType.Equals(MimeType.CreateFromContentType(u.Type), mime);
                });
            }
            catch (ArgumentNullException e)
            {
                if (osd.Url == null) log.DebugFormat("Url Null1");
                Thread.Sleep(100);
                log.DebugFormat("ArgumentNullException {0}", e.Message);
                var serializer = new XmlSerializer(typeof(OpenSearchDescription));
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, osd);
                    log.DebugFormat("OSDD {0}", textWriter.ToString());
                }
                log.DebugFormat("Url Length :{0}", osd.Url.Length);
                return osd.Url.FirstOrDefault(u =>
                {
                    if (u == null) return false;
                    return MimeType.Equals(MimeType.CreateFromContentType(u.Type), mime);
                });
            }
        }

        /// <summary>
        /// Gets the open search URL by rel.
        /// </summary>
        /// <returns>The open search URL by rel.</returns>
        /// <param name="osd">Osd.</param>
        /// <param name="rel">Rel.</param>
        public static OpenSearchDescriptionUrl GetOpenSearchUrlByRel(OpenSearchDescription osd, string rel)
        {
            return osd.Url.FirstOrDefault(u => u.Relation == rel);
        }

        /// <summary>
        /// Paginations the free equal.
        /// </summary>
        /// <returns><c>true</c>, if free equal was paginationed, <c>false</c> otherwise.</returns>
        /// <param name="parameters">Parameters.</param>
        /// <param name="parameters2">Parameters2.</param>
        public static bool PaginationFreeEqual(NameValueCollection parameters, NameValueCollection parameters2)
        {

            NameValueCollection nvc1 = new NameValueCollection(parameters);
            NameValueCollection nvc2 = new NameValueCollection(parameters2);

            nvc1.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startPage"]);
            nvc2.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startPage"]);

            nvc1.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startIndex"]);
            nvc2.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startIndex"]);

            return nvc1.AllKeys.OrderBy(key => key)
                    .SequenceEqual(nvc2.AllKeys.OrderBy(key => key))
            && nvc1.AllKeys.All(key => nvc1[key] == nvc2[key]);

        }

        /// <summary>
        /// Merges the open search parameters.
        /// </summary>
        /// <returns>The open search parameters.</returns>
        /// <param name="entities">Entities.</param>
        /// <param name="contentType">Content type.</param>
        public static NameValueCollection MergeOpenSearchParameters(IOpenSearchable[] entities, string contentType)
        {

            NameValueCollection nvc = new NameValueCollection();

            foreach (IOpenSearchable entity in entities)
            {

                NameValueCollection nvc2 = entity.GetOpenSearchParameters(contentType);
                int i = 1;
                foreach (string param in nvc2.Keys)
                {

                    if (nvc2[param] == nvc[param])
                        continue;

                    if (nvc[param] == null)
                    {
                        nvc.Add(param, nvc2[param]);
                        continue;
                    }

                    if (nvc[param] != null)
                    {
                        nvc.Add(param + i++, nvc2[param]);
                        continue;
                    }

                }

            }

            return nvc;

        }

        /// <summary>
        /// Finds the open search description.
        /// </summary>
        /// <returns>The open search description.</returns>
        /// <param name="baseUrl">Base URL.</param>
        public static IOpenSearchable FindOpenSearchable(OpenSearchableFactorySettings settings, Uri baseUrl, string mimeType = null)
        {

            OpenSearchUrl url = new OpenSearchUrl(baseUrl);
            UrlBasedOpenSearchableFactory urlBasedOpenSearchableFactory = new UrlBasedOpenSearchableFactory(settings);
            IOpenSearchable result;

            OpenSearchDescription openSearchDescription;

            try
            {
                openSearchDescription = settings.OpenSearchEngine.AutoDiscoverFromQueryUrl(new OpenSearchUrl(baseUrl), settings);
            }
            catch (ImpossibleSearchException)
            {
                try
                {
                    url = new OpenSearchUrl(new Uri(baseUrl, "/description"));
                    openSearchDescription = settings.OpenSearchEngine.LoadOpenSearchDescriptionDocument(url, settings);
                }
                catch
                {
                    try
                    {
                        url = new OpenSearchUrl(new Uri(baseUrl, "/OSDD"));
                        openSearchDescription = settings.OpenSearchEngine.LoadOpenSearchDescriptionDocument(url, settings);
                    }
                    catch
                    {
                        throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
                    }
                }
            }
            if (openSearchDescription == null)
            {
                throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
            }

            if (!string.IsNullOrEmpty(mimeType))
            {
                OpenSearchDescriptionUrl defaultUrl = OpenSearchFactory.GetOpenSearchUrlByType(openSearchDescription, mimeType);
                if (defaultUrl == null)
                    throw new EntryPointNotFoundException(string.Format("No OpenSearch description with mimetype {1} at {0}", baseUrl, mimeType));
                openSearchDescription.DefaultUrl = defaultUrl;
                //url = OpenSearchFactory.BuildRequestUrlFromTemplate(defaultUrl, HttpUtility.ParseQueryString(baseUrl.Query), new QuerySettings(mimeType, null));
            }

            urlBasedOpenSearchableFactory = new UrlBasedOpenSearchableFactory(settings);
            result = urlBasedOpenSearchableFactory.Create(openSearchDescription, url);
            return result;
        }

        /// <summary>
        /// Gets the name of the identifier from parameter.
        /// </summary>
        /// <returns>The identifier from parameter name.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="paramName">Parameter name.</param>
        public static string GetIdFromParamName(IOpenSearchable entity, string mimeType, string paramName)
        {

            NameValueCollection nvc = entity.GetOpenSearchParameters(mimeType);
            NameValueCollection revNvc = ReverseTemplateOpenSearchParameters(nvc);
            return revNvc[paramName];

        }

        /// <summary>
        /// Gets the open search parameters.
        /// </summary>
        /// <returns>The open search parameters.</returns>
        /// <param name="osUrl">Os URL.</param>
        public static NameValueCollection GetOpenSearchParameters(OpenSearchDescriptionUrl osUrl)
        {
            Uri finalUrl = new Uri(osUrl.Template);
            return HttpUtility.ParseQueryString(finalUrl.Query);
        }

        /// <summary>
        /// Reverses the template open search parameters.
        /// </summary>
        /// <returns>The template open search parameters.</returns>
        /// <param name="osdParam">Osd parameter.</param>
        public static NameValueCollection ReverseTemplateOpenSearchParameters(NameValueCollection osdParam)
        {

            NameValueCollection nvc = new NameValueCollection();
            foreach (string key in osdParam.AllKeys)
            {

                // first find the defintion of the parameter in the url template
                foreach (var value in osdParam.GetValues(key))
                {
                    Match matchParamDef = Regex.Match(value, @"^{([^?]+)\??}$");
                    // If parameter does not exist, continue
                    if (!matchParamDef.Success)
                        continue;
                    // We have the parameter defintion
                    string paramDef = matchParamDef.Groups[1].Value;
                    nvc.Add(paramDef, key);
                }

            }

            return nvc;

        }

        public static void RemoveLinksByRel(ref IOpenSearchResultCollection results, string relType)
        {

            var matchLinks = results.Links.Where(l => l.RelationshipType == relType).ToArray();
            foreach (var link in matchLinks)
            {
                results.Links.Remove(link);
            }

            foreach (IOpenSearchResultItem item in results.Items)
            {
                matchLinks = item.Links.Where(l => l.RelationshipType == relType).ToArray();
                foreach (var link in matchLinks)
                {
                    item.Links.Remove(link);
                }
            }
        }

        public static Type ResolveTypeFromRequest(NameValueCollection query, NameValueCollection headers, OpenSearchEngine ose)
        {
            Type type = ose.Extensions.First().Value.GetTransformType();

            if (query != null && query["format"] != null)
            {
                var osee = ose.GetExtensionByExtensionName(query["format"]);
                if (osee != null)
                {
                    type = osee.GetTransformType();
                }
            }
            else
            {
                if (headers != null && !string.IsNullOrEmpty(headers["Accept"]))
                {
                    foreach (string contentType in headers["Accept"].Split(','))
                    {
                        var osee = ose.GetExtensionByContentTypeAbility(contentType.Trim());
                        if (osee != null)
                        {
                            type = osee.GetTransformType();
                            break;
                        }
                    }
                }
            }

            return type;
        }

#if NETFRAMEWORK 
        [Obsolete("This method is not compatible with .Net Standard. Use ResolveTypeFromRequest(NameValueCollection query, NameValueCollection headers, OpenSearchEngine ose)")]
        public static Type ResolveTypeFromRequest(HttpRequest request, OpenSearchEngine ose)
        {

            Type type = ose.Extensions.First().Value.GetTransformType();

            if (request.Params["format"] != null)
            {
                var osee = ose.GetExtensionByExtensionName(request.Params["format"]);
                if (osee != null)
                {
                    type = osee.GetTransformType();
                }
            }
            else
            {
                if (request.AcceptTypes != null)
                {
                    foreach (string contentType in request.AcceptTypes)
                    {
                        var osee = ose.GetExtensionByContentTypeAbility(contentType);
                        if (osee != null)
                        {
                            type = osee.GetTransformType();
                            break;
                        }
                    }
                }
            }

            return type;
        }

#endif

        public static void ReplaceSelfLinks(IOpenSearchable entity, OpenSearchRequest request, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem, OpenSearchDescription, string, string> entryTemplate)
        {
            ReplaceSelfLinks(entity, request.Parameters, osr, entryTemplate, osr.ContentType);
        }

        public static void ReplaceSelfLinks(IOpenSearchable entity, NameValueCollection parameters, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem, OpenSearchDescription, string, string> entryTemplate)
        {
            ReplaceSelfLinks(entity, parameters, osr, entryTemplate, osr.ContentType);
        }

        public static void ReplaceSelfLinks(IOpenSearchable entity, NameValueCollection parameters, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem, OpenSearchDescription, string, string> entryTemplate, string contentType)
        {
            IOpenSearchResultCollection feed = osr;

            foreach (var link in feed.Links.ToArray())
            {
                if (link != null && link.RelationshipType == "self")
                    feed.Links.Remove(link);
            }

            OpenSearchDescription osd = null;
            if (entity is IProxiedOpenSearchable)
            {
                osd = ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription();
            }
            else
            {
                osd = entity.GetOpenSearchDescription();
            }
            if (OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType) == null)
                return;

            NameValueCollection newNvc = new NameValueCollection(parameters);
            NameValueCollection osparams = OpenSearchFactory.GetOpenSearchParameters(OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType));
            newNvc.AllKeys.FirstOrDefault(k =>
            {
                if (string.IsNullOrEmpty(OpenSearchFactory.GetParamNameFromId(osparams, k)))
                    newNvc.Remove(k);
                return false;
            });
            osparams.AllKeys.FirstOrDefault(k =>
            {
                Match matchParamDef = Regex.Match(osparams[k], @"^{([^?]+)\??}$");
                if (!matchParamDef.Success)
                    newNvc.Set(k, osparams[k]);
                return false;
            });

            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType).Template);
            string[] queryString = Array.ConvertAll(newNvc.AllKeys, key => string.Format("{0}={1}", key, newNvc[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Links.Add(new SyndicationLink(myUrl.Uri, "self", "Reference link", contentType, 0));
            feed.Id = myUrl.ToString();

            foreach (IOpenSearchResultItem item in feed.Items)
            {
                foreach (var link in item.Links.ToArray())
                {
                    if (link != null && link.RelationshipType == "self")
                        item.Links.Remove(link);
                }
                string template = entryTemplate(item, osd, contentType);
                if (template != null)
                {
                    item.Links.Add(new SyndicationLink(new Uri(template), "self", "Reference link", contentType, 0));
                    item.Id = template;
                }
            }
        }

        public static void ReplaceOpenSearchDescriptionLinks(IOpenSearchable entity, IOpenSearchResultCollection osr)
        {
            IOpenSearchResultCollection feed = osr;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "search").ToArray();
            foreach (var link in matchLinks)
            {
                feed.Links.Remove(link);
            }

            OpenSearchDescription osd;
            if (entity is IProxiedOpenSearchable)
                osd = ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription();
            else
                osd = entity.GetOpenSearchDescription();
            OpenSearchDescriptionUrl url = OpenSearchFactory.GetOpenSearchUrlByRel(osd, "self");
            Uri uri;
            if (url != null)
                uri = new Uri(url.Template);
            else
                uri = osd.Originator;
            if (uri != null)
                feed.Links.Add(new SyndicationLink(uri, "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));


        }


        public static SyndicationLink[] GetEnclosures(IOpenSearchResultCollection result)
        {

            List<SyndicationLink> links = new List<SyndicationLink>();

            foreach (IOpenSearchResultItem item in result.Items)
            {
                foreach (SyndicationLink link in item.Links)
                {
                    if (link.RelationshipType == "enclosure")
                    {
                        links.Add(link);
                    }
                }
            }

            return links.ToArray();
        }

        public static List<OpenSearchDescriptionUrlParameter> GetDefaultParametersDescription(int maxCount)
        {

            List<OpenSearchDescriptionUrlParameter> parameters = new List<OpenSearchDescriptionUrlParameter>();

            parameters.Add(new OpenSearchDescriptionUrlParameter()
            {
                Name = "count",
                Value = @"{count}",
                Title = "number of search results per page desired",
                Minimum = "0",
                MaxInclusive = maxCount.ToString()
            });
            parameters.Add(new OpenSearchDescriptionUrlParameter()
            {
                Name = "startPage",
                Value = @"{startPage}",
                Title = "page number of the set of search results desired",
                Minimum = "0"
            });
            parameters.Add(new OpenSearchDescriptionUrlParameter()
            {
                Name = "startIndex",
                Value = @"{startIndex}",
                Title = "index of the first search result desired",
                Minimum = "0"
            });
            parameters.Add(new OpenSearchDescriptionUrlParameter()
            {
                Name = "q",
                Value = @"{searchTerms}",
                Title = "keywords to be found in the search results",
                Minimum = "0"
            });
            parameters.Add(new OpenSearchDescriptionUrlParameter()
            {
                Name = "lang",
                Value = @"{language}",
                Title = "desired language of the results",
                Minimum = "0"
            });

            return parameters;

        }

        public static int GetCount(NameValueCollection parameters)
        {
            try
            {
                return int.Parse(parameters["count"]);
            }
            catch (Exception)
            {
                return OpenSearchEngine.DEFAULT_COUNT;
            }
        }
    }



    /// <summary>
    /// URL based open searchable factory.
    /// </summary>
    public class UrlBasedOpenSearchableFactory : IOpenSearchableFactory
    {

        public UrlBasedOpenSearchableFactory(OpenSearchableFactorySettings settings)
        {
            Settings = (OpenSearchableFactorySettings)settings.Clone();
        }

        #region IOpenSearchableFactory implementation

        public IOpenSearchable Create(OpenSearchUrl url)
        {
            if (Settings.Soft)
                return new SoftGenericOpenSearchable(url, Settings);
            return new GenericOpenSearchable(url, Settings);
        }

        public IOpenSearchable Create(OpenSearchDescription osd, OpenSearchUrl url = null)
        {

            GenericOpenSearchable os = null;
            if (Settings.Soft)
                os = new SoftGenericOpenSearchable(osd, Settings, url);
            else
                os = new GenericOpenSearchable(osd, Settings, url);

            return os;
        }

        public OpenSearchableFactorySettings Settings { get; private set; }

        #endregion
    }
}
