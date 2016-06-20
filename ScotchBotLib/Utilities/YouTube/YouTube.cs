using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using ScotchBotLib.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScotchBotLib.Utilities {

	public class YouTube {

		public YouTubeResult Search(string searchTerm) {
			return Run(searchTerm).Result;
		}

		private async Task<YouTubeResult> Run(string searchTerm) {
			var youtubeService = new YouTubeService(new BaseClientService.Initializer() {
				ApiKey = ConfigurationManager.AppSettings["YouTubeApiKey"],
				ApplicationName = this.GetType().ToString()
			});

			var searchListRequest = youtubeService.Search.List("snippet");
			searchListRequest.Q = searchTerm; // Replace with your search term.
			searchListRequest.MaxResults = 50;

			// Call the search.list method to retrieve results matching the specified query term.
			var searchListResponse = await searchListRequest.ExecuteAsync();

			List<string> videos = new List<string>();
			List<string> channels = new List<string>();
			List<string> playlists = new List<string>();

			// Add each result to the appropriate list, and then display the lists of
			// matching videos, channels, and playlists.
			YouTubeResult videoRes = searchListResponse.Items
				.Where(x => x.Id.Kind == "youtube#video")
				.Select(x => new YouTubeResult {
					Title = x.Snippet.Title,
					VideoId = x.Id.VideoId
				}).FirstOrDefault();

			return videoRes;
		}
	}
}
