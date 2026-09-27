#region Related components
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
#endregion

namespace net.vieapps.Components.Utility.Epub
{
	internal class Metadata
	{
        internal class Item
        {
            private readonly XName _tagName;
            private readonly string _content;
            private readonly IDictionary<XName, string> attributes = new Dictionary<XName, string>();

            internal Item(string tagContent, XName tagName)
            {
                this._content = tagContent;
                this._tagName = tagName;
            }

            internal string GetAttribute(XName name)
            {
                return attributes[name];
            }

            internal void SetAttribute(XName name, string value)
            {
                attributes[name] = value;
            }

            internal XElement ToElement()
            {
                var element = new XElement(_tagName);
                if (_content != null)
                {
                    element.SetValue(_content);
                }

                foreach (var attr in attributes)
                {

                    element.SetAttributeValue(attr.Key, attr.Value);
                }
                return element;
            }
        }

        private static readonly string iso8601_1FormatString = "yyyy-MM-ddTHH:mm:ssZ";
        private static readonly XName opfMeta = Document.OpfNS + "meta";
        internal readonly Document currentDoc;
        internal readonly Dictionary<string, int> _ids = new Dictionary<string, int>();
		readonly List<Item> _items = new List<Item>();

        internal Metadata(Document doc)
        {
            currentDoc = doc;
        }

        internal Item AddItem(string content)
            => AddItem(content, opfMeta);

        internal Item AddItem(string content, XName tagName)
        {
            var item = new Item(content, tagName);
            item.SetAttribute("id", currentDoc.GetNextID(tagName.LocalName));
            this._items.Add(item);
            return item;
        }

		internal XElement ToElement()
		{
            XNamespace aps = "https://schema.org/CreativeWork";
            var element = new XElement(Document.OpfNS + "metadata",
                new XAttribute(XNamespace.Xmlns + "dc", Document.DcNS),
                new XAttribute(XNamespace.Xmlns + "opf", Document.OpfNS),
                new XAttribute(XNamespace.Xmlns + "aps", Document.apsNS)
                );
			this._items.ForEach(item => element.Add(item.ToElement()));
			return element;
        }

        internal void AddAccessibilityProperty(string name, string value)
        {
            Item accesibilityItem = AddItem(value);
            accesibilityItem.SetAttribute("property", "schema:" + name);
        }

        internal void AddAccessibilityProperty(string name, string[] values)
        {
            foreach (var value in values)
            {
                AddAccessibilityProperty(name, value);
            }
        }

        internal void AddAccessibilityCertification(string conformsTo,
            string certifiedBy,
            DateTime certificationDate,
            string certifierCredentials = null,
            string certifierReportUri = null,
            string certifierReportMimeType = "text/html")
        {
            Item conformsToItem = AddItem(conformsTo);
            conformsToItem.SetAttribute("property", "dcterms:conformsTo");
            string conformsToRefinesId = "#" + conformsToItem.GetAttribute("id");

            Item certifiedByItem = AddItem(certifiedBy);
            certifiedByItem.SetAttribute("property", "a11y:certifiedBy");
            certifiedByItem.SetAttribute("refines", conformsToRefinesId);
            string certifiedByRefinesId = "#" + certifiedByItem.GetAttribute("id");

            Item certifiedDateItem = AddItem(certificationDate.ToString(iso8601_1FormatString));
            certifiedDateItem.SetAttribute("property", "dcterms:date");
            certifiedDateItem.SetAttribute("refines", certifiedByRefinesId);

            if (!string.IsNullOrWhiteSpace(certifierCredentials))
            {
                Item credItem = AddItem(certifierCredentials);
                credItem.SetAttribute("refines", certifiedByRefinesId);
                credItem.SetAttribute("property", "a11y:certifierCredential");
            }

            if (!string.IsNullOrWhiteSpace(certifierReportUri))
            {
                Item reportUriItem = AddItem(null, Document.OpfNS + "link");
                reportUriItem.SetAttribute("rel", "a11y:certifierReport");
                reportUriItem.SetAttribute("refines", certifiedByRefinesId); 
                reportUriItem.SetAttribute("href", certifierReportUri);
                reportUriItem.SetAttribute("media-type", certifierReportMimeType);
            }
        }

        internal void AddCreator(string name, string role)
		{
            Item creatorItem = AddItem(name, Document.DcNS + "creator");
            string refinesId = "#" + creatorItem.GetAttribute("id");

			if (!string.IsNullOrWhiteSpace(role))
            {
                Item roleItem = AddItem(role);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "role");
                roleItem.SetAttribute("scheme", "marc:relators");
            }
        }

		internal void AddCreator(string name)
			=> AddCreator(name, null);


        internal void AddAuthor(string name) 
			=> this.AddCreator(name, "aut");

        internal void AddTranslator(string name)
            => this.AddCreator(name, "trl");

        internal void AddArtist(string name)
            => this.AddCreator(name, "art");

        internal void AddContributor(string name, string role)
        {
            Item creatorItem = AddItem(name, Document.DcNS + "contributor");
            string refinesId = "#" + creatorItem.GetAttribute("id");

            if (!string.IsNullOrWhiteSpace(role))
            {
                Item roleItem = AddItem(role);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "role");
                roleItem.SetAttribute("scheme", "marc:relators");
            }
        }

        internal void AddContributor(string name)
			=> AddContributor(name, null);

        internal void AddSubject(string subject, string authority = null, string term = null)
        {
            Item creatorItem = AddItem(subject, Document.DcNS + "subject");
            string refinesId = "#" + creatorItem.GetAttribute("id");

            if (!string.IsNullOrWhiteSpace(authority))
            {
                Item roleItem = AddItem(authority);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "authority");
            }

            if (!string.IsNullOrWhiteSpace(term))
            {
                Item roleItem = AddItem(term);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "term");
            }
        }

        internal void AddDescription(string description)
            => AddItem(description, Document.DcNS + "description");


        internal void AddType(string type)
            => AddItem(type, Document.DcNS + "type");

        internal void AddFormat(string format)
            => AddItem(format, Document.DcNS + "format");

        internal void AddLanguage(string language)
            => AddItem(language, Document.DcNS + "language");

        internal void AddRelation(string relation)
            => AddItem(relation, Document.DcNS + "relation");

        internal void AddRights(string rights)
            => AddItem(rights, Document.DcNS + "rights");

        internal void AddTitle(string title)
            => AddItem(title, Document.DcNS + "title");

        internal void AddPublisher(string publisher)
            => AddItem(publisher, Document.DcNS + "publisher");

        internal void AddBookIdentifier(string id, string uuid, string scheme)
        {
            Item creatorItem = AddItem(uuid, Document.DcNS + "identifier");
            string refinesId = "#";
            if (!string.IsNullOrWhiteSpace(id))
            {
                creatorItem.SetAttribute("id", id);
                refinesId += id;
            } else
            {
                refinesId += creatorItem.GetAttribute("id");
            }

            if (!string.IsNullOrWhiteSpace(scheme))
            {
                Item roleItem = AddItem(scheme);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "identifier-type");
            }
		}

		internal void AddBookIdentifier(string id, string uuid)
			=> this.AddBookIdentifier(id, uuid, string.Empty);
        internal void AddBookIdentifier(string uuid)
            => AddBookIdentifier(string.Empty, uuid, string.Empty);

        internal void AddModifiedDateTime(DateTime dt)
        {
            Item modItem = AddItem(dt.ToString(iso8601_1FormatString));
            modItem.SetAttribute("property", "dcterms:modified");
        }
        internal void AddModifiedDateTime()
            => AddModifiedDateTime(DateTime.UtcNow);

        internal void AddPublicationDateTime(DateTime dt)
        {
            Item modItem = AddItem(dt.ToString(iso8601_1FormatString), Document.DcNS + "date");
        }

        internal void AddSeriesInfo(string seriesName, string positionInSeries = null, string seriesType = null, string seriesTypeScheme = null)
        {
            Item collectionItem = AddItem(seriesName);
            string refinesId = "#" + collectionItem.GetAttribute("id");
            collectionItem.SetAttribute("property", "belongs-to-collection");

            if (!string.IsNullOrWhiteSpace(positionInSeries))
            {
                Item posItem = AddItem(positionInSeries);
                posItem.SetAttribute("refines", refinesId);
                posItem.SetAttribute("property", "group-position");
            }

            if (!string.IsNullOrWhiteSpace(seriesType))
            {
                Item stItem = AddItem(seriesType);
                stItem.SetAttribute("refines", refinesId);
                stItem.SetAttribute("property", "collection-type");
                if (!string.IsNullOrWhiteSpace(seriesTypeScheme))
                {
                    stItem.SetAttribute("scheme", seriesTypeScheme);
                }
            }
        }
    }
}