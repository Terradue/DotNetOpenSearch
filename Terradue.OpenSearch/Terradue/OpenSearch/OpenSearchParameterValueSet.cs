namespace Terradue.OpenSearch {



    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------



    /// <summary>Represents the search values related to an OpenSearch request and provides useful methods for managing and converting OpenSearch values.</summary>
    public class OpenSearchParameterValueSet {

        private Dictionary<string, OpenSearchParameterDefinition> parametersByName;
        private Dictionary<string, OpenSearchParameterDefinition> parametersByIdentifier;
        private Dictionary<OpenSearchParameterDefinition, string[]> values;

        private static Regex urlRegex = new Regex(@"[^\?]+\?(.*)");
        private static Regex parameterDefinitionRegex = new Regex(@"([^=]+)=(\{([^\}\?]+)\??\}|.*)");

        //---------------------------------------------------------------------------------------------------------------------

        protected OpenSearchParameterValueSet() {
            parametersByName = new Dictionary<string, OpenSearchParameterDefinition>();
            parametersByIdentifier = new Dictionary<string, OpenSearchParameterDefinition>();
            values = new Dictionary<OpenSearchParameterDefinition, string[]>();
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an OpenSearchParameterValueSet instance based on the specified OpenSearchDescriptionUrl object.</summary>
        /// <returns>The created OpenSearchParameterValueSet instance.</returns>
        /// <param name="osdUrl">An OpenSearchDescriptionUrl object from a deserialized OpenSearch description document.</param>
        public static OpenSearchParameterValueSet FromOpenSearchDescription(OpenSearchDescriptionUrl osdUrl) {
            return FromOpenSearchDescription(osdUrl.Template, osdUrl.Parameters);
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an OpenSearchParameterValueSet instance based on the specified OpenSearch template.</summary>
        /// <returns>The created OpenSearchParameterValueSet instance.</returns>
        /// <param name="template">An OpenSearch template URL.</param>
        public static OpenSearchParameterValueSet FromOpenSearchDescription(string template) {
            return FromOpenSearchDescription(template, null);
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an OpenSearchParameterValueSet instance based on the specified OpenSearch template and optional additional parameter information.</summary>
        /// <returns>A new OpenSearchParameterValueSet instance.</returns>
        /// <param name="template">An OpenSearch template URL.</param>
        /// <param name="origParams">An array of objects containing parameter information as defined by the OpenSearch Parameter extension.</returns>
        public static OpenSearchParameterValueSet FromOpenSearchDescription(string template, OpenSearchDescriptionUrlParameter[] origParams) {
            Dictionary<string, OpenSearchDescriptionUrlParameter> tempParameters = new Dictionary<string, OpenSearchDescriptionUrlParameter>();
            if (origParams != null) {
                foreach (OpenSearchDescriptionUrlParameter origParam in origParams) tempParameters.Add(origParam.Name, origParam);
            }

            OpenSearchParameterValueSet result = new OpenSearchParameterValueSet();

            // Make sure URL is valid
            Match match = urlRegex.Match(template);
            if (!match.Success) throw new Exception(String.Format("Invalid URL template: {0}", template));

            // Split by query string parameter and add parameter definitions to the internal dictionaries:
            // parameters can be settable (e.g. name={key}, name={prefix:key}) or fixed (name=value)
            string[] items = match.Groups[1].Value.Split('&');
            foreach (string item in items) {
                Match match2 = parameterDefinitionRegex.Match(item);
                if (!match2.Success) continue;
                string name = match2.Groups[1].Value;
                OpenSearchParameterDefinition paramDef;
                if (match2.Groups[3].Success) { // parameter is settable
                    string identifier = match2.Groups[3].Value;
                    paramDef = new OpenSearchParameterDefinition(identifier, name, tempParameters.ContainsKey(name) ? tempParameters[name] : null);
                    result.parametersByIdentifier[identifier] = paramDef;
                } else { // parameter is fixed
                    paramDef = new OpenSearchParameterDefinition(name);
                    result.values[paramDef] = new string[] {match2.Groups[2].Value};
                }
                result.parametersByName[paramDef.Name] = paramDef;
            }

            return result;
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Adds an extra parameter not defined by the OpenSearch description.</summary>
        /// <remarks>This method can be useful if there are unpublicized OpenSearch parameters that need to be set. In such cases it is necessary to add the parameter definition before setting its value.</remarks>
        /// <param name="name">The name of the parameter.</param>
        public void AddExtraParameter(string name) {
            if (parametersByName.ContainsKey(name)) return;
            parametersByName.Add(name, new OpenSearchParameterDefinition(name));
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets the parameter values based on the values of another instance.</summary>
        /// <remarks>Only matching parameters are taken into account. The match is made by the fully qualified identifier of the OpenSearch parameters. It is assumed that the namespace prefixes used for the identifiers are the same in both value sets.</remarks>
        /// <param name="source">The OpenSearchParameterValueSet that serves as source.</param>
        public void TranslateFrom(OpenSearchParameterValueSet source) {
            foreach (string identifier in parametersByIdentifier.Keys) {
                string[] sourceValues = source.GetValuesByIdentifier(identifier);
                if (sourceValues != null) this.values[parametersByIdentifier[identifier]] = sourceValues;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets the parameter values based on the specified NameValueCollection.</summary>
        /// <remarks>Only matching parameters are taken into account. The match is made by the query string name of the parameter.</remarks>
        /// <param name="values">The NameValueCollection containing the values to be used.</param>
        public void SetValues(NameValueCollection values) {
            foreach (string name in values.AllKeys) {
                if (!parametersByName.ContainsKey(name) || parametersByName[name].IsFixed) continue;
                this.values[parametersByName[name]] = values.GetValues(name);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets the value for the OpenSearch parameter specified by its name.</summary>
        /// <param name="name">The parameter name, as in the query string.</param>
        /// <param name="values">The value.</param>
        public void SetValueByName(string name, string value) {
            SetValuesByName(name, new string[] {value});
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets multiple values for the OpenSearch parameter specified by its name.</summary>
        /// <param name="name">The parameter name, as in the query string.</param>
        /// <param name="values">An array containing the values.</param>
        public void SetValuesByName(string name, IEnumerable<string> values) {
            if (!parametersByName.ContainsKey(name)) throw new Exception(String.Format("Parameter \"{0}\" is unknown", name));
            if (parametersByName[name].IsFixed) throw new Exception(String.Format("Parameter \"{0}\" has a fixed value", name));
            this.values[parametersByName[name]] = values.ToArray();
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets the value for the OpenSearch parameter specified by its identifier.</summary>
        /// <param name="identifier">The parameter identifier, i.e. the fully qualified identifier between the curly brackets in the OpenSearch description URL template, e.g. "geo:box".</param>
        /// <param name="value">The value.</param>
        public void SetValueByIdentifier(string identifier, string value) {
            SetValuesByIdentifier(identifier, new string[] {value});
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Sets multiple values for the OpenSearch parameter specified by its identifier.</summary>
        /// <param name="identifier">The parameter identifier, i.e. the fully qualified identifier between the curly brackets in the OpenSearch description URL template, e.g. "geo:box".</param>
        /// <param name="values">An array containing the values.</param>
        public void SetValuesByIdentifier(string identifier, IEnumerable<string> values) {
            if (!parametersByIdentifier.ContainsKey(identifier)) throw new Exception(String.Format("Parameter with identifier \"{0}\" is unknown", identifier));
            this.values[parametersByIdentifier[identifier]] = values.ToArray();
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Returns the values of the parameter specified by its name.</summary>
        /// <returns>An array containing the values.</returns>
        /// <param name="name">The parameter name, as in the query string.</param>
        public string[] GetValuesByName(string name) {
            if (!parametersByName.ContainsKey(name)) return null;
            return values[parametersByName[name]];
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Returns the values of the parameter specified by its identifier.</summary>
        /// <returns>An array containing the values.</returns>
        /// <param name="identifier">The parameter identifier, i.e. the fully qualified identifier between the curly brackets in the OpenSearch description URL template, e.g. "geo:box".</param>
        public string[] GetValuesByIdentifier(string identifier) {
            if (!parametersByIdentifier.ContainsKey(identifier)) return null;
            return values[parametersByIdentifier[identifier]];
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets a NameValueCollection representing the parameters and their values.</summary>
        /// <returns>The NameValueCollection object representing the parameters.</returns>
        /// <param name="allParameters">If set to <c>true</c>, the NameValueCollection includes also unset parameters.</param>
        public NameValueCollection GetValues(bool allParameters) {
            NameValueCollection result = new NameValueCollection();
            foreach (OpenSearchParameterDefinition paramDef in parametersByName.Values) {
                if (!values.ContainsKey(paramDef)) {
                    if (allParameters) result.Add(paramDef.Name, String.Empty);
                    continue;
                }
                foreach (string value in values[paramDef]) {
                    result.Add(paramDef.Name, value);
                }
            }
            return result;
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets a NameValueCollection representing the parameters and their values.</summary>
        /// <returns>The query string segment for the parameters.</returns>
        /// <param name="allParameters">If set to <c>true</c>, the NameValueCollection includes also unset parameters.</param>
        public string GetQueryString(bool allParameters) {
            StringBuilder result = new StringBuilder(256);
            bool first = true;
            foreach (OpenSearchParameterDefinition paramDef in parametersByName.Values) {
                if (!values.ContainsKey(paramDef)) {
                    if (allParameters) {
                        result.Append(String.Format("{1}{0}=", paramDef.Name, first ? String.Empty : "&"));
                        first = false;
                    }
                    continue;
                }
                foreach (string value in values[paramDef]) {
                    result.Append(String.Format("{2}{0}={1}", paramDef.Name, value, first ? String.Empty : "&"));
                    first = false;
                }
            }
            return result.ToString();
        }

    }



    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------



    public class OpenSearchParameterDefinition {

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets or sets (protected) the query string name of this parameter, e.g. "bbox".</summary>
        public string Name { get; protected set; }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets or sets (protected) the fully qualified identifier of this parameter, e.g. "geo:box".</summary>
        public string Identifier { get; protected set; }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Indicates or decides (protected) whether the parameter has a fixed value.</summary>
        /// <remarks>Parameters with a fixed value cannot have an identifier.</remarks>
        public bool IsFixed { get; protected set; }

        //---------------------------------------------------------------------------------------------------------------------

        public OpenSearchDescriptionUrlParameter Parameter { get; protected set; }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an instance of an OpenSearchParameterDefinition.</summary>
        /// <param name="identifier">The fully qualified identifier of the parameter, e.g. "geo:box".</param>
        /// <param name="name">The query string name of the parameter, e.g. "bbox".</param>
        /// <param name="parameter">An optional reference to the OpenSearch Parameter extension object that contains further information about the parameter, such as options for values.</param>
        public OpenSearchParameterDefinition(string identifier, string name, OpenSearchDescriptionUrlParameter parameter) {
            this.Identifier = identifier;
            this.Name = name;
            this.Parameter = parameter;
        }

        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an instance of an OpenSearchParameterDefinition representing a fixed-value parameter.</summary>
        /// <param name="name">The query string name of the parameter, e.g. "format".</param>
        public OpenSearchParameterDefinition(string name) {
            this.Name = name;
            this.IsFixed = true;
        }

    }

}