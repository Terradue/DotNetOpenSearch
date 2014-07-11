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

                    // Special case for startPage and startIndex
                    if (paramDef == "startPage" && !string.IsNullOrEmpty(paramValue)) {
                        paramValue = (int.Parse(paramValue) + remoteUrlTemplate.PageOffset).ToString();
                    }

                    // Special case for startPage and startIndex
                    if (paramDef == "startIndex" && !string.IsNullOrEmpty(paramValue)) {
                        paramValue = (int.Parse(searchParameters[parameter]) + remoteUrlTemplate.IndexOffset).ToString();
                    }

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

                    // Special case for startPage and startIndex
                    if (paramDef == "startPage" && !string.IsNullOrEmpty(paramValue)) {
                        paramValue = (int.Parse(paramValue) + (remoteUrlTemplate.PageOffset - 1)).ToString();
                    }

                    // Special case for startPage and startIndex
                    if (paramDef == "startIndex" && !string.IsNullOrEmpty(paramValue)) {
                        paramValue = (int.Parse(searchParameters[parameter]) + (remoteUrlTemplate.IndexOffset - 1)).ToString();
                    }

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

                // Special case for startPage and startIndex
                if (parameter == "startPage" && !string.IsNullOrEmpty(value)) {
                    value = (int.Parse(value) + (remoteUrlTemplate.PageOffset - 1)).ToString();
                }

                // Special case for startPage and startIndex
                if (parameter == "startIndex" && !string.IsNullOrEmpty(value)) {
                    value = (int.Parse(value) + (remoteUrlTemplate.IndexOffset - 1)).ToString();
                }

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
        public static IOpenSearchable FindOpenSearchable(OpenSearchEngine ose, Uri baseUrl) {

            OpenSearchUrl tryUrl = new OpenSearchUrl(baseUrl);
            OpenSearchDescription osd = null;
            UrlBasedOpenSearchableFactory factory;

            try {
                osd = LoadOpenSearchDescriptionDocument(tryUrl);
            } catch (InvalidOperationException e) {
                if (e.InnerException is FileNotFoundException || e.InnerException is SecurityException || e.InnerException is UriFormatException) {
                    try {
                        tryUrl = new OpenSearchUrl(new Uri(baseUrl, "/description"));
                        osd = LoadOpenSearchDescriptionDocument(tryUrl);
                    } catch {
                        try {
                            tryUrl = new OpenSearchUrl(new Uri(baseUrl, "/OSDD"));
                            osd = LoadOpenSearchDescriptionDocument(tryUrl);
                        } catch {
                            throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
                        }
                    }
                }
                if (e.InnerException is InvalidOperationException) {
                    osd = ose.AutoDiscoverFromQueryUrl(new OpenSearchUrl(baseUrl));
                    factory = new UrlBasedOpenSearchableFactory(ose);
                    return factory.Create(tryUrl);
                }
            }
            if (osd != null) {
                factory = new UrlBasedOpenSearchableFactory(ose);
                return factory.Create(osd);
            } else
                throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
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

        public static void RemoveLinksByRel(IOpenSearchResult osr, string relType) {
            IOpenSearchResultCollection results = (IOpenSearchResultCollection)osr.Result;

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

            Type type = typeof(AtomFeed);

            if (request.Params["format"] != null) {
                var osee = ose.GetExtensionByExtensionName(request.Params["format"]);
                if (osee != null) {
                    type = osee.GetTransformType();
                }
            } else {
                foreach (string contentType in request.AcceptTypes) {
                    var osee = ose.GetExtensionByContentTypeAbility(contentType);
                    if (osee != null) {
                        type = osee.GetTransformType();
                        break;
                    }
                }
            }

            return type;
        }

        public static void ReplaceSelfLinks(IOpenSearchResult osr, Func<IOpenSearchResultItem,OpenSearchDescription,string,string> entryTemplate) {
            IOpenSearchResultCollection feed = osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            OpenSearchDescription osd = null;
            if (osr.OpenSearchableEntity is IProxiedOpenSearchable) {
                IProxiedOpenSearchable entity = (IProxiedOpenSearchable)osr.OpenSearchableEntity;
                osd = entity.GetProxyOpenSearchDescription(); 
            } else {
                osd = osr.OpenSearchableEntity.GetOpenSearchDescription();
            }
            if (OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/atom+xml") == null)
                return;
            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/atom+xml").Template);
            string[] queryString = Array.ConvertAll(osr.SearchParameters.AllKeys, key => string.Format("{0}={1}", key, osr.SearchParameters[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Links.Add(new SyndicationLink(myUrl.Uri, "self", "Reference link", "application/atom+xml", 0));

            foreach (IOpenSearchResultItem item in feed.Items) {
                string template = entryTemplate(item, osd, "application/atom+xml");
                if (template != null) {
                    item.Links.Add(new SyndicationLink(new Uri(template), "self", "Reference link", "application/atom+xml", 0));
                }
            }
        }

        public static void ReplaceOpenSearchDescriptionLinks(IOpenSearchResult osr) {
            IOpenSearchResultCollection feed = osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "search").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            OpenSearchDescription osd;
            if (osr.OpenSearchableEntity is IProxiedOpenSearchable) osd = ((IProxiedOpenSearchable)osr.OpenSearchableEntity).GetProxyOpenSearchDescription();
            else osd = osr.OpenSearchableEntity.GetOpenSearchDescription();
            OpenSearchDescriptionUrl url = OpenSearchFactory.GetOpenSearchUrlByRel(osd, "self");
            if (url != null) feed.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));

            foreach (IOpenSearchResultItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "search").ToArray();
                foreach (var link in matchLinks) {
                    item.Links.Remove(link);
                }
                if (url != null) item.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));
            }

        }

<<<<<<< HEAD
        public static OpenSearchDescriptionUrl GetOpenSearchUrlByTypeAndMaxParam(OpenSearchDescription osd, List<string> mimeTypes, NameValueCollection osParameters) {

            OpenSearchDescriptionUrl url = null;
            int maxParam = -1;

            foreach (var urlC in osd.Url) {
                UriBuilder tempU = new UriBuilder(BuildRequestUrlFromTemplateNameParameters(new OpenSearchUrl(urlC.Template), osParameters));
                int numParam = HttpUtility.ParseQueryString(tempU.Query).Count;
                if (maxParam < numParam){
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
=======
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
>>>>>>> 813935fe9596ad0d0ed5cab432b44d4bc2288b60
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
