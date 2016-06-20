using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Models {

	class FilePublic : SlackMessage {

		public string type { get; set; }
		public File file { get; set; }
		public string event_ts { get; set; }
	}

	public class FileShared : SlackMessage {
		public string type { get; set; }
		public File file { get; set; }
		public string event_ts { get; set; }
	}

	public class FileChange : SlackMessage {
		public string type { get; set; }
		public File file { get; set; }
		public string event_ts { get; set; }
	}

	public class FileUnshared : SlackMessage {
		public string type { get; set; }
		public File file { get; set; }
		public string event_ts { get; set; }
	}

	public class File {
		public string id { get; set; }
		public int created { get; set; }
		public int timestamp { get; set; }
		public string name { get; set; }
		public string title { get; set; }
		public string mimetype { get; set; }
		public string filetype { get; set; }
		public string pretty_type { get; set; }
		public string user { get; set; }
		public bool editable { get; set; }
		public int size { get; set; }
		public string mode { get; set; }
		public bool is_external { get; set; }
		public string external_type { get; set; }
		public bool is_public { get; set; }
		public bool public_url_shared { get; set; }
		public bool display_as_bot { get; set; }
		public string username { get; set; }
		public string url_private { get; set; }
		public string url_private_download { get; set; }
		public string thumb_64 { get; set; }
		public string thumb_80 { get; set; }
		public string thumb_360 { get; set; }
		public int thumb_360_w { get; set; }
		public int thumb_360_h { get; set; }
		public string thumb_160 { get; set; }
		public int image_exif_rotation { get; set; }
		public int original_w { get; set; }
		public int original_h { get; set; }
		public string permalink { get; set; }
		public string permalink_public { get; set; }
		public string[] channels { get; set; }
		public object[] groups { get; set; }
		public object[] ims { get; set; }
		public int comments_count { get; set; }
	}
}
