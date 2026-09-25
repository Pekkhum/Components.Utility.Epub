#region Related components
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
#endregion

namespace net.vieapps.Components.Utility.Epub
{
    class Manifest
    {
        XElement _element;

		internal Manifest() => this._element = new XElement(Document.OpfNS + "manifest");

        internal void AddItem(string id, string href, string type)
        {
            AddItem(id, href, type, (string)null);
        }

        internal void AddItem(string id, string href, string type, string[] properties)
        {
            string props = null;

            if (properties != null && properties.Length > 0)
            {
                props = string.Join(" ", properties);
            }
            AddItem(id, href, type, props);
        }

        internal void AddItem(string id, string href, string type, string properties)
        {
            var item = new XElement(Document.OpfNS + "item");
            item.SetAttributeValue("id", id);
            item.SetAttributeValue("href", href);
            item.SetAttributeValue("media-type", type);
            if (properties != null)
            {
                item.SetAttributeValue("properties", properties);
            }
            this._element.Add(item);
        }


        internal XElement ToElement() => this._element;
	}
}
