namespace Terradue.OpenSearch {

	using System.Runtime.Serialization;

	using System.Collections.ObjectModel;

	using System.Xml.Serialization;

	using System.Xml;

	using System;

	/// <remarks/>
    /// \ingroup modules_openSearchEngine
	[DataContract(Name="OpenSearchDescription",Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	[SerializableAttribute()]
	[XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	[XmlRootAttribute(Namespace="http://a9.com/-/spec/opensearch/1.1/", IsNullable=false)]
    public partial class OpenSearchDescription {
		
		private string shortNameField;
		
		private string descriptionField;
		
		private string tagsField;
		
		private string contactField;
		
		private OpenSearchDescriptionUrl[] urlField;
		
		private string longNameField;
		
		private OpenSearchDescriptionImage[] imageField;
		
		private OpenSearchDescriptionQuery queryField;
		
		private string developerField;
		
		private string attributionField;
		
		private string syndicationRightField;
		
		private string adultContentField;
		
		private string languageField;
		
		private string outputEncodingField;
		
		private string inputEncodingField;
		
		/// <remarks/>
        [DataMember]
		public string ShortName {
			get {
				return this.shortNameField;
			}
			set {
				this.shortNameField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
		public string Description {
			get {
				return this.descriptionField;
			}
			set {
				this.descriptionField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string Tags {
			get {
				return this.tagsField;
			}
			set {
				this.tagsField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string Contact {
			get {
				return this.contactField;
			}
			set {
				this.contactField = value;
			}
		}
		
		/// <remarks/>
		[XmlElementAttribute("Url")]
        [DataMember]
        public OpenSearchDescriptionUrl[] Url {
			get {
				return this.urlField;
			}
			set {
				this.urlField = value;
			}
        }   
		
		/// <remarks/>
        [DataMember]
        public string LongName {
			get {
				return this.longNameField;
			}
			set {
				this.longNameField = value;
			}
		}
		
		/// <remarks/>
		[XmlElementAttribute("Image")]
        [DataMember]
        public OpenSearchDescriptionImage[] Image {
			get {
				return this.imageField;
			}
			set {
				this.imageField = value;
			}
		}
		
		/// <remarks/> 
        [DataMember]
        public OpenSearchDescriptionQuery Query {
			get {
				return this.queryField;
			}
			set {
				this.queryField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string Developer {
			get {
				return this.developerField;
			}
			set {
				this.developerField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string Attribution {
			get {
				return this.attributionField;
			}
			set {
				this.attributionField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string SyndicationRight {
			get {
				return this.syndicationRightField;
			}
			set {
				this.syndicationRightField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string AdultContent {
			get {
				return this.adultContentField;
			}
			set {
				this.adultContentField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string Language {
			get {
				return this.languageField;
			}
			set {
				this.languageField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string OutputEncoding {
			get {
				return this.outputEncodingField;
			}
			set {
				this.outputEncodingField = value;
			}
		}
		
		/// <remarks/>
        [DataMember]
        public string InputEncoding {
			get {
				return this.inputEncodingField;
			}
			set {
				this.inputEncodingField = value;
			}
		}



		public OpenSearchDescriptionUrl GetUrlByType (string type)
		{
			foreach (OpenSearchDescriptionUrl url in urlField) {
				if ( url.Type == type )
					return url;
			}

			return null;
		}
	}
	
	/// <remarks/>
	[DataContract(Name="Url",Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	[SerializableAttribute()]
	[XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	public partial class OpenSearchDescriptionUrl {
		
		private string typeField;
		
		private string templateField;
		
		private string relField;

		private int pageOffset = 1;
		
		private int indexOffset = 1;

		public OpenSearchDescriptionUrl () {}

		public OpenSearchDescriptionUrl ( string type, string template, string rel ){
			this.Relation = rel;
			this.Template = template;
			this.Type = type;
		}
		
		/// <remarks/>
		[XmlAttribute(AttributeName="type")]
        [DataMember]
        public string Type {
			get {
				return this.typeField;
			}
			set {
				this.typeField = value;
			}
		}
		
		/// <remarks/>
		[XmlAttribute(AttributeName="template")]
        [DataMember]
        public string Template {
			get {
				return this.templateField;
			}
			set {
				this.templateField = value;
			}
		}
		
		/// <remarks/>
		[XmlAttribute(AttributeName="rel")]
        [DataMember]
        public string Relation {
			get {
				return this.relField;
			}
			set {
				this.relField = value;
			}
		}

		[XmlAttribute(AttributeName="pageOffset")]
        [DataMember]
        public int PageOffset {
			get {
				return pageOffset;
			}
			set {
				pageOffset = value;
			}
		}

		[XmlAttribute(AttributeName="indexOffset")]
        [DataMember]
        public int IndexOffset {
			get {
				return indexOffset;
			}
			set {
				indexOffset = value;
			}
		}
	}
	
	/// <remarks/>
	[DataContract(Name="Image",Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	[SerializableAttribute()]
	[XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	public partial class OpenSearchDescriptionImage {

		private int heightField;

		private int widthField;
		
		private string typeField1;
		
		private string valueField1;
		
		/// <remarks/>
		[XmlAttribute]
        [DataMember]
        public int height {
			get {
				return this.heightField;
			}
			set {
				this.heightField = value;
			}
		}

		/// <remarks/>
		[XmlAttribute]
        [DataMember]
        public int width {
			get {
				return this.widthField;
			}
			set {
				this.widthField = value;
			}
		}
		
		/// <remarks/>
		[XmlAttribute]
        [DataMember]
        public string type {
			get {
				return this.typeField1;
			}
			set {
				this.typeField1 = value;
			}
		}
		
		/// <remarks/>
		[XmlText]
        [DataMember]
        public string Value {
			get {
				return this.valueField1;
			}
			set {
				this.valueField1 = value;
			}
		}
	}
	
	/// <remarks/>
	[DataContract(Name="Query",Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	[SerializableAttribute()]
	[XmlTypeAttribute(Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	public partial class OpenSearchDescriptionQuery {
		
		private string roleField;
		
		private string searchTermsField;
		
		private string valueField2;
		
		/// <remarks/>
		[XmlAttribute]
        [DataMember]
        public string role {
			get {
				return this.roleField;
			}
			set {
				this.roleField = value;
			}
		}
		
		/// <remarks/>
		[XmlAttribute]
        [DataMember]
        public string searchTerms {
			get {
				return this.searchTermsField;
			}
			set {
				this.searchTermsField = value;
			}
		}
		
		/// <remarks/>
		[XmlText]
        [DataMember]
        public string Value {
			get {
				return this.valueField2;
			}
			set {
				this.valueField2 = value;
			}
		}
	}
   
}
