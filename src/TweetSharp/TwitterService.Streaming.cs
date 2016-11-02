using System;
using Hammock;
using Hammock.Streaming;
using Hammock.Web;

namespace TweetSharp
{
	partial class TwitterService
	{
		private readonly RestClient _userStreamsClient;
		private readonly RestClient _publicStreamsClient;

		/// <summary>
		/// Cancels pending streaming actions from this service.
		/// </summary>
		public virtual void CancelStreaming()
		{
			if (_userStreamsClient != null)
			{
				_userStreamsClient.CancelStreaming();
			}
			if (_publicStreamsClient != null)
			{
				_publicStreamsClient.CancelStreaming();
			}
		}

		/// <summary>
		/// Accesses an asynchronous Twitter filter stream indefinitely, until terminated.
		/// </summary>
		/// <seealso href="http://dev.twitter.com/pages/streaming_api_methods#statuses-filter" />
		/// <param name="action"></param>
		/// <returns></returns>
#if !WINDOWS_PHONE
		public virtual IAsyncResult StreamFilter(Action<TwitterStreamArtifact, TwitterResponse> action, FilterStreamOptions options)
#else
        public virtual void StreamFilter(Action<TwitterStreamArtifact, TwitterResponse> action, FilterStreamOptions options)
#endif
		{
			var streamOptions = new StreamOptions { ResultsPerCallback = 1 };
#if !WINDOWS_PHONE
			return
#endif
						WithHammockPublicStreaming(streamOptions, options, action, "statuses/filter.json");
		}

		/// <summary>
		/// Accesses an asynchronous Twitter user stream indefinitely, until terminated.
		/// </summary>
		/// <seealso href="http://dev.twitter.com/pages/user_streams" />
		/// <param name="action"></param>
		/// <returns></returns>
#if !WINDOWS_PHONE
		public virtual IAsyncResult StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action)
#else
        public virtual void StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action)
#endif
		{
#if !WINDOWS_PHONE
			return StreamUser(action, null);
#else
			StreamUser(action, null);
#endif
		}

		/// <summary>
		/// Accesses an asynchronous Twitter user stream indefinitely, until terminated.
		/// </summary>
		/// <seealso href="http://dev.twitter.com/pages/user_streams" />
		/// <param name="action"></param>
		/// <returns></returns>
#if !WINDOWS_PHONE
		public virtual IAsyncResult StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action, FilterStreamOptions options)
#else
        public virtual void StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action, FilterStreamOptions options)
#endif
		{
			var streamOptions = new StreamOptions { ResultsPerCallback = 1 };

#if !WINDOWS_PHONE
			return
#endif
						WithHammockUserStreaming(streamOptions, options, action, "user.json");
		}

#if !WINDOWS_PHONE
		private IAsyncResult WithHammockUserStreaming<T>(StreamOptions streamOptions, FilterStreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#else
        private void WithHammockUserStreaming<T>(StreamOptions streamOptions, FilterStreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#endif
		{
			var request = PrepareHammockQuery(path);
			if (options != null)
				AddStreamFilterOptionsToRequest(options, request);

#if !WINDOWS_PHONE
			return
#endif
						WithHammockStreamingImpl(_userStreamsClient, request, streamOptions, action);
		}

#if !WINDOWS_PHONE
		private IAsyncResult WithHammockPublicStreaming<T>(StreamOptions streamOptions, FilterStreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#else
        private void WithHammockPublicStreaming<T>(StreamOptions streamOptions, FilterStreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#endif
		{
			var request = PrepareHammockQuery(path);
			AddStreamFilterOptionsToRequest(options, request);

#if !WINDOWS_PHONE
			return
#endif
						WithHammockStreamingImpl(_publicStreamsClient, request, streamOptions, action);
		}

		private void AddStreamFilterOptionsToRequest(FilterStreamOptions options, RestRequest request)
		{
			if (options.Delimited)
				request.AddParameter("delimited", "length");

			request.AddParameter("stall_warnings", options.StallWarnings.ToString().ToLower());
			request.AddParameter("filter_level", options.FilterLevel.ToString().ToLower());

			if (!String.IsNullOrEmpty(options.Language))
				request.AddParameter("language", options.Language);

			if (options.Follow != null && options.Follow.Length > 0)
				request.AddParameter("follow", ToCommaSeparatedString<long>(options.Follow));

			if (options.Track != null && options.Track.Length > 0)
				request.AddParameter("track", ToCommaSeparatedString<string>(options.Track));

			if (options.Locations != null && options.Locations.Length > 0)
				request.AddParameter("locations", ToCommaSeparatedString(options.Locations));

			request.AddParameter("with", options.With.ToString().ToLower());
			request.AddParameter("replies", options.Replies.ToString().ToLower());
			request.AddParameter("stringify_friend_id", options.StringifyFriendIds.ToString().ToLower());
		}

		private string ToCommaSeparatedString<T>(T[] values)
		{
			var sb = new System.Text.StringBuilder();

			foreach (var value in values)
			{
				if (sb.Length > 0)
					sb.Append(",");

				sb.Append(value);
			}

			return sb.ToString();
		}

		private string ToCommaSeparatedString(TwitterGeoLocation.GeoCoordinates[] values)
		{
			var sb = new System.Text.StringBuilder();

			foreach (var value in values)
			{
				if (sb.Length > 0)
					sb.Append(",");

				sb.Append(value.Longitude);
				sb.Append(",");
				sb.Append(value.Latitude);
			}

			return sb.ToString();
		}

#if !WINDOWS_PHONE
		private IAsyncResult WithHammockStreamingImpl<T>(RestClient client, RestRequest request, StreamOptions options, Action<T, TwitterResponse> action)
#else
        private void WithHammockStreamingImpl<T>(RestClient client, RestRequest request, StreamOptions options, Action<T, TwitterResponse> action)
#endif
		{
			request.StreamOptions = options;
			request.Method = WebMethod.Get;
#if SILVERLIGHT
            request.AddHeader("X-User-Agent", client.UserAgent); 
#endif

			var deserialiser = this.Serializer as JsonSerializer;
			if (deserialiser == null)
				deserialiser = new JsonSerializer();

#if !WINDOWS_PHONE
			return
#endif

			client.BeginRequest(request, new RestCallback<T>((req, resp, state) =>
			{
				Exception exception;
				var entity = TryAsyncResponse(() =>
											{
#if !SILVERLIGHT
													SetResponse(resp);
#endif

													return (T)deserialiser.DeserializeContent<T>(resp.Content);
									},
											out exception);
				action(entity, new TwitterResponse(resp, exception));
			}));
		}
	}
}
