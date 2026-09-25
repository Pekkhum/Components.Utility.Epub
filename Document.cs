#region Related components
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
#endregion

#if !SIGN
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VIEApps.Components.XUnitTests")]
#endif

namespace net.vieapps.Components.Utility.Epub
{
	/// <summary>
	/// Represents an .EPUB document
	/// </summary>
	public class Document
	{
		internal readonly static XNamespace OpfNS = "http://www.idpf.org/2007/opf";
		internal readonly static XNamespace DcNS = "http://purl.org/dc/elements/1.1/";

		readonly Metadata _metadata;
		readonly Manifest _manifest;
		readonly Spine _spine;
		readonly Guide _guide;
		readonly NCX _ncx;
		readonly Container _container;
		readonly Dictionary<string, int> _ids;

		// several variables is just for convenience
		string _tempDirectory;

		string _opfDirectory;
		string _metaDirectory;

		/// <summary>
		/// Creates new instance of .EPUB document
		/// </summary>
		public Document()
		{
			this._metadata = new Metadata();
			this._manifest = new Manifest();
			this._spine = new Spine();
			this._guide = new Guide();
			this._ncx = new NCX();
			this._container = new Container();
			this._ids = new Dictionary<string, int>();

			// setup mandatory TOC file
			this._manifest.AddItem("ncx", "toc.ncx", "application/x-dtbncx+xml");
			this._spine.SetToc("ncx");
			this._container.AddRootFile("OPF/content.opf", "application/oebps-package+xml");

			var uuid = $"urn:uuid:{Guid.NewGuid()}";
			this._ncx.SetUid(uuid);
			this._metadata.AddBookIdentifier("BookId", uuid);
		}

		/// <summary>
		/// Gets the temporary directory
		/// </summary>
		/// <returns></returns>
		public string GetTempDirectory()
		{
			if (string.IsNullOrWhiteSpace(this._tempDirectory))
			{
				this._tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(this._tempDirectory);
			}
			return this._tempDirectory;
		}

		string GetOpfDirectory()
		{
			if (string.IsNullOrWhiteSpace(this._opfDirectory))
			{
				this._opfDirectory = Path.Combine(this.GetTempDirectory(), "OPF");
				Directory.CreateDirectory(this._opfDirectory);
			}
			return this._opfDirectory;
		}

		string GetMetaDirectory()
		{
			if (string.IsNullOrWhiteSpace(this._metaDirectory))
			{
				this._metaDirectory = Path.Combine(this.GetTempDirectory(), "META-INF");
				Directory.CreateDirectory(this._metaDirectory);
			}

			return this._metaDirectory;
		}

		string GetNextID(string kind)
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

		/// <summary>
		/// Add author of the document
		/// </summary>
		/// <param name="author">Human-readable full name</param>
		public void AddAuthor(string author)
		{
			this._metadata.AddAuthor(author);
			this._ncx.AddAuthor(author);
		}

		/// <summary>
		/// Add title to epub document
		/// </summary>
		/// <param name="title">document's title</param>
		public void AddTitle(string title)
		{
			this._metadata.AddTitle(title);
			this._ncx.AddTitle(title);
		}

		/// <summary>
		/// Add publisher to epub document
		/// </summary>
		/// <param name="publisher">document's publisher</param>
		public void AddPublisher(string publisher)
			=> this._metadata.AddPublisher(publisher);

		/// <summary>
		/// Add document translator
		/// </summary>
		/// <param name="name">Human-readable full name</param>
		public void AddTranslator(string name)
			=> this._metadata.AddTranslator(name);

		/// <summary>
		/// Add document contributor
		/// </summary>
		/// <param name="name">Human-readable full name</param>
		public void AddContributor(string name)
			=> this._metadata.AddContributor(name);

		/// <summary>
		/// Add document subject: phrase or list of keywords
		/// </summary>
		/// <param name="subject">Document's subject</param>
		public void AddSubject(string subject)
			=> this._metadata.AddSubject(subject);

		/// <summary>
		/// Add description of document's content
		/// </summary>
		/// <param name="description">Document description</param>
		public void AddDescription(string description)
			=> this._metadata.AddDescription(description);

		/// <summary>
		/// Add terms describing general categories, functions, genres, or aggregation levels for content.
		/// The advised best practice is to select a value from a controlled vocabulary.
		/// </summary>
		/// <param name="type">document type</param>
		public void AddType(string type)
			=> this._metadata.AddType(type);

		/// <summary>
		/// Add media type or dimensions of the resource. Best practice is to use a value from a controlled vocabulary (e.g. MIME media types).
		/// </summary>
		/// <param name="format">document format</param>
		public void AddFormat(string format)
			=> this._metadata.AddFormat(format);

		/// <summary>
		/// Add a language of the intellectual content of the Publication.
		/// </summary>
		/// <param name="language">RFC3066-complient two-letter language code e.g. "en", "es", "it"</param>
		public void AddLanguage(string language)
			=> this._metadata.AddLanguage(language);

		/// <summary>
		/// Add an identifier of an auxiliary resource and its relationship to the publication.
		/// </summary>
		/// <param name="relation">document relation</param>
		public void AddRelation(string relation)
			=> this._metadata.AddRelation(relation);

		/// <summary>
		/// Add a statement about rights, or a reference to one.
		/// </summary>
		/// <param name="rights">A statement about rights, or a reference to one</param>
		public void AddRights(string rights)
			=> this._metadata.AddRights(rights);

		/// <summary>
		/// Add book identifier
		/// </summary>
		/// <param name="id">A string or number used to uniquely identify the resource</param>
		public void AddBookIdentifier(string id)
			=> this.AddBookIdentifier(id, string.Empty);

		/// <summary>
		/// Add book identifier
		/// </summary>
		/// <param name="id">A string or number used to uniquely identify the resource</param>
		/// <param name="scheme">System or authority that generated or assigned the id parameter, for example "ISBN" or "DOI." </param>
		public void AddBookIdentifier(string id, string scheme)
			=> this._metadata.AddBookIdentifier(GetNextID("id"), id, scheme);

		/// <summary>
		/// Add generic metadata
		/// </summary>
		/// <param name="name">meta element name</param>
		/// <param name="value">meta element value</param>
		public void AddMetaItem(string name, string value)
			=> this._metadata.AddItem(name, value);

		/// <summary>
		/// Add DC metadata
		/// </summary>
		/// <param name="name">meta element name</param>
		/// <param name="value">meta element value</param>
		public void AddMetaDCItem(string name, string value)
			=> this._metadata.AddDCItem(name, value);

        string AddEntry(string id, string path, string type, string[] properties)
        {
            this._manifest.AddItem(id, path, type, properties);
            return id;
        }
        string AddEntry(string id, string path, string type)
        {
            AddEntry(id, path, type, null);
            return id;
        }

        string AddEntry(string path, string type)
        {
            return AddEntry(path, type, (string[]) null);
        }

        string AddEntry(string path, string type, string[] properties)
        {
            return AddEntry(this.GetNextID("id"), path, type, properties);
        }

        string AddStylesheetEntry(string path)
        {
            return AddStylesheetEntry(path, "text/css");
        }

        string AddStylesheetEntry(string path, string contentType)
        {
            return AddStylesheetEntry(path, contentType, null);
        }
        string AddStylesheetEntry(string path, string[] properties)
        {
            return AddStylesheetEntry(path, "text/css", properties);
        }

        string AddStylesheetEntry(string path, string contentType, string[] properties)
        {
            return AddEntry(this.GetNextID("stylesheet"), path, contentType, properties);
        }

        string AddXhtmlEntry(string path, bool linear = true)
        {
            return AddXhtmlEntry(path, (string[]) null, linear);
        }

        string AddXhtmlEntry(string path, string[] properties, bool linear = true)
        {
            return AddXhtmlEntry(path, "application/xhtml+xml", properties, linear);
        }

        string AddXhtmlEntry(string path, string contentType, bool linear = true)
        {
            return AddXhtmlEntry(path, contentType, (string[])null, linear);
        }

        string AddXhtmlEntry(string path, string contentType, string[] properties, bool linear = true)
        {
            var id = this.GetNextID("html");
            AddEntry(id, path, contentType, properties);
            this._spine.AddItemRef(id, linear);
            return id;
        }

        string ImageMimeFromExtension(string path)
        {
            var contentType = string.Empty;
            var filepath = path.ToLower();

            if (filepath.EndsWith(".jpg") || filepath.EndsWith(".jpeg"))
                contentType = "image/jpeg";
            else if (filepath.EndsWith(".png"))
                contentType = "image/png";
            else if (filepath.EndsWith(".gif"))
                contentType = "image/gif";
            else if (filepath.EndsWith(".bmp"))
                contentType = "image/bmp";
            else if (filepath.EndsWith(".svg"))
                contentType = "image/svg+xml";

			return contentType;
        }

        string AddImageEntry(string path)
        {
            return AddImageEntry(path, ImageMimeFromExtension(path));
        }

        string AddImageEntry(string path, string[] properties)
        {
            return AddImageEntry(path, ImageMimeFromExtension(path), properties);
        }

        string AddImageEntry(string path, string contentType)
        {
            return AddImageEntry(path, contentType, null);
        }

        string AddImageEntry(string path, string contentType, string[] properties)
        {
            var id = this.GetNextID("img");
            AddEntry(id, path, contentType, properties);
            return id;
        }

        void CopyFile(string path, string epubPath)
		{
			var fullPath = Path.Combine(this.GetOpfDirectory(), epubPath);
			this.EnsureDirectoryExists(fullPath);
			File.Copy(path, fullPath);
		}

		string EnsureDirectoryExists(string path)
		{
			var directory = Path.GetDirectoryName(path);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			return path;
		}

		void WriteFile(string epubPath, byte[] content)
		{
			var fullPath = Path.Combine(this.GetOpfDirectory(), epubPath);
			this.EnsureDirectoryExists(fullPath);
			File.WriteAllBytes(fullPath, content);
		}

		void WriteFile(string epubPath, string content)
		{
			var fullPath = Path.Combine(this.GetOpfDirectory(), epubPath);
			this.EnsureDirectoryExists(fullPath);
			File.WriteAllText(fullPath, content, Encoding.UTF8);
        }

        /// <summary>
        /// Add image to document's contents
        /// </summary>
        /// <param name="path">Path to source image file</param>
        /// <param name="epubPath">Path to image file in EPUB</param>
        /// <returns>id of newly created element</returns>
        public string AddImageFile(string path, string epubPath)
        {
            this.CopyFile(path, epubPath);
            return AddImageEntry(epubPath);
        }

        /// <summary>
        /// Add image to document's contents
        /// </summary>
        /// <param name="path">Path to source image file</param>
        /// <param name="epubPath">Path to image file in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>id of newly created element</returns>
        public string AddImageFile(string path, string epubPath, string[] properties)
        {
            this.CopyFile(path, epubPath);
            return AddImageEntry(epubPath, properties);
        }

        /// <summary>
        /// Add image to document's contents
        /// </summary>
        /// <param name="path">Path to source image file</param>
        /// <param name="epubPath">Path to image file in EPUB</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <returns>id of newly created element</returns>
        public string AddImageFile(string path, string epubPath, string contentType)
        {
            return AddImageFile(epubPath, contentType, (string[])null);
        }

        /// <summary>
        /// Add image to document's contents
        /// </summary>
        /// <param name="path">Path to source image file</param>
        /// <param name="epubPath">Path to image file in EPUB</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>id of newly created element</returns>
        public string AddImageFile(string path, string epubPath, string contentType, string[] properties)
        {
            this.CopyFile(path, epubPath);
            return AddImageEntry(epubPath, contentType, properties);
        }

        /// <summary>
        /// Add CSS file to document's contents
        /// </summary>
        /// <param name="path">path to source CSS file</param>
        /// <param name="epubPath">path to destination file in EPUB</param>
        /// <returns>id of newly created element</returns>
        public string AddStylesheetFile(string path, string epubPath)
        {
            this.CopyFile(path, epubPath);
            return this.AddStylesheetEntry(epubPath);
        }

        /// <summary>
        /// Add CSS file to document's contents
        /// </summary>
        /// <param name="path">path to source CSS file</param>
        /// <param name="epubPath">path to destination file in EPUB</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <returns>id of newly created element</returns>
        public string AddStylesheetFile(string path, string epubPath, string contentType)
        {
            this.CopyFile(path, epubPath);
            return this.AddStylesheetEntry(epubPath, contentType);
        }

        /// <summary>
        /// Add CSS file to document's contents
        /// </summary>
        /// <param name="path">path to source CSS file</param>
        /// <param name="epubPath">path to destination file in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>id of newly created element</returns>
        public string AddStylesheetFile(string path, string epubPath, string[] properties)
        {
            this.CopyFile(path, epubPath);
            return this.AddStylesheetEntry(epubPath, properties);
        }

        /// <summary>
        /// Add CSS file to document's contents
        /// </summary>
        /// <param name="path">path to source CSS file</param>
        /// <param name="epubPath">path to destination file in EPUB</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>id of newly created element</returns>
        public string AddStylesheetFile(string path, string epubPath, string contentType, string[] properties)
        {
            this.CopyFile(path, epubPath);
            return this.AddStylesheetEntry(epubPath, contentType, properties);
        }

        /// <summary>
        /// Add primary or auxiliary (like notes) XHTML file to document's content
        /// </summary>
        /// <param name="path">path to source file</param>
        /// <param name="epubPath">path in epub</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <param name="primary">true for primary document, false for auxiliary</param>
        /// <returns>id of newly created element</returns>
        public string AddXhtmlFile(string path, string epubPath, string contentType, bool primary = true)
        {
            this.CopyFile(path, epubPath);
            return this.AddXhtmlEntry(epubPath, contentType, primary);
        }

        /// <summary>
        /// Add primary or auxiliary (like notes) XHTML file to document's content
        /// </summary>
        /// <param name="path">path to source file</param>
        /// <param name="epubPath">path in epub</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="primary">true for primary document, false for auxiliary</param>
        /// <returns>id of newly created element</returns>
        public string AddXhtmlFile(string path, string epubPath, string[] properties, bool primary = true)
        {
            this.CopyFile(path, epubPath);
            return this.AddXhtmlEntry(epubPath, properties, primary);
        }

        /// <summary>
        /// Add primary or auxiliary (like notes) XHTML file to document's content
        /// </summary>
        /// <param name="path">path to source file</param>
        /// <param name="epubPath">path in epub</param>
        /// <param name="contentType">The MIME content type of the image file</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="primary">true for primary document, false for auxiliary</param>
        /// <returns>id of newly created element</returns>
        public string AddXhtmlFile(string path, string epubPath, string contentType, string[] properties, bool primary = true)
        {
            this.CopyFile(path, epubPath);
            return this.AddXhtmlEntry(epubPath, contentType, properties, primary);
        }

        /// <summary>
        /// Add primary or auxiliary (like notes) XHTML file to document's content
        /// </summary>
        /// <param name="path">path to source file</param>
        /// <param name="epubPath">path in epub</param>
        /// <param name="primary">true for primary document, false for auxiliary</param>
        /// <returns>id of newly created element</returns>
        public string AddXhtmlFile(string path, string epubPath, bool primary = true)
        {
            this.CopyFile(path, epubPath);
            return this.AddXhtmlEntry(epubPath, primary);
        }

        /// <summary>
        /// Add generic file to document's contents
        /// </summary>
        /// <param name="path">source file path</param>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <returns>id of newly added file</returns>
        public string AddFile(string path, string epubPath, string mediaType)
        {
            this.CopyFile(path, epubPath);
            return this.AddEntry(epubPath, mediaType);
        }

        /// <summary>
        /// Add generic file to document's contents
        /// </summary>
        /// <param name="path">source file path</param>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>id of newly added file</returns>
        public string AddFile(string path, string epubPath, string mediaType, string[] properties)
        {
            this.CopyFile(path, epubPath);
            return this.AddEntry(epubPath, mediaType, properties);
        }

        // Data versions of AddNNN functions
        /// <summary>
        /// Add image file to document with specified content. Image type
        /// is detected by filename's extension
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddImageData(string epubPath, byte[] content)
        {
            this.WriteFile(epubPath, content);
            return this.AddImageEntry(epubPath);
        }

        // Data versions of AddNNN functions
        /// <summary>
        /// Add image file to document with specified content. Image type
        /// is detected by filename's extension
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddImageData(string epubPath, string mediaType, byte[] content)
        {
            this.WriteFile(epubPath, content);
            return this.AddImageEntry(epubPath, mediaType);
        }

        // Data versions of AddNNN functions
        /// <summary>
        /// Add image file to document with specified content. Image type
        /// is detected by filename's extension
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddImageData(string epubPath, string[] properties, byte[] content)
        {
            this.WriteFile(epubPath, content);
            return this.AddImageEntry(epubPath, properties);
        }

        // Data versions of AddNNN functions
        /// <summary>
        /// Add image file to document with specified content. Image type
        /// is detected by filename's extension
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddImageData(string epubPath, string mediaType, string[] properties, byte[] content)
        {
            this.WriteFile(epubPath, content);
            return this.AddImageEntry(epubPath, mediaType, properties);
        }

        /// <summary>
        /// Add CSS file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddStylesheetData(string epubPath, string content)
        {
            this.WriteFile(epubPath, content);
            return this.AddStylesheetEntry(epubPath);
        }

        /// <summary>
        /// Add CSS file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddStylesheetData(string epubPath, string mediaType, string content)
        {
            this.WriteFile(epubPath, content);
            return this.AddStylesheetEntry(epubPath, mediaType);
        }

        /// <summary>
        /// Add CSS file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddStylesheetData(string epubPath, string[] properties, string content)
        {
            this.WriteFile(epubPath, content);
            return this.AddStylesheetEntry(epubPath, properties);
        }

        /// <summary>
        /// Add CSS file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <returns>id of newly added file</returns>
        public string AddStylesheetData(string epubPath, string mediaType, string[] properties, string content)
        {
            this.WriteFile(epubPath, content);
            return this.AddStylesheetEntry(epubPath, mediaType, properties);
        }

        /// <summary>
        /// Add primary or auxiliary XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file content</param>
        /// <param name="primary">true if file is primary, false if auxiliary</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string content, bool primary)
        {
            this.WriteFile(epubPath, content);
            return this.AddXhtmlEntry(epubPath, primary);
        }

        /// <summary>
        /// Add primary  XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file contents</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string content)
            => this.AddXhtmlData(epubPath, content, true);

        /// <summary>
        /// Add primary or auxiliary XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="content">file content</param>
        /// <param name="primary">true if file is primary, false if auxiliary</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string mediaType, string content, bool primary)
        {
            this.WriteFile(epubPath, content);
            return this.AddXhtmlEntry(epubPath, mediaType, primary);
        }

        /// <summary>
        /// Add primary  XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="content">file contents</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string mediaType, string content)
            => this.AddXhtmlData(epubPath, mediaType, content, true);

        /// <summary>
        /// Add primary or auxiliary XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <param name="primary">true if file is primary, false if auxiliary</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string[] properties, string content, bool primary)
        {
            this.WriteFile(epubPath, content);
            return this.AddXhtmlEntry(epubPath, properties, primary);
        }

        /// <summary>
        /// Add primary  XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file contents</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string[] properties, string content)
            => this.AddXhtmlData(epubPath, properties, content, true);

        /// <summary>
        /// Add primary or auxiliary XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file content</param>
        /// <param name="primary">true if file is primary, false if auxiliary</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string mediaType, string[] properties, string content, bool primary)
        {
            this.WriteFile(epubPath, content);
            return this.AddXhtmlEntry(epubPath, mediaType, properties, primary);
        }

        /// <summary>
        /// Add primary  XHTML file to document with specified content.
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <param name="content">file contents</param>
        /// <returns>identifier of added file</returns>
        public string AddXhtmlData(string epubPath, string mediaType, string[] properties, string content)
            => this.AddXhtmlData(epubPath, mediaType, properties, content, true);

        /// <summary>
        /// Add generic file to document with specified content
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file content</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <returns>identifier of added file</returns>
        public string AddData(string epubPath, byte[] content, string mediaType)
        {
            this.WriteFile(epubPath, content);
            return this.AddEntry(epubPath, mediaType);
        }

        /// <summary>
        /// Add generic file to document with specified content
        /// </summary>
        /// <param name="epubPath">path in EPUB</param>
        /// <param name="content">file content</param>
        /// <param name="mediaType">MIME media-type, e.g. "application/octet-stream"</param>
        /// <param name="properties">EPUB Manifest properties for this image</param>
        /// <returns>identifier of added file</returns>
        public string AddData(string epubPath, byte[] content, string mediaType, string[] properties)
        {
            this.WriteFile(epubPath, content);
            return this.AddEntry(epubPath, mediaType, properties);
        }

        void WriteOpf(string opfFilePath)
		{
			var packageElement = new XElement(Document.OpfNS + "package", new XAttribute("version", "2.0"), new XAttribute("unique-identifier", "BookId"));
			packageElement.Add(this._metadata.ToElement());
			packageElement.Add(this._manifest.ToElement());
			packageElement.Add(this._spine.ToElement());
			packageElement.Add(this._guide.ToElement());
			packageElement.Save(Path.Combine(this.GetOpfDirectory(), opfFilePath));
		}

		void WriteNcx(string ncxFilePath)
			=> this._ncx.ToXmlDocument().Save(Path.Combine(this.GetOpfDirectory(), ncxFilePath));

		void WriteContainer()
			=> this._container.ToElement().Save(Path.Combine(this.GetMetaDirectory(), "container.xml"));

		void WriteAppleiBooksDisplayOptions()
			=> new XElement("display_options", new XElement("platform", new XAttribute("name", "*"), new XElement("option", new XAttribute("name", "specified-fonts"), true))).Save(Path.Combine(this.GetMetaDirectory(), "com.apple.ibooks.display-options.xml"));

		/// <summary>
		/// Add navigation point to top-level Table of Contents.
		/// </summary>
		/// <param name="label">Text of TOC entry</param>
		/// <param name="content">Link to TOC entry</param>
		/// <param name="playOrder">play order counter</param>
		/// <returns>newly created NavPoint </returns>
		public NavPoint AddNavPoint(string label, string content, int playOrder)
			=> this._ncx.AddNavPoint(label, this.GetNextID("navid"), content, playOrder);

		/// <summary>
		/// Add reference to guide
		/// </summary>
		/// <param name="href">href of guide reference</param>
		/// <param name="type">type of guide reference</param>
		public void AddReference(string href, string type)
			=> this._guide.AddReference(href, type);

		/// <summary>
		/// Add reference to guide
		/// </summary>
		/// <param name="href">href of guide reference</param>
		/// <param name="type">type of guide reference</param>
		/// <param name="title">title of guide reference</param>
		public void AddReference(string href, string type, string title)
			=> this._guide.AddReference(href, type, title);

		/// <summary>
		/// Generate .EPUB document and save into a specified file path
		/// </summary>
		/// <param name="filePath">The absolute file path to save the .EPUB document</param>
		/// <param name="onSuccess">The action to callback when generated successfully</param>
		/// <param name="onFailure">The action to callback when got any error</param>
		public void Generate(string filePath, Action<string> onSuccess = null, Action<Exception> onFailure = null)
		{
			try
			{
				// write contents to temp directory
				this.WriteOpf("content.opf");
				this.WriteNcx("toc.ncx");
				this.WriteContainer();
				this.WriteAppleiBooksDisplayOptions();

				// check to delete old .EPUB file
				if (File.Exists(filePath))
					try
					{
						File.Delete(filePath);
					}
					catch { }

				// zip the temp directory as .EPUB file
				ZipFile.CreateFromDirectory(this.GetTempDirectory(), filePath, CompressionLevel.Optimal, false, Encoding.UTF8);

				// add MIME type => for working with Apple iBooks
				using (var zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Update))
				{
					var entry = zipArchive.CreateEntry("mimetype", CompressionLevel.NoCompression);
					using (var writer = new StreamWriter(entry.Open()))
					{
						writer.WriteLine("application/epub+zip");
					}
				}

				// callback
				onSuccess?.Invoke(filePath);
			}
			catch (Exception ex)
			{
				if (onFailure != null)
					onFailure(ex);
				else
					throw;
			}
			finally
			{
				// delete the temporary directory
				try
				{
					Directory.Delete(this.GetTempDirectory(), true);
				}
				catch { }
			}
		}
	}
}