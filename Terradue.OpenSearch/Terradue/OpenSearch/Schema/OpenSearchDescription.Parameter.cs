//
//  OpenSearchDescription.Parameter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

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
        [XmlElementAttribute("Url")]
        [DataMember]
        public OpenSearchDescriptionUrlParameter[] Url {
            get {
                return this.parameterField;
            }
            set {
                this.parameterField = value;
            }
        }  
		
		
	}
	
	/// <remarks/>
    [DataContract(Name="Paraemeter",Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
	[SerializableAttribute()]
    [XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/")]
    public partial class OpenSearchDescriptionUrlParameter {
		
        private string nameField;
		
        private string valueField;
		
		private int minimumField = 1;

        private int maximumField = 1;
		
		/// <remarks/>
        [XmlAttribute(AttributeName="type")]
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
        public int Minimum {
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
        public int Maximum {
            get {
                return this.maximumField;
            }
            set {
                this.maximumField = value;
            }
        }
	}
   
}
