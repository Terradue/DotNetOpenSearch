//
//  RdfXmlDocument.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;

namespace Terradue.OpenSearch.Result {

    public class RdfXmlResult : IOpenSearchResultItem {

        XElement root;

        public RdfXmlResult(IOpenSearchResultItem item) : base() {

            root = new XElement(XName.Get("dataset", RdfXmlDocument.dclite4gns.NamespaceName));

            root.SetAttributeValue(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName), item.Id);
            if (!string.IsNullOrEmpty(item.Title))
                Title = item.Title;
            if (item.Date.Ticks != 0)
                Date = item.Date;
            if (!string.IsNullOrEmpty(item.Identifier))
                Identifier = item.Identifier;
            foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                root.Add(XElement.Load(ext.GetReader()));
            }

        }

        public RdfXmlResult(XElement root) : base() {

            this.root = root;

        }

        public XElement Root {
            get {
                return root;
            }
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                return root.Attribute(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")).Value;
            }
            set{ 
                root.SetAttributeValue(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"), value);
            }
        }

        public string Title {
            get {
                try {
                    return root.Element(XName.Get("title", "http://purl.org/dc/elements/1.1/")).Value;
                } catch {
                    return null;
                }
            }
            set {
                root.Elements(XName.Get("title", "http://purl.org/dc/elements/1.1/")).Remove();
                root.Add(new XElement(XName.Get("title", "http://purl.org/dc/elements/1.1/"), value));
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(root.Element(XName.Get("date", "http://purl.org/dc/elements/1.1/")).Value);
                } catch {
                    return DateTime.UtcNow;
                }
            }
            set {
                root.Elements(XName.Get("date", "http://purl.org/dc/elements/1.1/")).Remove();
                root.Add(new XElement(XName.Get("date", "http://purl.org/dc/elements/1.1/"), value.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
            }
        }

        public string Identifier {
            get {
                return root.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            }
            set {
                root.Elements(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Remove();
                root.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), value));
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return RdfXmlDocument.ChildrenElementToSyndicationElementExtensionCollection(root);
            }
        }

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                IEnumerable<SyndicationLink> col = root.Elements(XName.Get("link", "http://www.w3.org/2005/Atom"))
                    .Select(l => RdfXmlDocument.SyndicationLinkFromXElement(l));
                return new Collection<SyndicationLink>(col.ToList());
            }
        }

        public bool ShowNamespaces {
            get {
                return true;
            }
            set {
                ;
                ;
            }
        }

        readonly Collection<SyndicationCategory> categories;
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        readonly Collection<SyndicationPerson> authors;
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }
        #endregion
    }
}
