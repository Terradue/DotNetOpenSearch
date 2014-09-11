//
//  RdfXmlResult.cs
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

        public RdfXmlResult(IOpenSearchResultItem item) : base() {

            if (!string.IsNullOrEmpty(item.Title))
                Title = item.Title;
            if (item.Date.Ticks != 0)
                Date = item.Date;
            if (!string.IsNullOrEmpty(item.Identifier))
                Identifier = item.Identifier;
            elementExtensions = item.ElementExtensions;
            links = item.Links;

        }

        public RdfXmlResult(XElement root) : base() {

            Id = root.Attribute(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")).Value;
            Title = root.Element(XName.Get("title", "http://purl.org/dc/elements/1.1/")).Value;
            Date = DateTime.Parse(root.Element(XName.Get("date", "http://purl.org/dc/elements/1.1/")).Value);
            Identifier = root.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            elementExtensions = RdfXmlDocument.XElementsToElementExtensions(root.Elements());
        }

        public XElement GetElement() {
            XElement root = new XElement(XName.Get("DataSet", RdfXmlDocument.dclite4gns.NamespaceName));
            root.SetAttributeValue(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"), Id);
            root.Add(new XElement(XName.Get("title", "http://purl.org/dc/elements/1.1/"), Title));
            root.Add(new XElement(XName.Get("date", "http://purl.org/dc/elements/1.1/"), Date.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
            root.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), Identifier));

            XElement exts = XElement.Load(ElementExtensions.GetReaderAtExtensionWrapper());
            root.Add(exts.Elements());
            // TODO : to be moved to EO specific
            if (exts.Descendants(XName.Get("beginPosition", "http://www.opengis.net/gml/3.2")).Count() > 0) {
                root.Add(new XElement(XName.Get("dtstart", "http://www.w3.org/2002/12/cal/ical#"), exts.Descendants(XName.Get("beginPosition", "http://www.opengis.net/gml/3.2")).First().Value));
            }
            if (exts.Descendants(XName.Get("endPosition", "http://www.opengis.net/gml/3.2")).Count() > 0) {
                root.Add(new XElement(XName.Get("dtend", "http://www.w3.org/2002/12/cal/ical#"), exts.Descendants(XName.Get("endPosition", "http://www.opengis.net/gml/3.2")).First().Value));
            }
            root.SetAttributeValue(XName.Get("ical", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/2002/12/cal/ical#");
            root.Add(GetOnlineResources());
            return root;
        }

        IEnumerable<XElement> GetOnlineResources() {
            List<XElement> elements = new List<XElement>();
            var enclosures = Links.Where(l => l.RelationshipType == "enclosure");
            foreach (var enclosure in enclosures) {
                UriBuilder uri = new UriBuilder(enclosure.Uri);
                XElement onlineResource = new XElement(XName.Get("onlineResource", RdfXmlDocument.dclite4gns.NamespaceName));
                onlineResource.SetAttributeValue(XName.Get("ws", XNamespace.Xmlns.NamespaceName), "http://dclite4g.xmlns.com/ws.rdf#");
                XElement ws = new XElement(XName.Get(uri.Scheme.ToUpper(), RdfXmlDocument.wsns.NamespaceName));
                ws.SetAttributeValue(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName), uri.ToString());
                onlineResource.Add(ws);
                elements.Add(onlineResource);
            }
            return elements;
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                var link = Links.FirstOrDefault(l => l.RelationshipType == "self");
                return link == null ? null : link.Uri.ToString();
            }
            set {
            }
        }

        public string Title {
            get ;
            set ;
        }

        public DateTime Date {
            get ;
            set ;
        }

        public string Identifier {
            get ;
            set ;
        }

        SyndicationElementExtensionCollection elementExtensions = new SyndicationElementExtensionCollection();

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links = new Collection<SyndicationLink>();

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
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

        readonly Collection<SyndicationCategory> categories = new Collection<SyndicationCategory>();

        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        readonly Collection<SyndicationPerson> authors = new Collection<SyndicationPerson>();

        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }

        #endregion

       
    }
}
