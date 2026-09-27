#region Related components
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            private readonly string content;
            private readonly IDictionary<XName, string> attributes = new Dictionary<XName, string>();

            internal Item(string tagContent, XName tagName)
            {
                this.content = tagContent;
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
                element.SetValue(content);

                foreach (var attr in attributes)
                {

                    element.SetAttributeValue(attr.Key, attr.Value);
                }
                return element;
            }
        }

        private static readonly XName opfMeta = Document.OpfNS + "meta";
        internal readonly Dictionary<string, int> _ids = new Dictionary<string, int>();
		readonly List<Item> _items = new List<Item>();

        internal string GetNextID(string kind)
        {
            string id;
            if (this._ids.Keys.Contains(kind))
            {
                this._ids[kind] += 1;
                id = kind + this._ids[kind].ToString();
            }
            else
            {
                id = kind + "1";
                this._ids[kind] = 1;
            }
            return id;
        }

        internal Item AddItem(string content)
            => AddItem(content, opfMeta);

        internal Item AddItem(string content, XName tagName)
        {
            var item = new Item(content, tagName);
            item.SetAttribute("id", GetNextID(tagName.LocalName));
            this._items.Add(item);
            return item;
        }

		internal XElement ToElement()
		{
			XNamespace dc = "http://purl.org/dc/elements/1.1/";
			XNamespace opf = "http://www.idpf.org/2007/opf";
			var element = new XElement(Document.OpfNS + "metadata", new XAttribute(XNamespace.Xmlns + "dc", dc), new XAttribute(XNamespace.Xmlns + "opf", opf));
			this._items.ForEach(item => element.Add(item.ToElement()));
			return element;
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
    }
}