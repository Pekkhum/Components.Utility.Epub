#region Related components
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using static net.vieapps.Components.Utility.Epub.CreatorData;
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
		internal readonly List<Item> _items = new List<Item>();
        internal string primaryBookIdentifierId;
        internal bool hasModifiedDateTime = false;

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
            string certifierReportMimeType = "application/html")
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

        internal void AddCreatorData(CreatorData cDat)
        {
            if(string.IsNullOrWhiteSpace(cDat.Name))
            {
                new ArgumentException(cDat.Kind.ToString() + " cannot be added to metadata without a name.");
            }

            Item creatorItem = AddItem(cDat.Name, Document.DcNS + (cDat.Kind == CreatorData.CreatorKind.Creator ? "creator" : "contributor"));
            if(!string.IsNullOrWhiteSpace(cDat.Id))
            {
                creatorItem.SetAttribute("id", cDat.Id);
            }
            string refinesId = "#" + creatorItem.GetAttribute("id");


            if (!string.IsNullOrWhiteSpace(cDat.Lang))
            {
                creatorItem.SetAttribute(XNamespace.Xml + "lang", cDat.Lang);
            }

            if (!string.IsNullOrWhiteSpace(cDat.Role))
            {
                Item roleItem = AddItem(cDat.Role);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "role");
                roleItem.SetAttribute("scheme",
                    (string.IsNullOrWhiteSpace(cDat.RoleScheme) ? "marc:relators" : cDat.RoleScheme));
            }

            if (!string.IsNullOrWhiteSpace(cDat.HomepageUri))
            {
                Item homepageUriItem = AddItem(null, Document.OpfNS + "link");
                homepageUriItem.SetAttribute("rel", "foaf:homepage");
                homepageUriItem.SetAttribute("refines", refinesId);
                homepageUriItem.SetAttribute("href", cDat.HomepageUri);
                homepageUriItem.SetAttribute("media-type",
                    (string.IsNullOrWhiteSpace(cDat.HomepageMimeType) ? "application/html" : cDat.HomepageMimeType));
            }

            if (!string.IsNullOrWhiteSpace(cDat.FileAs))
            {
                Item fileAsItem = AddItem(cDat.FileAs);
                fileAsItem.SetAttribute("refines", refinesId);
                fileAsItem.SetAttribute("property", "file-as");
            }

            List<NameLangPair> altScr = cDat.GetAlternateScripts();
            foreach (NameLangPair nameLangPair in altScr)
            {
                if(!string.IsNullOrWhiteSpace(nameLangPair.Name)
                    && !string.IsNullOrWhiteSpace(nameLangPair.Lang))
                {
                    Item altScrItem = AddItem(nameLangPair.Name);
                    altScrItem.SetAttribute("refines", refinesId);
                    altScrItem.SetAttribute("property", "alternate-script");
                    altScrItem.SetAttribute(XNamespace.Xml + "lang", nameLangPair.Lang);
                }
            }
        }

        internal void AddCreatorContributor(CreatorData.CreatorKind creatorKind, string contributorName, string role, string homepageUri = null, string homepageMimeType = "application/html")
        {
            CreatorData cDat = new CreatorData(creatorKind, contributorName, role)
            {
                HomepageUri = homepageUri,
                HomepageMimeType = homepageMimeType
            };
            AddCreatorData(cDat);
        }

        internal void AddCreator(string creatorName, string role, string homepageUri = null, string homepageMimeType = "application/html")
            => AddCreatorContributor(CreatorData.CreatorKind.Creator, creatorName, role, homepageUri, homepageMimeType);

		internal void AddCreator(string name, string homepageUri = null, string homepageMimeType = "application/html")
			=> AddCreator(name, null, homepageUri, homepageMimeType);


        internal void AddAuthor(string name, string homepageUri = null, string homepageMimeType = "application/html") 
			=> this.AddCreator(name, "aut", homepageUri, homepageMimeType);

        internal void AddTranslator(string name, string homepageUri = null, string homepageMimeType = "application/html")
            => this.AddCreator(name, "trl", homepageUri, homepageMimeType);

        internal void AddArtist(string name, string homepageUri = null, string homepageMimeType = "application/html")
            => this.AddCreator(name, "art", homepageUri, homepageMimeType);

        internal void AddContributor(string contributorName, string role, string homepageUri = null, string homepageMimeType = "application/html")
            => AddCreatorContributor(CreatorData.CreatorKind.Contributor, contributorName, role, homepageUri, homepageMimeType);

        internal void AddContributor(string name)
			=> AddContributor(name, null);

        internal void AddSubject(string subject, string authority = null, string term = null)
        {
            Item creatorItem = AddItem(subject, Document.DcNS + "subject");
            string refinesId = "#" + creatorItem.GetAttribute("id");

            if (!string.IsNullOrWhiteSpace(authority) && !string.IsNullOrWhiteSpace(term))
            {
                Item authorityItem = AddItem(authority);
                authorityItem.SetAttribute("refines", refinesId);
                authorityItem.SetAttribute("property", "authority");
                Item termItem = AddItem(term);
                termItem.SetAttribute("refines", refinesId);
                termItem.SetAttribute("property", "term");
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

        internal void AddPublisher(string publisher, string homepageUri = null, string homepageMimeType = "application/html")
        {
            Item pubItem = AddItem(publisher, Document.DcNS + "publisher");
            string refinesId = "#" + pubItem.GetAttribute("id");

            if (!string.IsNullOrWhiteSpace(homepageUri))
            {
                Item homepageUriItem = AddItem(null, Document.OpfNS + "link");
                homepageUriItem.SetAttribute("rel", "foaf:homepage");
                homepageUriItem.SetAttribute("refines", refinesId);
                homepageUriItem.SetAttribute("href", homepageUri);
                homepageUriItem.SetAttribute("media-type", homepageMimeType);
            }
        }

        internal void AddBookIdentifier(string id, string uuid, string type, string scheme, bool isPrimaryId = false)
        {
            if (isPrimaryId && !string.IsNullOrWhiteSpace(primaryBookIdentifierId))
            {
                throw new ArgumentException("Primary Book Identifier has already been set!");
            }

            Item bookIdItem = AddItem(uuid, Document.DcNS + "identifier");
            string bookIdId;
            if (string.IsNullOrWhiteSpace(id))
            {
                bookIdId = bookIdItem.GetAttribute("id");
            } else
            {
                bookIdItem.SetAttribute("id", id);
                bookIdId = id;
            }
            string refinesId = "#" + bookIdId;
            if (isPrimaryId)
            {
                primaryBookIdentifierId = bookIdId;
            }

            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(scheme))
            {
                Item roleItem = AddItem(type);
                roleItem.SetAttribute("refines", refinesId);
                roleItem.SetAttribute("property", "identifier-type");
                roleItem.SetAttribute("scheme", "scheme");
            }
		}

		internal void AddBookIdentifier(string id, string uuid, bool isPrimaryId = false)
			=> this.AddBookIdentifier(id, uuid, string.Empty, string.Empty, isPrimaryId);
        internal void AddBookIdentifier(string uuid, bool isPrimaryId = false)
            => AddBookIdentifier(string.Empty, uuid, string.Empty, string.Empty, isPrimaryId);
        internal void AddBookIdentifier(string uuid, string type, string scheme, bool isPrimaryId = false)
            => AddBookIdentifier(string.Empty, uuid, type, scheme, isPrimaryId);

        internal void AddModifiedDateTime(DateTime dt)
        {
            if (hasModifiedDateTime)
            {
                throw new ArgumentException("Modified Date/Time has already been set!");
            }
            Item modItem = AddItem(dt.ToString(iso8601_1FormatString));
            modItem.SetAttribute("property", "dcterms:modified");
            hasModifiedDateTime = true;
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