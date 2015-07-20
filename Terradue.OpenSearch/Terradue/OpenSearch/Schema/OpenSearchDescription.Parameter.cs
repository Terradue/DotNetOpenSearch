//
//  OpenSearchDescription.Parameter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
using System.Collections.Generic;

namespace Terradue.OpenSearch.Schema {

	using System.Runtime.Serialization;

	using System.Collections.ObjectModel;

	using System.Xml.Serialization;

	using System.Xml;

	using System;

	/// <remarks/>
	public partial class OpenSearchDescriptionUrl {
		
        private string methodField;
		
        private string enctypeField;
		
        private OpenSearchDescriptionUrlParameter[] parameterField;
		
		/// <remarks/>
        [XmlAttribute(AttributeName="method", Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
        [DataMember]
        public string ParametersMethod {
			get {
                return this.methodField;
			}
			set {
                this.methodField = value;
			}
		}
		
		/// <remarks/>
        [XmlAttribute(AttributeName="enctype", Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
        [DataMember]
        public string ParametersEncodingType {
			get {
                return this.enctypeField;
			}
			set {
                this.enctypeField = value;
			}
		}

        /// <remarks/>
        [XmlElement("Parameter", Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
        public OpenSearchDescriptionUrlParameter[] Parameters {
            get {
                return this.parameterField;
            }
            set {
                this.parameterField = value;
            }
        }  
		
		
	}
	
	/// <remarks/>
    [DataContract(Name="Parameter",Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
	[SerializableAttribute()]
    [XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
    public partial class OpenSearchDescriptionUrlParameter {
		
        private string nameField;
		
        private string valueField;
		
		private string minimumField = null;

        private string maximumField = null;

        private string pattern = null;

        private string title = null;

        private string minExclusive = null;

        private string maxExclusive = null;

        private string minInclusive = null;

        private string maxInclusive = null;

        private string step = null;

        private List<OpenSearchDescriptionUrlParameterOption> options = new List<OpenSearchDescriptionUrlParameterOption>();
		
		/// <remarks/>
        [XmlAttribute(AttributeName="name")]
        [DataMember]
        public string Name {
			get {
                return this.nameField;
			}
			set {
                this.nameField = value;
			}
		}
		
		/// <remarks/>
        [XmlAttribute(AttributeName="value")]
        [DataMember]
        public string Value {
			get {
                return this.valueField;
			}
			set {
                this.valueField = value;
			}
		}
		
		/// <remarks/>
        [XmlAttribute(AttributeName="minimum")]
        [DataMember]
        public string Minimum {
			get {
                return this.minimumField;
			}
			set {
                this.minimumField = value;
			}
		}

        /// <remarks/>
        [XmlAttribute(AttributeName="maximum")]
        [DataMember]
        public string Maximum {
            get {
                return this.maximumField;
            }
            set {
                this.maximumField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="pattern")]
        [DataMember]
        public string Pattern {
            get {
                return this.pattern;
            }
            set {
                this.pattern = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="title")]
        [DataMember]
        public string Title {
            get {
                return this.title;
            }
            set {
                this.title = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="minExclusive")]
        [DataMember]
        public string MinExclusive {
            get {
                return this.minExclusive;
            }
            set {
                this.minExclusive = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="maxExclusive")]
        [DataMember]
        public string MaxExclusive {
            get {
                return this.maxExclusive;
            }
            set {
                this.maxExclusive = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="minInclusive")]
        [DataMember]
        public string MinInclusive {
            get {
                return this.minInclusive;
            }
            set {
                this.minInclusive = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="maxInclusive")]
        [DataMember]
        public string MaxInclusive {
            get {
                return this.maxInclusive;
            }
            set {
                this.maxInclusive = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="step")]
        [DataMember]
        public string Step {
            get {
                return this.step;
            }
            set {
                this.step = value;
            }
        }

        [XmlElement("Option")]
        public List<OpenSearchDescriptionUrlParameterOption> Options {
            get {
                return options;
            }
            set {
                options = value;
            }
        }

        [XmlAnyElement]
        public XmlElement[] Any {
            get ;
            set ;
        }
	}

    /// <remarks/>
    [DataContract(Name="Parameter",Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
    public partial class OpenSearchDescriptionUrlParameterOption {

        private string valueField;

        private string labelField;

        /// <remarks/>
        [XmlAttribute(AttributeName="value")]
        [DataMember]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute(AttributeName="label")]
        [DataMember]
        public string Label {
            get {
                return this.labelField;
            }
            set {
                this.labelField = value;
            }
        }
    }
   
}
