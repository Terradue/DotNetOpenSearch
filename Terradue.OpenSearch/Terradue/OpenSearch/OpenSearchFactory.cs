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
using System.ServiceModel.Syndication;
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
            if (param == null) return null;

            // first find the defintion of the parameter in the url template
            Match matchParamDef = Regex.Match(param, @"^{([^?]+)\??}$");
            // If parameter does not exist, continue
            if (!matchParamDef.Success) return null;
            // We have the parameter defintion
            return matchParamDef.Groups[1].Value;

            return null;
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
        /// Bests the transform function by number of parameter.
        /// </summary>
        /// <returns>The transform function by number of parameter.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="osee">Osee.</param>
        public static Tuple<string, Func<OpenSearchResponse, object>> BestTransformFunctionByNumberOfParam(IOpenSearchable entity, IOpenSearchEngineExtension osee) {
            string contentType = null;
            int paramnumber = -1;
            foreach (string mimeType in osee.GetInputFormatTransformPath()) {
                NameValueCollection nvc = entity.GetOpenSearchParameters(mimeType);
                if (nvc == null)
                    continue;
                if (nvc.Count > paramnumber) {
                    contentType = mimeType;
                    paramnumber = nvc.Count;
                }
            }
            return new Tuple<string, Func<OpenSearchResponse, object>>(contentType, osee.TransformResponse);
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
            }
            else
                throw new EntryPointNotFoundException(string.Format("No OpenSearch description found around {0}", baseUrl));
        }

        /// <summary>
        /// Gets the name of the identifier from parameter.
        /// </summary>
        /// <returns>The identifier from parameter name.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="paramName">Parameter name.</param>
        public static string GetIdFromParamName (IOpenSearchable entity, string mimeType, string paramName){

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
        public void Test () {
            var engine = new OpenSearchEngine();
            engine.LoadPlugins();
            var entity = new GenericOpenSearchable(new OpenSearchUrl("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/atom"), engine);
            var parameters = new NameValueCollection();
            parameters.Add("count", "20");
            parameters.Add("start", "1992-01-01");
            parameters.Add("stop", "2014-04-15");
            parameters.Add("bbox", "24,30,42,53");
            var result = engine.Query(entity, parameters, "Atom");
            XmlWriter atomWriter = XmlWriter.Create("result.xml");
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter((SyndicationFeed)result.Result);
            atomFormatter.WriteTo(atomWriter);
            atomWriter.Close();
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
