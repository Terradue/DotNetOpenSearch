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
using System.Web;
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
            return osd;
        }

        public static OpenSearchUrl BuildRequestUrlFromTemplate(OpenSearchDescriptionUrl remoteUrlTemplate, NameValueCollection searchParameters, QuerySettings querySettings)
        {

            string finalUrl = remoteUrlTemplate.Template;

            NameValueCollection additionalParameters = new NameValueCollection();

            // For each parameter requested
            foreach (string parameter_id in searchParameters.AllKeys)
            {

                if (string.IsNullOrEmpty(parameter_id))
                    continue;

                // default finder
                Regex findParam = new Regex(string.Format("{0}={{(?'prefix'[^:]+):(?'name'[^&]+)\\??}}", parameter_id));
                string replacement = string.Format("{0}={1}", parameter_id, HttpUtility.UrlEncode(searchParameters[parameter_id]));

                // Is it a FQDN
                Match matchFQDN = Regex.Match(parameter_id, @"^{(?'namespace'[^}]+)}(?'name'.+)$");

                if (matchFQDN.Success)
                {
                    string ns = matchFQDN.Groups["namespace"].Value;
                    string name = matchFQDN.Groups["name"].Value;

                    XmlQualifiedName qPrefix = remoteUrlTemplate.ExtraNamespace.ToArray().FirstOrDefault(n => n.Namespace == ns);

                    if (qPrefix == null)
                        continue;

                    findParam = new Regex(string.Format("{{{0}{1}\\??}}", string.IsNullOrEmpty(qPrefix.Name) ? "" : qPrefix.Name + ":", name));
                    replacement = HttpUtility.UrlEncode(searchParameters[parameter_id]);

                }

                // Is it a prefixed param
                Match matchPrefixed = Regex.Match(parameter_id, @"^(?'prefix'[a-zA-Z0-9_\-]+):(?'name'[^&]+)$");

                if (matchPrefixed.Success)
                {
                    string prefix = matchPrefixed.Groups["prefix"].Value;
                    string name = matchPrefixed.Groups["name"].Value;

                    // check that prefix exist
                    if (!remoteUrlTemplate.ExtraNamespace.ToArray().Any(n => n.Name == prefix) && !querySettings.ForceUnspecifiedParameters)
                        continue;

                    findParam = new Regex(string.Format("{{{0}:{1}\\??}}", prefix, name));
                    replacement = HttpUtility.UrlEncode(searchParameters[parameter_id]);

                }

                string newUrl = findParam.Replace(finalUrl, replacement);

                // special case for non qualified opensearch parameters
                if (newUrl == finalUrl && !matchFQDN.Success && !matchPrefixed.Success)
                {
                    if (new string[] { "count", "startPage", "startIndex", "language", "searchTerms" }.Contains(parameter_id))
                    {
                        XmlQualifiedName qPrefix = remoteUrlTemplate.ExtraNamespace.ToArray().FirstOrDefault(n => n.Namespace == "http://a9.com/-/spec/opensearch/1.1/");

                        findParam = new Regex(string.Format("{{{0}{1}\\??}}", string.IsNullOrEmpty(qPrefix.Name) ? "" : qPrefix.Name + ":", parameter_id));
                        newUrl = findParam.Replace(finalUrl, HttpUtility.UrlEncode(searchParameters[parameter_id]));
                    }

                    // force parameter anyway
                    if (newUrl == finalUrl && querySettings.ForceUnspecifiedParameters)
                    {
                        additionalParameters.Set(parameter_id, searchParameters[parameter_id]);
                    }
                }

                finalUrl = newUrl;
            }

            //Clean the remaining parameters templates
            Regex cleanParam = new Regex(string.Format("{{(?'prefix'[^:]+):(?'name'[^&]+)\\??}}"));
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


        /// <summary>
        /// Builds the request URL for template.
        /// </summary>
        /// <returns>The request URL for template.</returns>
        /// <param name="remoteUrlTemplate">Remote URL template.</param>
        /// <param name="searchParameters">Search parameters.</param>
        /// <param name="urlTemplateDef">URL template def.</param>
        [Obsolete("BuildRequestUrlForTemplate is deprecated, please use BuildRequestUrlFromTemplate instead.")]
        public static OpenSearchUrl BuildRequestUrlForTemplate(OpenSearchDescriptionUrl remoteUrlTemplate, NameValueCollection searchParameters, NameValueCollection requestUrlTemplateDef, QuerySettings querySettings)
        {
            // container for the final query url
            UriBuilder finalUrl = new UriBuilder(remoteUrlTemplate.Template);
            // parameters for final query
            NameValueCollection finalQueryParameters = new NameValueCollection();

            // Parse the possible parameters of the remote url template
            NameValueCollection remoteParametersDef = HttpUtility.ParseQueryString(finalUrl.Query);

            // control duplicates
            foreach (string key in remoteParametersDef.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                int count = remoteParametersDef.GetValues(key).Count();
                if (count > 1)
                {
                    var value = remoteParametersDef.GetValues(key)[0];
                    remoteParametersDef.Remove(key);
                    remoteParametersDef.Add(key, value);
                }
            }

            // For each parameter id set for search
            foreach (string parameter_id in searchParameters.AllKeys)
            {
                // skip if parameter is not in the template unless it is forced
                if (requestUrlTemplateDef[parameter_id] == null)
                {
                    // if forced, set the param
                    if (querySettings.ForceUnspecifiedParameters)
                    {
                        if (!(querySettings.SkipNullOrEmptyQueryStringParameters && string.IsNullOrEmpty(searchParameters[parameter_id])))
                            finalQueryParameters.Set(parameter_id, searchParameters[parameter_id]);
                    }
                    continue;
                }
                // first find the defintion of the parameter in the url template
                foreach (var key in requestUrlTemplateDef.GetValues(parameter_id))
                {
                    Match matchParamDef = Regex.Match(key, @"^{([^?]+)\??}$");
                    // If parameter is not respecting OpenSearch template spec, skip
                    if (!matchParamDef.Success)
                        continue;
                    // We have the parameter defintion
                    string paramDef = matchParamDef.Groups[1].Value;
                    string paramValue = searchParameters[parameter_id];



                    // Find the paramdef in the remote URL template
                    foreach (string keyDef in remoteParametersDef.AllKeys)
                    {
                        foreach (var key2 in remoteParametersDef.GetValues(keyDef))
                        {
                            Match remoteMatchParamDef = Regex.Match(key2, @"^{(" + paramDef + @")\??}$");
                            // if match is successful
                            if (remoteMatchParamDef.Success)
                            {
                                // then add the parameter with the right key
                                if (!(querySettings.SkipNullOrEmptyQueryStringParameters && string.IsNullOrEmpty(paramValue)))
                                    finalQueryParameters.Set(keyDef, paramValue);
                            }
                        }
                    }
                }

            }

            // All other remote query parameters
            foreach (string parameter in remoteParametersDef.AllKeys)
            {
                Match matchParamDef = Regex.Match(remoteParametersDef[parameter], @"^{([^?]+)\??}$");
                // If parameter does not exist, continue
                if (!matchParamDef.Success && !string.IsNullOrEmpty(parameter))
                {
                    if (!(querySettings.SkipNullOrEmptyQueryStringParameters && string.IsNullOrEmpty(remoteParametersDef[parameter])))
                        finalQueryParameters.Set(parameter, remoteParametersDef[parameter]);
                }
            }

            //finalQueryParameters.Set("enableSourceproduct", "true");
            string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, HttpUtility.UrlEncode(finalQueryParameters[key])));
            finalUrl.Query = string.Join("&", queryString);

            return new OpenSearchUrl(finalUrl.Uri);
        }

        /// <summary>
        /// Builds the request URL from template name parameters.
        /// </summary>
        /// <returns>The request URL from template name parameters.</returns>
        /// <param name="remoteUrlTemplate">Remote URL template.</param>
        /// <param name="templateSearchParameters">Template search parameters.</param>
        public static string BuildRequestUrlFromTemplateNameParameters(OpenSearchUrl remoteUrlTemplate, NameValueCollection templateSearchParameters)
        {
            // container for the final query url
            UriBuilder finalUrl = new UriBuilder(remoteUrlTemplate);
            // parameters for final query
            NameValueCollection finalQueryParameters = new NameValueCollection();

            // Parse the possible parametrs of the remote urls
            NameValueCollection remoteParametersDef = HttpUtility.ParseQueryString(remoteUrlTemplate.Query);

            // For each parameter requested
            foreach (string parameter in templateSearchParameters.AllKeys)
            {

                string value = templateSearchParameters[parameter];

                // Find the paramdef in the remote URl template
                foreach (string keyDef in remoteParametersDef.AllKeys)
                {
                    foreach (string key2 in remoteParametersDef.GetValues(keyDef))
                    {
                        Match remoteMatchParamDef = Regex.Match(key2, @"^{(" + parameter + @")\??}$");
                        // if martch is successful
                        if (remoteMatchParamDef.Success)
                        {
                            // then add the parameter with the right key
                            finalQueryParameters.Add(keyDef, value);
                        }
                    }
                }

            }

            string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, HttpUtility.UrlEncode(finalQueryParameters[key])));
            finalUrl.Query = string.Join("&", queryString);

            return finalUrl.ToString();
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
            catch (ImpossibleSearchException e)
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
            result = urlBasedOpenSearchableFactory.Create(openSearchDescription);
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

        /*public static void ReplaceId(ref IOpenSearchResultCollection osr) {
            IOpenSearchResultCollection feed = osr;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            if (matchLinks.Count() > 0)
                feed.Id = matchLinks[0].Uri.ToString();

            foreach (IOpenSearchResultItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "self").ToArray();
                if (matchLinks.Count() > 0)
                    item.Id = matchLinks[0].Uri.ToString();
            }
        }*/

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

        public static OpenSearchDescriptionUrl GetOpenSearchUrlByTypeAndMaxParam(OpenSearchDescription osd, List<string> mimeTypes, NameValueCollection osParameters)
        {

            OpenSearchDescriptionUrl url = null;
            int maxParam = -1;

            foreach (var urlC in osd.Url)
            {
                UriBuilder tempU = new UriBuilder(BuildRequestUrlFromTemplateNameParameters(new OpenSearchUrl(urlC.Template), osParameters));
                int numParam = HttpUtility.ParseQueryString(tempU.Query).Count;
                if (maxParam < numParam)
                {
                    maxParam = numParam;
                    url = urlC;
                }
            }

            return url;

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
            catch (Exception e)
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

        public IOpenSearchable Create(OpenSearchDescription osd)
        {
            if (Settings.Soft)
                return new SoftGenericOpenSearchable(osd, Settings);
            return new GenericOpenSearchable(osd, Settings);
        }

        public OpenSearchableFactorySettings Settings { get; private set; }

        #endregion
    }
}
