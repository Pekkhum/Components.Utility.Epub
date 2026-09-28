using System;
using System.Collections.Generic;
using System.Text;

namespace net.vieapps.Components.Utility.Epub
{
    /// <summary>
    /// A class used to represent creator or contributor information for addition to EPUB metadata.
    /// </summary>
    public class CreatorData
    {
        internal class NameLangPair
        {
            public string Name { get; set; }
            public string Lang { get; set; }   

            internal NameLangPair(string name, string lang)
            {
                Name = name;
                Lang = lang;
            }
        }

        /// <summary>
        /// If the creator is a primary contributor to the work, they are a creator.
        /// If they are a secondary contributor, they are a contributor.
        /// Per EPUB 3.4 standard.
        /// </summary>
        public enum CreatorKind {
            Creator,
            Contributor
        }

        /// <summary>
        /// Whether this represents a creator (primary contributor)
        /// or contributor (secondary contributor).
        /// </summary>
        public CreatorKind Kind { get; set; }
        /// <summary>
        /// Optional; May be used to customize the ID of the base HTML element in the resulting metadata.
        /// If not set, it will be assigned automatically, otherwise, it must be fully unique.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name of the creator.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Optional; The Role of the author. This should be drawn from the MARC Relators list,
        /// unless another scheme is specified in the RoleScheme property.
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Optional; What labeling scheme the Role field is based on. Defaults to "marc:relators".
        /// This is not used if the Role property is not set.
        /// </summary>
        public string RoleScheme { get; set; } = "marc:relators";
        /// <summary>
        /// Optional; A language code from the "xml:lang" code list.
        /// </summary>
        public string Lang { get; set; }
        /// <summary>
        /// Optional; FileAs MAY be used to associate a normalized form of the creator's name.
        /// </summary>
        public string FileAs { get; set; }
        /// <summary>
        /// Optional; The URI string for this creator's homepage.
        /// </summary>
        public string HomepageUri { get; set; }
        /// <summary>
        /// Optional; The MIME type string for this creator's homepage, defaults to "application/html".
        /// </summary>
        public string HomepageMimeType { get; set; } = "application/html";

        private readonly List<NameLangPair> _alternateScripts = new List<NameLangPair>();

        public CreatorData(CreatorKind kind, string name, string role = null) {
            Kind = kind;
            Name = name;
            Role = role;
        }

        /// <summary>
        /// The text and "xml:lang" language code for the creator's name in an alternate script.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lang"></param>
        public void AddAlternateScript(string name, string lang)
        {
            NameLangPair alternateScript = new NameLangPair(name, lang);
            _alternateScripts.Add(alternateScript);
        }

        internal List<NameLangPair> GetAlternateScripts()
        {
            return _alternateScripts;
        }
    }
}
