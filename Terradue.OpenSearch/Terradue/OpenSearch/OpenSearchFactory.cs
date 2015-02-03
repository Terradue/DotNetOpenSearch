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

namespace Terradue.OpenSearch {
    /// <summary>
    /// OpenSearch factory. Helper class with a set of static class with the most common manipulation
    /// functions for OpenSearch.
    /// </summary>
    public partial class OpenSearchFactory {
        /// <summary>
        /// Loads the OpenSearch description document.
        /// </summary>
        /// <returns>The OpenSearch description document.</returns>
        /// <param name="url">URL.</param>
        public static OpenSearchDescription LoadOpenSearchDescriptionDocument(OpenSearchUrl url) {
            try {
                XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
                XmlReader reader = XmlReader.Create(url.ToString());
                return (OpenSearchDescription)ser.Deserialize(reader);
            } catch (Exception e) {
                throw new InvalidOperationException("Exception querying OpenSearch description at " + url.ToString() + " : " + e.Message, e);
            }
        }

        /// <summary>
        /// Builds the request URL for template.
        /// </summary>
        /// <returns>The request URL for template.</returns>
        /// <param name="remoteUrlTemplate">Remote URL template.</param>
        /// <param name="searchParameters">Search parameters.</param>
        public static OpenSearchUrl BuildRequestUrlForTemplate(OpenSearchDescriptionUrl remoteUrlTemplate, NameValueCollection searchParameters) {
            // container for the final query url
            UriBuilder finalUrl = new UriBuilder(remoteUrlTemplate.Template);
            // parameters for final query
            NameValueCollection finalQueryParameters = new NameValueCollection();

            // Parse the possible parametrs of the remote urls
            NameValueCollection remoteParametersDef = HttpUtility.ParseQueryString(finalUrl.Query);

            // For each parameter requested
            foreach (string parameter in searchParameters.AllKeys) {
                if (remoteParametersDef[parameter] == null)
                    continue;
                // first find the defintion of the parameter in the url template
                foreach (var key in remoteParametersDef.GetValues(parameter)) {
                    Match matchParamDef = Regex.Match(key, @"^{([^?]+)\??}$");
                    // If parameter does not exist, continue
                    if (!matchParamDef.Success)
                        continue;
                    // We have the parameter defintion
                    string paramDef = matchParamDef.Groups[1].Value;
                    string paramValue = searchParameters[parameter];

                    finalQueryParameters.Set(parameter, paramValue);
                }

            }

            string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, finalQueryParameters[key]));
            finalUrl.Query = string.Join("&", queryString);

            return new OpenSearchUrl(finalUrl.Uri);
        }

        /// <summary>
        /// Builds the request URL for template.
        /// </summary>
        /// <returns>The request URL for template.</returns>
        /// <param name="remoteUrlTemplate">Remote URL template.</param>
        /// <param name="searchParameters">Search parameters.</param>
        /// <param name="urlTemplateDef">URL template def.</param>
        public static OpenSearchUrl BuildRequestUrlForTemplate(OpenSearchDescriptionUrl remoteUrlTemplate, NameValueCollection searchParameters, NameValueCollection urlTemplateDef) {
            // container for the final query url
            UriBuilder finalUrl = new UriBuilder(remoteUrlTemplate.Template);
            // parameters for final query
            NameValueCollection finalQueryParameters = new NameValueCollection();

            // Parse the possible parametrs of the remote urls
            NameValueCollection remoteParametersDef = HttpUtility.ParseQueryString(finalUrl.Query);

            // control duplicates
            foreach (string key in remoteParametersDef)
            {
                if (string.IsNullOrEmpty(key)) continue;
                int count = remoteParametersDef.GetValues(key).Count();
                if (count > 1) throw new OpenSearchException(string.Format("Url template [{0}] from OpenSearch Description cannot contains duplicates parameter definition: {1}", finalUrl, key));
            }

            // For each parameter requested
            foreach (string parameter in searchParameters.AllKeys) {
                if (urlTemplateDef[parameter] == null)
                    continue;
                // first find the defintion of the parameter in the url template
                foreach (var key in urlTemplateDef.GetValues(parameter)) {
                    Match matchParamDef = Regex.Match(key, @"^{([^?]+)\??}$");
                    // If parameter does not exist, continue
                    if (!matchParamDef.Success)
                        continue;
                    // We have the parameter defintion
                    string paramDef = matchParamDef.Groups[1].Value;
                    string paramValue = searchParameters[parameter];

                    // Find the paramdef in the remote URl template
                    foreach (string keyDef in remoteParametersDef.AllKeys) {
                        foreach (var key2 in remoteParametersDef.GetValues(keyDef)) {
                            Match remoteMatchParamDef = Regex.Match(key2, @"^{(" + paramDef + @")\??}$");
                            // if martch is successful
                            if (remoteMatchParamDef.Success) {
                                // then add the parameter with the right key
                                finalQueryParameters.Set(keyDef, paramValue);
                            }
                        }
                    }
                }

            }

            foreach (string parameter in remoteParametersDef.AllKeys) {
                Match matchParamDef = Regex.Match(remoteParametersDef[parameter], @"^{([^?]+)\??}$");
                // If parameter does not exist, continue
                if (!matchParamDef.Success && !string.IsNullOrEmpty(parameter))
                    finalQueryParameters.Set(parameter, remoteParametersDef[parameter]);
            }

            finalQueryParameters.Set("enableSourceproduct", "true");

            string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, finalQueryParameters[key]));
            finalUrl.Query = string.Join("&", queryString);
			
            return new OpenSearchUrl(finalUrl.Uri);
        }

        /// <summary>
        /// Builds the request URL from template name parameters.
        /// </summary>
        /// <returns>The request URL from template name parameters.</returns>
        /// <param name="remoteUrlTemplate">Remote URL template.</param>
        /// <param name="templateSearchParameters">Template search parameters.</param>
        public static string BuildRequestUrlFromTemplateNameParameters(OpenSearchUrl remoteUrlTemplate, NameValueCollection templateSearchParameters) {
            // container for the final query url
            UriBuilder finalUrl = new UriBuilder(remoteUrlTemplate);
            // parameters for final query
            NameValueCollection finalQueryParameters = new NameValueCollection();

            // Parse the possible parametrs of the remote urls
            NameValueCollection remoteParametersDef = HttpUtility.ParseQueryString(remoteUrlTemplate.Query);

            // For each parameter requested
            foreach (string parameter in templateSearchParameters.AllKeys) {
           
                string value = templateSearchParameters[parameter];

                // Find the paramdef in the remote URl template
                foreach (string keyDef in remoteParametersDef.AllKeys) {
                    foreach (string key2 in remoteParametersDef.GetValues(keyDef)) {
                        Match remoteMatchParamDef = Regex.Match(key2, @"^{(" + parameter + @")\??}$");
                        // if martch is successful
                        if (remoteMatchParamDef.Success) {
                            // then add the parameter with the right key
                            finalQueryParameters.Add(keyDef, value);
                        }
                    }
                }

            }

            string[] queryString = Array.ConvertAll(finalQueryParameters.AllKeys, key => string.Format("{0}={1}", key, finalQueryParameters[key]));
            finalUrl.Query = string.Join("&", queryString);

            return finalUrl.ToString();
        }

        /// <summary>
        /// Gets the base open search parameter.
        /// </summary>
        /// <returns>The base open search parameter.</returns>
        public static NameValueCollection GetBaseOpenSearchParameter() {
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
        public static string GetParamNameFromId(NameValueCollection parameters, string id) {

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
        public static OpenSearchDescriptionUrl GetOpenSearchUrlByType(OpenSearchDescription osd, string type) {
            return osd.Url.FirstOrDefault(u => u.Type == type);
        }

        /// <summary>
        /// Gets the open search URL by rel.
        /// </summary>
        /// <returns>The open search URL by rel.</returns>
        /// <param name="osd">Osd.</param>
        /// <param name="rel">Rel.</param>
        public static OpenSearchDescriptionUrl GetOpenSearchUrlByRel(OpenSearchDescription osd, string rel) {
            return osd.Url.FirstOrDefault(u => u.Relation == rel);
        }

        /// <summary>
        /// Paginations the free equal.
        /// </summary>
        /// <returns><c>true</c>, if free equal was paginationed, <c>false</c> otherwise.</returns>
        /// <param name="parameters">Parameters.</param>
        /// <param name="parameters2">Parameters2.</param>
        public static bool PaginationFreeEqual(NameValueCollection parameters, NameValueCollection parameters2) {

            NameValueCollection nvc1 = new NameValueCollection(parameters);
            NameValueCollection nvc2 = new NameValueCollection(parameters2);

            nvc1.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startPage"]);
            nvc2.Remove(ReverseTemplateOpenSearchParameters(GetBaseOpenSearchParameter())["startPage"]);

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
        public static NameValueCollection MergeOpenSearchParameters(IOpenSearchable[] entities, string contentType) {

            NameValueCollection nvc = new NameValueCollection();

            foreach (IOpenSearchable entity in entities) {

                NameValueCollection nvc2 = entity.GetOpenSearchParameters(contentType);
                int i = 1;
                foreach (string param in nvc2.Keys) {

                    if (nvc2[param] == nvc[param])
                        continue;

                    if (nvc[param] == null) {
                        nvc.Add(param, nvc2[param]);
                        continue;
                    }

                    if (nvc[param] != null) {
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
        public static IOpenSearchable FindOpenSearchable(OpenSearchEngine ose, Uri baseUrl, string mimeType = null) {

            OpenSearchUrl url = new OpenSearchUrl(baseUrl);
            OpenSearchDescription openSearchDescription = null;
            UrlBasedOpenSearchableFactory urlBasedOpenSearchableFactory;
            IOpenSearchable result;
            try {
                openSearchDescription = OpenSearchFactory.LoadOpenSearchDescriptionDocument(url);
                OpenSearchDescriptionUrl openSearchDescriptionUrl;
                if (mimeType == null) {
                    openSearchDescriptionUrl = openSearchDescription.Url.First<OpenSearchDescriptionUrl>();
                }
                else {
                    openSearchDescriptionUrl = openSearchDescription.Url.FirstOrDefault((OpenSearchDescriptionUrl u) => u.Type == mimeType);
                }
                if (openSearchDescriptionUrl == null) {
                    throw new InvalidOperationException("Impossible to find an OpenSearchable link for the type " + mimeType);
                }
                openSearchDescription.DefaultUrl = openSearchDescriptionUrl;
            }
            catch (InvalidOperationException ex) {
                if (!(ex.InnerException is FileNotFoundException) && !(ex.InnerException is SecurityException) && !(ex.InnerException is UriFormatException)) {
                    openSearchDescription = ose.AutoDiscoverFromQueryUrl(new OpenSearchUrl(baseUrl));
                    urlBasedOpenSearchableFactory = new UrlBasedOpenSearchableFactory(ose);
                    result = urlBasedOpenSearchableFactory.Create(url);
                    return result;
                }
                try {
                    url = new OpenSearchUrl(new Uri(baseUrl, "/description"));
                    openSearchDescription = OpenSearchFactory.LoadOpenSearchDescriptionDocument(url);
                }
                catch {
                    try {
                        url = new OpenSearchUrl(new Uri(baseUrl, "/OSDD"));
                        openSearchDescription = OpenSearchFactory.LoadOpenSearchDescriptionDocument(url);
                    }
                    catch {
                        throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
                    }
                }
            }
            if (openSearchDescription == null) {
                throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
            }
            urlBasedOpenSearchableFactory = new UrlBasedOpenSearchableFactory(ose);
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
        public static string GetIdFromParamName(IOpenSearchable entity, string mimeType, string paramName) {

            NameValueCollection nvc = entity.GetOpenSearchParameters(mimeType);
            NameValueCollection revNvc = ReverseTemplateOpenSearchParameters(nvc);
            return revNvc[paramName];

        }

        /// <summary>
        /// Gets the open search parameters.
        /// </summary>
        /// <returns>The open search parameters.</returns>
        /// <param name="osUrl">Os URL.</param>
        public static NameValueCollection GetOpenSearchParameters(OpenSearchDescriptionUrl osUrl) {
            Uri finalUrl = new Uri(osUrl.Template);
            return HttpUtility.ParseQueryString(finalUrl.Query);
        }

        /// <summary>
        /// Reverses the template open search parameters.
        /// </summary>
        /// <returns>The template open search parameters.</returns>
        /// <param name="osdParam">Osd parameter.</param>
        public static NameValueCollection ReverseTemplateOpenSearchParameters(NameValueCollection osdParam) {

            NameValueCollection nvc = new NameValueCollection();
            foreach (string key in osdParam.AllKeys) {

                // first find the defintion of the parameter in the url template
                foreach (var value in osdParam.GetValues(key)) {
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

        public static void RemoveLinksByRel(ref IOpenSearchResult osr, string relType) {
            IOpenSearchResultCollection results = (IOpenSearchResultCollection)osr.Result;

            RemoveLinksByRel(ref results, relType);
        }

        public static void RemoveLinksByRel(ref IOpenSearchResultCollection results, string relType) {

            var matchLinks = results.Links.Where(l => l.RelationshipType == relType).ToArray();
            foreach (var link in matchLinks) {
                results.Links.Remove(link);
            }

            foreach (IOpenSearchResultItem item in results.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == relType).ToArray();
                foreach (var link in matchLinks) {
                    item.Links.Remove(link);
                }
            }
        }

        public static Type ResolveTypeFromRequest(HttpRequest request, OpenSearchEngine ose) {

            Type type = ose.Extensions.First().Value.GetTransformType();

            if (request.Params["format"] != null) {
                var osee = ose.GetExtensionByExtensionName(request.Params["format"]);
                if (osee != null) {
                    type = osee.GetTransformType();
                }
            } else {
                if (request.AcceptTypes != null) {
                    foreach (string contentType in request.AcceptTypes) {
                        var osee = ose.GetExtensionByContentTypeAbility(contentType);
                        if (osee != null) {
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

        public static void ReplaceSelfLinks(IOpenSearchable entity, OpenSearchRequest request, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem,OpenSearchDescription,string,string> entryTemplate) {
            ReplaceSelfLinks(entity, request.Parameters, osr, entryTemplate, osr.ContentType);
        }

        public static void ReplaceSelfLinks(IOpenSearchable entity, NameValueCollection parameters, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem,OpenSearchDescription,string,string> entryTemplate) {
            ReplaceSelfLinks(entity, parameters, osr, entryTemplate, osr.ContentType);
        }

        public static void ReplaceSelfLinks(IOpenSearchable entity, NameValueCollection parameters, IOpenSearchResultCollection osr, Func<IOpenSearchResultItem,OpenSearchDescription,string,string> entryTemplate, string contentType) {
            IOpenSearchResultCollection feed = osr;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            OpenSearchDescription osd = null;
            if (entity is IProxiedOpenSearchable) {
                osd = ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription(); 
            } else {
                osd = entity.GetOpenSearchDescription();
            }
            if (OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType) == null)
                return;

            NameValueCollection newNvc = new NameValueCollection(parameters);
            NameValueCollection nvc = OpenSearchFactory.GetOpenSearchParameters(OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType));
            newNvc.AllKeys.FirstOrDefault(k => {
                if (nvc[k] == null)
                    newNvc.Remove(k);
                return false;
            });
            nvc.AllKeys.FirstOrDefault(k => {
                Match matchParamDef = Regex.Match(nvc[k], @"^{([^?]+)\??}$");
                if (!matchParamDef.Success)
                    newNvc.Set(k, nvc[k]);
                return false;
            });

            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, contentType).Template);
            string[] queryString = Array.ConvertAll(newNvc.AllKeys, key => string.Format("{0}={1}", key, newNvc[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Links.Add(new SyndicationLink(myUrl.Uri, "self", "Reference link", contentType, 0));

            foreach (IOpenSearchResultItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "self").ToArray();
                foreach (var link in matchLinks) {
                    item.Links.Remove(link);
                }
                string template = entryTemplate(item, osd, contentType);
                if (template != null) {
                    item.Links.Add(new SyndicationLink(new Uri(template), "self", "Reference link", contentType, 0));
                }
            }
        }


        /*public static void ReplaceId(IOpenSearchResult osr) {
            IOpenSearchResultCollection feed = osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            if (matchLinks.Count() > 0)
                feed.Id = matchLinks[0].Uri.ToString();

            foreach (IOpenSearchResultItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "self").ToArray();
                if (matchLinks.Count() > 0)
                    item.Id = matchLinks[0].Uri.ToString();
            }
        }*/

        public static void ReplaceOpenSearchDescriptionLinks(IOpenSearchable entity, IOpenSearchResultCollection osr) {
            IOpenSearchResultCollection feed = osr;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "search").ToArray();
            foreach (var link in matchLinks) {
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

            foreach (IOpenSearchResultItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "search").ToArray();
                foreach (var link in matchLinks) {
                    item.Links.Remove(link);
                }
                if (url != null)
                    item.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));
            }

        }

        public static OpenSearchDescriptionUrl GetOpenSearchUrlByTypeAndMaxParam(OpenSearchDescription osd, List<string> mimeTypes, NameValueCollection osParameters) {

            OpenSearchDescriptionUrl url = null;
            int maxParam = -1;

            foreach (var urlC in osd.Url) {
                UriBuilder tempU = new UriBuilder(BuildRequestUrlFromTemplateNameParameters(new OpenSearchUrl(urlC.Template), osParameters));
                int numParam = HttpUtility.ParseQueryString(tempU.Query).Count;
                if (maxParam < numParam) {
                    maxParam = numParam;
                    url = urlC;
                }
            }

            return url;

        }

        public static NameValueCollection ReplaceTemplateByIdentifier(NameValueCollection osParameters, OpenSearchDescriptionUrl osdUrl) {
            NameValueCollection dic = osdUrl.GetIdentifierDictionary();
            NameValueCollection newNvc = new NameValueCollection();
            foreach (var templateKey in osParameters.AllKeys) {
                if (dic[templateKey] == null)
                    continue;
                newNvc.Add(dic[templateKey], osParameters[templateKey]);

            }
            return newNvc;
        }

        public static SyndicationLink[] GetEnclosures(IOpenSearchResultCollection result) {

            List<SyndicationLink> links = new List<SyndicationLink>();

            foreach (IOpenSearchResultItem item in result.Items) {
                foreach (SyndicationLink link in item.Links) {
                    if (link.RelationshipType == "enclosure") {
                        links.Add(link);
                    }
                }
            }

            return links.ToArray();
        }

        public static ParametersResult GetDefaultParametersResult(){

            ParametersResult p = new ParametersResult();

            ParameterDescription parameter;

            parameter = new ParameterDescription();

            p.Parameters.Add(new ParameterDescription(){Id="count", Template = new XmlQualifiedName("count","http://a9.com/-/spec/opensearch/1.1/"), Abstract="number of search results per page desired", Type="integer"});
            p.Parameters.Add(new ParameterDescription(){Id="startPage", Template = new XmlQualifiedName("startPage","http://a9.com/-/spec/opensearch/1.1/"), Namespace="http://a9.com/-/spec/opensearch/1.1/", Abstract="page number of the set of search results desired", Type="integer"});
            p.Parameters.Add(new ParameterDescription(){Id="startIndex", Template = new XmlQualifiedName("startIndex","http://a9.com/-/spec/opensearch/1.1/"), Namespace="http://a9.com/-/spec/opensearch/1.1/", Abstract="index of the first search result desired", Type="integer"});
            p.Parameters.Add(new ParameterDescription(){Id="q", Template = new XmlQualifiedName("searchTerms","http://a9.com/-/spec/opensearch/1.1/"), Namespace="http://a9.com/-/spec/opensearch/1.1/", Abstract="keywords to be found in the search results", Type="string"});
            p.Parameters.Add(new ParameterDescription(){Id="lang", Template = new XmlQualifiedName("language","http://a9.com/-/spec/opensearch/1.1/"), Namespace="http://a9.com/-/spec/opensearch/1.1/", Abstract="desired language of the results", Type="string"});

            return p;

        }
    }



    /// <summary>
    /// URL based open searchable factory.
    /// </summary>
    public class UrlBasedOpenSearchableFactory : IOpenSearchableFactory {
        OpenSearchEngine ose;

        public UrlBasedOpenSearchableFactory(OpenSearchEngine ose) {
            this.ose = ose;
        }

        #region IOpenSearchableFactory implementation

        public IOpenSearchable Create(OpenSearchUrl url) {
            return new GenericOpenSearchable(url, ose);
        }

        public IOpenSearchable Create(OpenSearchDescription osd) {
            return new GenericOpenSearchable(osd, ose);
        }

        #endregion
    }
}
