using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;

namespace TweetSharp.Tests.Service
{
	[TestFixture, System.Runtime.InteropServices.GuidAttribute("DD654DE5-566A-4DAB-A675-7AD6C998F4B9")]
	public partial class TwitterServiceTests
	{
		private readonly string _hero;
		private readonly string _consumerKey;
		private readonly string _consumerSecret;
		private readonly string _accessToken;
		private readonly string _accessTokenSecret;
		private readonly string _twitPicKey;
		private readonly string _twitPicUserName;

		public TwitterServiceTests()
		{
			_hero = ConfigurationManager.AppSettings["Hero"];
			_consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
			_consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
			_accessToken = ConfigurationManager.AppSettings["AccessToken"];
			_accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
			_twitPicKey = ConfigurationManager.AppSettings["TwitPicKey"];
			_twitPicUserName = ConfigurationManager.AppSettings["TwitPicUserName"];
		}

		[Test]
		public void Can_get_twitter_configuration()
		{
			var service = GetAuthenticatedService();
			var configuration = service.GetConfiguration();

			Assert.IsNotNull(configuration);
			Assert.Greater(configuration.CharactersReservedPerMedia, 0);
			Assert.Greater(configuration.MaxMediaPerUpload, 0);
			Assert.Greater(configuration.ShortUrlLength, 0);
			Assert.Greater(configuration.ShortUrlLengthHttps, 0);
			Assert.Greater(configuration.PhotoSizeLimit, 0);
			Assert.IsNotNull(configuration.NonUserNamePaths);
			Assert.Greater(configuration.NonUserNamePaths.Count(), 0);
			Assert.IsNotNull(configuration.PhotoSizes);
			Assert.IsNotNull(configuration.PhotoSizes.Thumb);
			Assert.IsNotNull(configuration.PhotoSizes.Small);
			Assert.IsNotNull(configuration.PhotoSizes.Medium);
			Assert.IsNotNull(configuration.PhotoSizes.Large);
			Assert.Greater(configuration.PhotoSizes.Thumb.Height, 0);
			Assert.Greater(configuration.PhotoSizes.Thumb.Width, 0);
			Assert.IsNotEmpty(configuration.PhotoSizes.Thumb.Resize);
			Assert.Greater(configuration.PhotoSizes.Small.Height, 0);
			Assert.Greater(configuration.PhotoSizes.Small.Width, 0);
			Assert.IsNotEmpty(configuration.PhotoSizes.Small.Resize);
			Assert.Greater(configuration.PhotoSizes.Medium.Height, 0);
			Assert.Greater(configuration.PhotoSizes.Medium.Width, 0);
			Assert.IsNotEmpty(configuration.PhotoSizes.Medium.Resize);
			Assert.Greater(configuration.PhotoSizes.Large.Height, 0);
			Assert.Greater(configuration.PhotoSizes.Large.Width, 0);
			Assert.IsNotEmpty(configuration.PhotoSizes.Large.Resize);
		}

		[Test]
		public void Can_parse_ids_greater_than_53_bits()
		{
			const string json = "{ \"id\": 90071992547409921}";
			var status = new TwitterService().Deserialize<TwitterStatus>(json);
			Assert.IsNotNull(status);
			Assert.AreEqual(90071992547409921, status.Id);
		}

		[Test]
		public void Can_deserialize_dates()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

			Assert.IsNotNull(tweets);
			Assert.IsTrue(tweets.Any());

			foreach (var tweet in tweets)
			{
				Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Id);
				Assert.AreNotEqual(default(DateTime), tweet.CreatedDate);
			}
		}

		[Test]
		public void Can_get_mentions_and_fail_due_to_unauthorized_request()
		{
			var service = new TwitterService();
			var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions());

			Assert.AreEqual(HttpStatusCode.BadRequest, service.Response.StatusCode);
			Assert.IsNull(mentions);

			var error = service.Response.Error;
			Assert.IsNotNull(error);
			Assert.IsNotEmpty(error.Message);
			Trace.WriteLine(error.ToString());
		}

		[Test]
		public void Can_get_mentions()
		{
			var service = GetAuthenticatedService();
			var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions()).ToList();

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(mentions);
			Assert.IsTrue(mentions.Count() <= 20);

			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);

			foreach (var dm in mentions)
			{
				Console.WriteLine("{0} said '{1}'", dm.User.ScreenName, dm.Text);
				System.Diagnostics.Debug.WriteLine(String.Format("{0} said '{1}'", dm.User.ScreenName, dm.Text));
			}
		}

		[Test]
		public void Can_get_authenticated_user_profile()
		{
			var service = GetAuthenticatedService();
			var profile = service.GetUserProfile(new GetUserProfileOptions());

			Trace.WriteLine(service.Response.Response);

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(profile);
			Assert.IsNotEmpty(profile.ScreenName);
		}

		[Test]
		public void Can_get_user_profile_for()
		{
			var service = GetAuthenticatedService();
			var profile = GetHeroProfile(service);

			Trace.WriteLine(service.Response.Response);

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(profile);
			Assert.IsNotEmpty(profile.ScreenName);
		}

		private TwitterUser GetHeroProfile(TwitterService service)
		{
			return service.GetUserProfileFor(new GetUserProfileForOptions { ScreenName = _hero });
		}

		[Test]
		public void Can_get_banner_sizes_for()
		{
			var service = GetAuthenticatedService();
			var bannerSizes = service.GetProfileBannerFor(new GetProfileBannerForOptions() { ScreenName = "yortw" });

			Trace.WriteLine(service.Response.Response);

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(bannerSizes);
			Assert.IsNotNull(bannerSizes.Sizes);
			Assert.IsTrue(bannerSizes.Sizes.Any());
		}

		[Test]
		public void Can_destroy_tweet()
		{
			var service = GetAuthenticatedService();
			var status = "This tweet should self-destruct in 5 seconds. " + Guid.NewGuid().ToString();
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);

			System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

			var deletedStatus = service.DeleteTweet(new DeleteTweetOptions() { Id = tweet.Id });
			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(deletedStatus);
			Assert.AreEqual(deletedStatus.Id, tweet.Id);

			var foundStatus = service.GetTweet(new GetTweetOptions() { Id = deletedStatus.Id });
			AssertResultWas(service, HttpStatusCode.NotFound);
			Assert.IsNull(foundStatus);
		}

		[Test]
		public void Can_tweet()
		{
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_TweetWithCallback()
		{
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
			var callbackHappened = false;
			var asyncResult = service.SendTweet
			(	
				new SendTweetOptions { Status = status }, 
				(cbStatus, cbResponse) =>
				{
					callbackHappened = true;
				}
			);

			asyncResult.AsyncWaitHandle.WaitOne();

			Assert.IsTrue(callbackHappened);
			AssertResultWas(service, HttpStatusCode.OK);
		}

		[Test]
		public void IsRetweetReturnsTrue()
		{
			#region Test Setup 
			//First tweet
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });

			var retweet = service.Retweet(new RetweetOptions() { Id = tweet.Id });

			#endregion

			var updatedStatus = service.GetTweet(new GetTweetOptions() { Id = retweet.Id });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(updatedStatus);
			Assert.AreEqual(true, retweet.IsRetweeted);
			Assert.AreEqual(true, updatedStatus.IsRetweeted);
		}

		[Test]
		public void Can_Unretweet()
		{
			#region Test Setup 
			//First tweet
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });

			var retweet = service.Retweet(new RetweetOptions() { Id = tweet.Id });

			#endregion

			var originalTweet = service.Unretweet(new UnretweetOptions() { Id = retweet.Id });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);

			//Check we can't find the retweet anymore
			var notFoundTweet = service.GetTweet(new GetTweetOptions() { Id = retweet.Id });
			Assert.IsNull(notFoundTweet);
			AssertResultWas(service, HttpStatusCode.NotFound);
		}

		[Test]
		public void Can_tweet_with_attachment_url()
		{
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet with attachment url from TweetSharp unit tests";
			var tweet = service.SendTweet(new SendTweetOptions { Status = status, AttachmentUrl = "https://twitter.com/yortwdevtest/status/778901592632233985" });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_tweet_with_auto_reply()
		{
			var service = GetAuthenticatedService();
			var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);

			var reply = service.SendTweet(new SendTweetOptions { Status = "This is a reply with auto populated reply metadata " + DateTime.UtcNow.Ticks.ToString(), AutoPopulateReplyMetadata = true, InReplyToStatusId= tweet.Id });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(reply);
			Assert.AreNotEqual(0, reply.Id);

		}

		[Test]
		public void Can_tweet_accented_chars()
		{
			var service = GetAuthenticatedService();
			var status = "Can_tweet_with_image:Not a duplicate of following tweet " + Guid.NewGuid().ToString();
			var tweet = service.SendTweet(new SendTweetOptions { Status = status });
			status = "Can_tweet_with_image:Tweet an accented char à...." + Guid.NewGuid().ToString();
			tweet = service.SendTweet(new SendTweetOptions { Status = status });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_tweet_with_geo()
		{
			// status=123&lat=56.95&%40long=24.1&include_entities=1
			var service = GetAuthenticatedService();
			var tweet = service.SendTweet(new SendTweetOptions { Status = DateTime.UtcNow.Ticks.ToString(), Lat = 56.95, Long = 24.1 });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_tweet_emjoi_greater_than_hex_10000()
		{
			var service = GetAuthenticatedService();

			//TODO: Fix code and then finish this test.
			var message = "Testing 😀👌☺️☺☹😀😉 😀☺️ " + System.Guid.NewGuid().ToString();
			var tweet = service.SendTweet(new SendTweetOptions { Status = message });
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);

			var retrievedTweet = service.GetTweet(new GetTweetOptions() { Id = tweet.Id });
			Assert.AreNotEqual(0, retrievedTweet.Id);
		}

		[Test]
		public void Can_tweet_with_special_characters()
		{
			var service = GetAuthenticatedService();

			var message = "!@#$%^&*();:-" + DateTime.UtcNow.Ticks;
			var tweet = service.SendTweet(new SendTweetOptions { Status = message });
			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_tweet_with_location_custom_type()
		{
			var service = GetAuthenticatedService();
			var tweet = service.SendTweet(new SendTweetOptions { Status = DateTime.UtcNow.Ticks.ToString(), Lat = 45.43989910068863, Long = -75.69168090820312 });

			var uri = service.Response.RequestUri;
			var queryString = HttpUtility.ParseQueryString(uri.Query);
			var location = queryString["location"];
			Assert.AreNotEqual("TweetSharp.TwitterGeoLocation", location);

			Assert.IsNotNull(tweet);
			Assert.AreNotEqual(0, tweet.Id);
		}

		[Test]
		public void Can_tweet_and_handle_dupes()
		{
			var service = GetAuthenticatedService();

			service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet" });
			var response = service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet" });

			Assert.IsNull(response);
			Assert.IsNotNull(service.Response);
			Assert.AreNotEqual(HttpStatusCode.OK, service.Response.StatusCode);

			var error = service.Response.Error;
			Assert.IsNotNull(error);
			Assert.IsNotEmpty(error.Message);
		}

		[Test]
		public void Can_tweet_with_image()
		{
			var service = GetAuthenticatedService();
			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "daniel_8bit.png"), FileMode.Open))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var tweet = service.SendTweetWithMedia(new SendTweetWithMediaOptions
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
				{
					Status = "Can_tweet_with_image:Tweet",
					Images = new Dictionary<string, Stream> { { "test", stream } }
				});
#pragma warning restore CS0618 // Type or member is obsolete
				Assert.IsNotNull(tweet);
				Assert.AreNotEqual(0, tweet.Id);
			}
		}

		[Explicit("Fails, don't know why, should be fixed. However, this is not the appropriate way to tweet with media anymore, and the alternate method works so this is low priority.")]
		[Test]
		public void Can_tweet_with_image_and_accented_char()
		{
			//This test currently fails. Don't know why. Response is an error
			//to do with authorisation failing, but everything looks correct.
			//Tweeting with image an no accented character works, using
			//alternate endpoint to tweet status with accented character and
			//no media also works, but this one fails if both conditions are true.
			//This method is deprecated anyway and using uploadmedia + the normal
			//status update with mediaids works even when an accented char is present
			//so not critical long term, but it would be great to understand why
			//this is an issue and possibly fix it.
			var service = GetAuthenticatedService();
			service.TraceEnabled = true;
			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "daniel_8bit.png"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var tweet = service.SendTweetWithMedia(new SendTweetWithMediaOptions
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
				{
					Status = "Can_tweet_with_image:Tweet and accented char à",
					Images = new Dictionary<string, Stream> { { "test", stream } }
				});
#pragma warning restore CS0618 // Type or member is obsolete

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(tweet);
				Assert.AreNotEqual(0, tweet.Id);
			}
		}

		[Test]
		public void Can_upload_media()
		{
			var service = GetAuthenticatedService();
			service.TraceEnabled = true;
			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "daniel_8bit.png"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var uploadedMedia = service.UploadMedia(new UploadMediaOptions
				{
					Media = new MediaFile() { FileName = "test", Content = stream }
				});

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(uploadedMedia);
				Assert.AreNotEqual(0, uploadedMedia.Media_Id);
			}
		}

		[Test]
		public void Can_tweet_uploaded_media_and_accented_char()
		{
			List<string> mediaIds = new List<string>(2);

			var service = GetAuthenticatedService();
			service.TraceEnabled = true;
			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "daniel_8bit.png"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var uploadedMedia = service.UploadMedia(new UploadMediaOptions
				{
					Media = new MediaFile { FileName = "test", Content = stream }
				});

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(uploadedMedia);
				Assert.AreNotEqual(0, uploadedMedia.Media_Id);
				mediaIds.Add(uploadedMedia.Media_Id);
			}

			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sparrow.jpg"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var uploadedMedia = service.UploadMedia(new UploadMediaOptions
				{
					Media = new MediaFile { FileName = "test2", Content = stream }
				});

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(uploadedMedia);
				Assert.AreNotEqual(0, uploadedMedia.Media_Id);
				mediaIds.Add(uploadedMedia.Media_Id);
			}

			service.SendTweet(new SendTweetOptions() { Status = "TweetMoaSharp:Can_tweet_uploaded_media_and_accented_char:Tweet and accented char à....", MediaIds = mediaIds });
		}

		[Test]
		public void Can_get_followers_on_first_page()
		{
			var service = GetAuthenticatedService();
			var followers = service.ListFollowers(new ListFollowersOptions());
			Assert.IsNotNull(followers);
		}

		[Test]
		public void Can_get_friends_on_first_page()
		{
			var service = GetAuthenticatedService();
			TwitterCursorList<TwitterUser> followers = service.ListFriends(new ListFriendsOptions());
			Assert.IsNotNull(followers);
		}

		[Test]
		public void Can_get_followers_from_authenticated_user()
		{
			var service = GetAuthenticatedService();
			var followers = service.ListFollowers(new ListFollowersOptions());
			Assert.IsNotNull(followers);
			Assert.IsTrue(followers.Count > 0);
		}

		[Test]
		public void Can_get_favorites_for()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListFavoriteTweets(new ListFavoriteTweetsOptions { ScreenName = _hero });

			Console.WriteLine(service.Response.Response);

			foreach (var tweet in tweets)
			{
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_create_and_destroy_favorite()
		{
			// http://twitter.com/#!/kellabyte/status/16578173168779264
			var service = GetAuthenticatedService();
			var fave = service.FavoriteTweet(new FavoriteTweetOptions { Id = 16578173168779264 });
			if (service.Response != null && (int)service.Response.StatusCode == 403)
			{
				Trace.WriteLine("This tweet is already a favorite.");
			}
			else
			{
				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(fave);
			}

			var unfave = service.UnfavoriteTweet(new UnfavoriteTweetOptions { Id = 16578173168779264 });
			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(unfave);
		}

		[Test]
		public void Can_get_favorites()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListFavoriteTweets(new ListFavoriteTweetsOptions());

			Console.WriteLine(service.Response.Response);

			foreach (var tweet in tweets)
			{
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_get_favorites_async()
		{
			var service = GetAuthenticatedService();
			var result = service.BeginListFavoriteTweets(new ListFavoriteTweetsOptions { ScreenName = _hero });
			var tweets = service.EndListFavoriteTweets(result);

			Console.WriteLine(service.Response.Response);

			foreach (var tweet in tweets)
			{
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_get_friends_or_followers_with_next_cursor()
		{
			var service = new TwitterService(_consumerKey, _consumerSecret);
			service.AuthenticateWith(_accessToken, _accessTokenSecret);

			var followers = service.ListFollowers(new ListFollowersOptions { ScreenName = _hero });
			Assert.IsNotNull(followers);
			Assert.IsNotNull(followers.NextCursor);
			Assert.IsNotNull(followers.PreviousCursor);
		}

		[Test]
		public void Can_create_and_destroy_saved_search()
		{
			var service = new TwitterService(_consumerKey, _consumerSecret);
			service.AuthenticateWith(_accessToken, _accessTokenSecret);

			// Twitter 403's on duplicate saved search requests, so delete if found
			var searches = service.ListSavedSearches();
			Assert.IsNotNull(searches);

			var existing = searches.SingleOrDefault(s => s.Query.Equals("tweetsharp"));
			if (existing != null)
			{
				var deleted = service.DeleteSavedSearch(new DeleteSavedSearchOptions { Id = existing.Id });
				Assert.IsNotNull(deleted);
				Assert.IsNotEmpty(deleted.Query);
				Assert.AreEqual(deleted.Query, existing.Query);
			}

			var search = service.CreateSavedSearch(new CreateSavedSearchOptions { Query = "tweetsharp" });
			Assert.IsNotNull(search);
			Assert.AreEqual("tweetsharp", search.Query);
		}

		[Test]
		public void Can_search()
		{
			var service = GetAuthenticatedService();
			var results = service.Search(new SearchOptions { Q = "tweetsharp", Count = 10 });

			Assert.IsNotNull(results);
			Assert.IsTrue(results.Statuses.Count() <= 10);

			foreach (var tweet in results.Statuses)
			{
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_search_until()
		{
			var service = GetAuthenticatedService();
			var results = service.Search(new SearchOptions { Q = "microsoft", Count = 10, Until = DateTime.Today.AddDays(-1) });

			Assert.IsNotNull(results);
			Assert.IsTrue(results.Statuses.Count() <= 10);

			Assert.IsFalse(results.Statuses.Any((s) => s.CreatedDate >= DateTime.Today));

			foreach (var tweet in results.Statuses)
			{
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_ListSuggestedUsers()
		{
			var service = GetAuthenticatedService();
			var results = service.ListSuggestedUsers(new ListSuggestedUsersOptions() { Lang = "en", Slug = "television" });

			Assert.IsNotNull(results);
			Assert.IsTrue(results.Users.Any());

			foreach (var user in results.Users)
			{
				Console.WriteLine("Suggested; {0}", user.ScreenName);
			}
		}

		[Test]
		public void Can_search_with_geo_and_lang()
		{
			var italyGeoCode = new TwitterGeoLocationSearch(41.9, 12.5, 10, TwitterGeoLocationSearch.RadiusType.Mi);
			var service = GetAuthenticatedService();
			var results = service.Search(new SearchOptions { Q = "papa", Geocode = italyGeoCode, Lang = "en", Count = 100, });

			Assert.IsNotNull(results);
			if (!results.Statuses.Any())
			{
				Assert.Inconclusive("No tweets to check the location of to match within search radius");
			}

			Assert.IsTrue(results.Statuses.Count() <= 100);
			var geoTaggedTweets = results.Statuses.Where(x => x.Location != null);
			if (!geoTaggedTweets.Any())
			{
				Assert.Inconclusive("Unable to find tweets that were geo tagged for this test");
			}
			foreach (var tweet in geoTaggedTweets)
			{
				Console.WriteLine("{0} says '{1}' ({2})", tweet.User.ScreenName, tweet.Text, tweet.Id);

				//Twitter API does not return coordinates in search request
				Assert.IsTrue(tweet.IsWithinSearchRadius(italyGeoCode));
			}
		}

		[Test]
		public void Searches_with_explicit_include_options_still_work()
		{
			var service = GetAuthenticatedService();
			var results = service.Search(new SearchOptions
			{
				Count = 500,
				Resulttype = TwitterSearchResultType.Mixed,
				IncludeEntities = false,
				Q = "stackoverflow"
			});

			Assert.IsNotNull(results);
			foreach (var result in results.Statuses)
			{
				Console.WriteLine(result.Text);
			}
		}

		[Test]
		public void Can_get_raw_source_from_search()
		{
			var service = GetAuthenticatedService();
			var results = service.Search(new SearchOptions { Q = "tweetsharp", Count = 10 });
			Assert.IsNotNull(results);
			Assert.IsTrue(results.Statuses.Count() <= 10);
			if (!results.Statuses.Any())
			{
				Assert.Ignore("No search results provided for this test");
			}

			foreach (var tweet in results.Statuses)
			{
				Assert.IsNotEmpty(tweet.RawSource);
				Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_get_friendship_info()
		{
			var service = GetAuthenticatedService();
			var friendship = service.GetFriendshipInfo(new GetFriendshipInfoOptions { SourceScreenName = "jdiller", TargetScreenName = "danielcrenna" });

			Assert.IsNotNull(friendship);
			Assert.IsNotNull(friendship.Relationship);
		}

		[Test]
		public void Can_get_user_suggestion_categories_and_users()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

			var service = GetAuthenticatedService();
			var categories = service.ListSuggestedUserCategories(new ListSuggestedUserCategoriesOptions());
			Assert.IsNotNull(categories);
			Assert.IsTrue(categories.Count() > 0);

			foreach (var category in categories)
			{
				Trace.WriteLine(category.RawSource);
				Trace.WriteLine(string.Format("{0}({1})", category.Name, category.Slug));
			}

			var suggestions = service.ListSuggestedUsers(new ListSuggestedUsersOptions() { Slug = categories.First().Slug });
			Assert.IsNotNull(suggestions);
			Assert.IsNotNull(suggestions.Users);
			Assert.IsTrue(suggestions.Users.Count() > 0);

			foreach (var user in suggestions.Users)
			{
				Trace.WriteLine(user.ScreenName);
			}
		}

		[Test]
		public void Can_get_tweet_default_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new GetTweetOptions { Id = 778329152193781762 };
			TestSyncAndAsync(options, service.GetTweet, service.GetTweetAsync, tweet =>
			{
				Assert.IsNotNull(tweet);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw… https://t.co/M4WBDumXl9", tweet.Text);
			});
		}

		[Test]
		public void Can_get_tweet_compat_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new GetTweetOptions { Id = 778329152193781762, TweetMode = TweetMode.Compatibility };

			TestSyncAndAsync(options, service.GetTweet, service.GetTweetAsync, tweet =>
			{
				Assert.IsNotNull(tweet);
				Assert.IsNotEmpty(tweet.Text);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw… https://t.co/M4WBDumXl9", tweet.Text);
			});
		}

		[Test]
		public void Can_get_tweet_extended_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new GetTweetOptions { Id = 778329152193781762, TweetMode = TweetMode.Extended };

			TestSyncAndAsync(options, service.GetTweet, service.GetTweetAsync, tweet =>
			{
				Assert.IsNotNull(tweet);
				Assert.IsNotNull(service.Response);
				Assert.IsNotEmpty(tweet.FullText, "Full text was empty!");
				Assert.IsNotNull(tweet.DisplayTextRange);
				Assert.AreEqual(2, tweet.DisplayTextRange.Length);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
				Assert.IsNull(tweet.Text);
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw https://t.co/rJAbUfEK9a https://t.co/UwCfXRETR3", tweet.FullText);
			});
		}

		[Test]
		public void Can_list_tweets_on_user_timeline_default_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new ListTweetsOnUserTimelineOptions { ScreenName = "collinsauve", SinceId = 778329152193781761, MaxId = 778329152193781763 };
			TestSyncAndAsync(options, service.ListTweetsOnUserTimeline, service.ListTweetsOnUserTimelineAsync, result =>
			{
				Assert.IsNotNull(result);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);

				var tweets = result.ToArray();
				Assert.AreEqual(1, tweets.Length);

				var tweet = tweets[0];
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw… https://t.co/M4WBDumXl9", tweet.Text);
			});
		}

		[Test]
		public void Can_list_tweets_on_user_timeline_compat_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new ListTweetsOnUserTimelineOptions { ScreenName = "collinsauve", SinceId = 778329152193781761, MaxId = 778329152193781763, TweetMode = "compat" };
			TestSyncAndAsync(options, service.ListTweetsOnUserTimeline, service.ListTweetsOnUserTimelineAsync, result =>
			{

				Assert.IsNotNull(result);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);

				var tweets = result.ToArray();
				Assert.AreEqual(1, tweets.Length);

				var tweet = tweets[0];
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw… https://t.co/M4WBDumXl9", tweet.Text);
			});
		}

		[Test]
		public void Can_list_tweets_on_user_timeline_extended_tweet_mode()
		{
			var service = GetAuthenticatedService();
			var options = new ListTweetsOnUserTimelineOptions { ScreenName = "collinsauve", SinceId = 778329152193781761, MaxId = 778329152193781763, TweetMode = "extended" };
			TestSyncAndAsync(options, service.ListTweetsOnUserTimeline, service.ListTweetsOnUserTimelineAsync, result =>
			{
				Assert.IsNotNull(result);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);

				var tweets = result.ToArray();
				Assert.AreEqual(1, tweets.Length);

				var tweet = tweets[0];
				Assert.AreEqual(@"Upcoming changes to Tweets - Allows tweets more than 140 characters. Looking at extending TweetMoaSharp @yortw https://t.co/rJAbUfEK9a https://t.co/UwCfXRETR3", tweet.FullText);
			});
		}

		[Test]
		public void Can_list_muted_users()
		{
			var service = GetAuthenticatedService();
			var options = new ListMutedUsersOptions { };
			TestSyncAndAsync(options, service.ListMutedUsers, service.ListMutedUsersAsync, result =>
			{
				Assert.IsNotNull(result);
				Assert.IsNotNull(service.Response);
				Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);

				var users = result.ToArray();
				Assert.IsTrue(users.Length > 0);
			});
		}

		private static void TestSyncAndAsync<TOptions, TResult>(TOptions options, Func<TOptions, TResult> syncFunc, Func<TOptions, Task<TwitterAsyncResult<TResult>>> asyncFunc, Action<TResult> assertFunc)
		{
			var syncResult = syncFunc(options);
			assertFunc(syncResult);
			var asyncResultTask = asyncFunc(options);
			asyncResultTask.Wait();
			assertFunc(asyncResultTask.Result.Value);
		}

		[Test]
		public void Can_get_tweet_with_multiple_images()
		{
			var service = GetAuthenticatedService();
			var tweet = service.GetTweet(new GetTweetOptions { Id = 568680219474726912 });

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
			Assert.AreEqual(3, tweet.ExtendedEntities.Count());
			Assert.AreEqual(1, tweet.Entities.Count());
		}

		[Test]
		public void Can_get_tweet_with_animated_gif()
		{
			var service = GetAuthenticatedService();
			var tweet = service.GetTweet(new GetTweetOptions { Id = 480032281591939072 });

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
			Assert.AreEqual(1, tweet.ExtendedEntities.Count());
		}

		[Test]
		public void Can_get_tweet_with_merged_entities()
		{
			var service = GetAuthenticatedService(new JsonSerializer() { MergeMultiplePhotos = true });
			var tweet = service.GetTweet(new GetTweetOptions { Id = 568680219474726912 });

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
			Assert.AreEqual(3, tweet.ExtendedEntities.Count());
			Assert.AreEqual(3, tweet.Entities.Count());
		}

		[Test]
		public void Can_get_tweet_with_video()
		{
			var service = GetAuthenticatedService(new JsonSerializer() { MergeMultiplePhotos = true });
			var tweet = service.GetTweet(new GetTweetOptions { Id = 560049149836808192 });

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
			Assert.AreEqual(1, tweet.ExtendedEntities.Count());
			var ve = tweet.ExtendedEntities.First();
			Assert.AreEqual(4, ve.Sizes.Count());
			Assert.IsNotNull(ve.VideoInfo);
			Assert.AreEqual(30008, ve.VideoInfo.DurationMs);
			Assert.GreaterOrEqual(ve.VideoInfo.Variants.Count(), 4);
			Assert.AreEqual(2, ve.VideoInfo.AspectRatio.Count);
			Assert.AreEqual(1, ve.VideoInfo.AspectRatio[0]);
			Assert.AreEqual(1, ve.VideoInfo.AspectRatio[1]);
			Assert.IsTrue(ve.VideoInfo.Variants.Any((v) => v.ContentType == "video/mp4"));
			Assert.IsTrue(ve.VideoInfo.Variants.Any((v) => !String.IsNullOrEmpty(v?.Url?.ToString())));
		}

		[Test]
		public void Can_get_tweet_async()
		{
			var service = GetAuthenticatedService();
			var result = service.BeginGetTweet(new GetTweetOptions { Id = 10080880705929216 });
			var tweet = service.EndGetTweet(result);

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
		}

		[Test]
		public void Can_get_tweet_uses_default_tweet_mode()
		{
			var service = GetAuthenticatedService();
			service.TweetMode = TweetMode.Extended;
			var tweet = service.GetTweet(new GetTweetOptions { Id = 10080880705929216 });

			Assert.IsNotNull(tweet);
			Assert.IsNotEmpty(tweet.FullText);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
		}

		[Test]
		public void Can_get_quoted_tweet_async()
		{
			var service = GetAuthenticatedService();
			var result = service.BeginGetTweet(new GetTweetOptions { Id = 642272776582230016 });
			var tweet = service.EndGetTweet(result);

			Assert.IsNotNull(tweet);
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
			Assert.IsTrue(tweet.IsQuoteStatus);
			Assert.IsNotNull(tweet.QuotedStatus);
			Assert.IsNotEmpty(tweet.QuotedStatusIdStr);
			Assert.IsNotNull(tweet.QuotedStatusId);
		}

		[Test]
		public void Can_get_entities_on_timeline()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

			foreach (var tweet in tweets)
			{
				Assert.IsNotNull(tweet.Entities);
				if (tweet.Entities.HashTags.Any())
				{
					foreach (var hashtag in tweet.Entities.HashTags)
					{
						Assert.IsNotEmpty(hashtag.Text);
					}
				}
				if (tweet.Entities.Urls.Count() > 0)
				{
					foreach (var url in tweet.Entities.Urls)
					{
						Assert.IsNotEmpty(url.Value);
					}
				}
				if (tweet.Entities.Mentions.Count() > 0)
				{
					foreach (var mention in tweet.Entities.Mentions)
					{
						Assert.IsNotEmpty(mention.ScreenName);
					}
				}
			}
		}

		[Test]
		public void Can_parse_multibyte_entities()
		{
			var service = GetAuthenticatedService();
			var tweet = service.GetTweet(new GetTweetOptions() { Id = 637228279825608706 });

			Assert.IsNotNull(tweet.Entities);
			Assert.AreEqual(3, tweet.Entities.Count());
			Assert.AreEqual("Heerlijk! 😋 Wij kijken er naar uit 😋 Thnx voor dit supermooie initiatief 👍🏻 <a href=\"https://twitter.com/degoudenaar\" target=\"_blank\">@degoudenaar</a> <a href=\"https://twitter.com/search?q=SlagerijvanHoof\" target=\"_blank\">#SlagerijvanHoof</a> <a href=\"https://twitter.com/brabantsedag/status/637195781049581568\" target=\"_blank\">https://t.co/W2TeREt0Hd</a>", tweet.TextAsHtml);
		}

		[Test]
		public void Can_coalesce_entities_on_timeline()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

			var passed = false;

			foreach (var tweet in tweets)
			{
				//Appears on retweets that are over 140 chars multiple entities near the end can end up being assigned a start of 139.
				//Twitter recommends using entities from the original tweet anyway.

				var tweetToTest = tweet;
				if (tweetToTest.RetweetedStatus != null)
					tweetToTest = tweetToTest.RetweetedStatus;

				if (tweetToTest.Entities == null)
				{
					continue;
				}

				var entities = tweetToTest.Entities.Coalesce();
				if (entities.Count() < 2)
				{
					continue;
				}

				var previous = -1;
				foreach (var entity in entities)
				{
					Assert.IsTrue(previous < entity.StartIndex);
					previous = entity.StartIndex;
				}

				passed = true;
			}

			if (!passed)
			{
				Assert.Ignore("This test pass yielded no entities with both a hashtag and a URL.");
			}
		}

		[Test]
		public void Can_get_tweets_on_user_timeline_with_since_paging()
		{
			var service = GetAuthenticatedService();
			var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { Count = 200 }).ToList();
			foreach (var tweet in tweets)
			{
				Assert.IsNotNull(tweet.RawSource);
				Assert.IsNotNull(tweet.Entities);

				Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Text);
			}
			if (!tweets.Any()) Assert.Ignore();
			var sinceId = tweets.Last().Id;
			var tweets2 = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { SinceId = sinceId, Count = 200 });
			foreach (var tweet in tweets2)
			{
				Assert.IsNotNull(tweet.RawSource);
				Assert.IsNotNull(tweet.Entities);
			}
		}

		[Test]
		public void Can_get_tweets_on_specified_user_timeline()
		{
			var service = GetAuthenticatedService();

			var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = "mabster" });
			foreach (var tweet in tweets)
			{
				Assert.IsNotNull(tweet.RawSource);
				Assert.IsNotNull(tweet.Entities);

				Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Text);
			}
		}

		[Test]
		public void Can_get_user_lists()
		{
			var service = GetAuthenticatedService();
			var lists = service.ListListsFor(new ListListsForOptions() { ScreenName = _hero });

			Assert.IsNotNull(lists);
			if (!lists.Any())
			{
				Assert.Ignore("This test account has no lists");
			}

			foreach (var list in lists)
			{
				Assert.IsNotEmpty(list.Name);
				Trace.WriteLine(list.Name);
			}
		}

		[Test]
		public void Can_get_user_owned_lists()
		{
			var service = GetAuthenticatedService();
			var lists = service.ListOwnedListsFor(new ListOwnedListsForOptions() { ScreenName = _hero });

			Assert.IsNotNull(lists);
			if (!lists.Any())
			{
				Assert.Ignore("This test account has no lists");
			}

			foreach (var list in lists)
			{
				Assert.IsNotEmpty(list.Name);
				Trace.WriteLine(list.Name);
			}
		}

		[Test]
		public void Can_add_multiplelistusers()
		{
			var service = GetAuthenticatedService();
			var list = service.CreateList(new CreateListOptions() { Name = "SecondTestList" });
			try
			{
				Assert.IsNotNull(list);

				var result = service.AddListMembers(new AddListMembersOptions() { ListId = list.Id, ScreenName = new string[] { "yortw", "elonmusk" } });
				Assert.IsNotNull(result);
				Thread.Sleep(50); // Twitter needs a little time to update the list
				var members = service.ListListMembers(new ListListMembersOptions() { ListId = result.Id });
				Assert.IsNotNull(members);
				Assert.AreEqual(2, members.Count);
			}
			finally
			{
				service.DeleteList(new DeleteListOptions() { ListId = list.Id });
			}
		}

		[Test]
		public void Can_update_list()
		{
			var service = GetAuthenticatedService();
			var list =
					service.CreateList(new CreateListOptions()
					{
						Name = "Name",
						Description = "Description",
						Mode = TwitterListMode.Private
					});

			try
			{
				Assert.IsNotNull(list);
				System.Threading.Thread.Sleep(1000); //Needed because sometimes the newly created list isn't available immediately.
				var list2 = service.UpdateList(new UpdateListOptions { ListId = list.Id, Name = "Name 2", Description = "Description 2" });
				Assert.IsNotNull(list2);
				Assert.AreEqual("Name 2", list2.Name);
				Assert.AreEqual("Description 2", list2.Description);
				Assert.AreEqual("private", list2.Mode);

				System.Threading.Thread.Sleep(1000); //Needed because sometimes the newly created list isn't available immediately.
				var list3 = service.UpdateList(new UpdateListOptions { ListId = list.Id, Name = "Name 3", Description = "Description 3", Mode = TwitterListMode.Public });
				Assert.IsNotNull(list3);
				Assert.AreEqual("Name 3", list3.Name);
				Assert.AreEqual("Description 3", list3.Description);
				Assert.AreEqual("public", list3.Mode);

			}
			finally
			{
				service.DeleteList(new DeleteListOptions() { ListId = list.Id });
			}
		}

		[Test]
		public void Can_limit_list_members()
		{
			var service = GetAuthenticatedService();
			var lists = service.ListListsFor(new ListListsForOptions() { ScreenName = "yortw" });

			Assert.IsNotNull(lists);
			if (!lists.Any())
			{
				Assert.Ignore("This test account has no lists");
			}

			var list = (from l in lists where l.MemberCount > 1 select l).FirstOrDefault();
			Assert.IsNotNull(list, "No lists with more than one member.");

			var membersCursor = service.ListListMembers(new ListListMembersOptions() { ListId = list.Id, Count = 1 });
			Assert.AreEqual(1, membersCursor.Count);
		}

		[Test]
		public void Can_create_and_delete_list()
		{
			var service = GetAuthenticatedService();
			var list =
					service.CreateList(new CreateListOptions
					{
						ListOwner = _hero,
						Name = "test-list",
						Mode = TwitterListMode.Public
					});

			Assert.IsNotNull(list);
			Assert.IsNotEmpty(list.Name);
			Assert.AreEqual(list.Name, "test-list");

			System.Threading.Thread.Sleep(1000);

			list = service.DeleteList(new DeleteListOptions { ListId = list.Id });
			Assert.IsNotNull(list);
			Assert.IsNotEmpty(list.Name);
			Assert.AreEqual(list.Name, "test-list");
		}

		[Test]
		public void Can_get_followers_ids()
		{
			var service = GetAuthenticatedService();
			var followers = service.ListFollowerIdsOf(new ListFollowerIdsOfOptions() { ScreenName = _hero });
			Assert.IsNotNull(followers);
			Assert.IsTrue(followers.Count > 0);
		}

		[Test]
		public void Can_get_friend_ids()
		{
			var service = GetAuthenticatedService();
			var friends = service.ListFriendIdsOf(new ListFriendIdsOfOptions() { ScreenName = _hero });
			Assert.IsNotNull(friends);
			Assert.IsTrue(friends.Count > 0);
		}

		[Test]
		public void Can_get_available_local_trend_locations()
		{
			var service = GetAuthenticatedService();
			var locations = service.ListAvailableTrendsLocations();
			Assert.IsNotNull(locations);

			foreach (var location in locations)
			{
				Trace.WriteLine(string.Format("{0}:{1}, {2}[{3}]", location.WoeId, location.Name, location.Country, location.PlaceType.Name));
			}
		}

		[Test]
		public void Can_update_profile_image()
		{
			var service = GetAuthenticatedService();
			var user = service.UpdateProfileImage(new UpdateProfileImageOptions { ImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "daniel_8bit.png") });
			Assert.IsNotNull(user);
			Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
		}


		[Test]
		public void Can_get_local_trends()
		{
			var service = GetAuthenticatedService();
			var trendList = service.ListLocalTrendsFor(new ListLocalTrendsForOptions { Id = 4118 });
			Assert.IsNotNull(trendList);

			foreach (var trend in trendList)
			{
				Trace.WriteLine(trend.Query);
			}
		}

		[Test]
		public void Can_get_multiple_user_profiles()
		{
			var service = GetAuthenticatedService();
			var users = service.ListUserProfilesFor(new ListUserProfilesForOptions() { ScreenName = new[] { "danielcrenna", "jdiller" } });

			Assert.IsNotNull(users);
			Assert.AreEqual(2, users.Count());
		}

		private static void AssertResultWas(TwitterService service, HttpStatusCode statusCode)
		{
			Assert.IsNotNull(service.Response);
			Assert.AreEqual(statusCode, service.Response.StatusCode);
		}

		private static void AssertRateLimitStatus(TwitterService service)
		{
			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Assert.AreNotEqual(0, rate.HourlyLimit);
			Console.WriteLine();
			Console.WriteLine("{0} / {1} API calls remaining", rate.RemainingHits, rate.HourlyLimit);
		}

		private TwitterService GetAuthenticatedService(JsonSerializer serializer)
		{
			var service = new TwitterService(_consumerKey, _consumerSecret);
			if (serializer != null)
			{
				service.Serializer = serializer;
				service.Deserializer = serializer;
			}

			service.TraceEnabled = true;
			service.AuthenticateWith(_accessToken, _accessTokenSecret);
			return service;
		}

		private TwitterService GetAuthenticatedService()
		{
			return GetAuthenticatedService(null);
		}

		[Test]
		public void Can_get_friendship_lookup()
		{
			var service = GetAuthenticatedService();
			var friendships = service.ListFriendshipsFor(new ListFriendshipsForOptions() { ScreenName = new[] { "danielcrenna" } });
			Assert.IsNotNull(friendships);
		}

		[Test]
		public void Can_loop_through_followers()
		{
			var service = GetAuthenticatedService();
			var me = service.GetUserProfile(new GetUserProfileOptions());

			var count = 0;
			var followers = service.ListFollowers(new ListFollowersOptions { UserId = me.Id });
			count += followers.Count;
			while (followers.NextCursor != 0)
			{
				followers = service.ListFollowers(new ListFollowersOptions { UserId = me.Id, Cursor = followers.NextCursor });
				count += followers.Count;
			}
			Assert.AreEqual(me.FollowersCount, count);
		}

		[Test]
		public void Can_list_tweets_on_list()
		{
			var service = GetAuthenticatedService();
			var result = service.BeginListTweetsOnList(new ListTweetsOnListOptions
			{
				OwnerScreenName = "Joesebok",
				Slug = "poker",
				IncludeRts = true,
				SinceId = 308773934705299458
			});
			var tweets = service.EndListTweetsOnList(result);
			Assert.IsNotNull(tweets);

			foreach (var tweet in tweets)
			{
				Console.WriteLine(tweet.Id);
			}
		}

		[Test]
		public void Can_get_rate_limit_status_summary()
		{
			var service = GetAuthenticatedService();
			var summary = service.GetRateLimitStatus(new GetRateLimitStatusOptions());
			Assert.IsNotNull(summary);
			Assert.IsNotEmpty(summary.AccessToken);



			Console.WriteLine(service.Response.Response);

		}

		[Test]
		public void Can_Deserialize_Integer_GeoCoordinates()
		{
			//coordinates of this tweet are 2 integers  "{\"type\":\"Point\",\"coordinates\":[10, 234]}";
			TwitterService service = GetAuthenticatedService();
			TwitterStatus tweet = service.GetTweet(new GetTweetOptions() { Id = 294853375609163776 });
			Assert.NotNull(tweet);
		}

		[Test]
		public void Recursive_issues_on_private_accounts()
		{
			TwitterService service = GetAuthenticatedService();

			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RecursiveProfileData.json"), FileMode.Open))
			{
				using (var reader = new System.IO.StreamReader(stream))
				{
					var tweets = service.Deserialize<IEnumerable<TwitterStatus>>(reader.ReadToEnd());
					Assert.IsNotNull(tweets);
				}
			}
		}

		[Test]
		public void Can_Upload_VideoChunked()
		{
			var service = GetAuthenticatedService();
			service.TraceEnabled = true;

			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"big_buck_bunny.mp4"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				TwitterChunkedMedia uploadedMedia = InitialiseMediaUpload(service, stream);

				UploadMediaChunks(service, stream, uploadedMedia);

				FinializeMediaAndWaitForProcessing(service, uploadedMedia);

				// Now send a tweet with the media attached
				var twitterStatus = service.SendTweet(new SendTweetOptions()
				{
					Status = "(c) copyright 2008, Blender Foundation / www.bigbuckbunny.org - " + Guid.NewGuid().ToString(),
					MediaIds = new string[] { uploadedMedia.MediaId.ToString() }
				});

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(twitterStatus);
			}

		}

		[Test]
		public void Can_GetPublicTimeline_AppAuthOnly()
		{
			var service = new TwitterService(_consumerKey, _consumerSecret);
			service.TraceEnabled = true;
			ListTweetsOnUserTimelineOptions lto = new ListTweetsOnUserTimelineOptions();
			lto.ScreenName = "yortw";
			lto.Count = 5;
			lto.IncludeRts = true;
			lto.ExcludeReplies = true;
			lto.TrimUser = true;
			var result = service.ListTweetsOnUserTimeline(lto);
			var tweets = result.OrderByDescending(data => data.Id).ToList();
			Assert.IsNotNull(tweets);
			Assert.IsTrue(tweets.Count > 0);
		}

		private static TwitterChunkedMedia InitialiseMediaUpload(TwitterService service, FileStream stream)
		{
			var uploadedMedia = service.UploadMediaInit(new UploadMediaInitOptions
			{
				MediaType = "video/mp4",
				TotalBytes = stream.Length,
				MediaCategory = TwitterMediaCategory.Video
			});

			AssertResultWas(service, HttpStatusCode.Accepted);
			Assert.IsNotNull(uploadedMedia);
			Assert.AreNotEqual(0, uploadedMedia.MediaId);
			return uploadedMedia;
		}

		private static void UploadMediaChunks(TwitterService service, FileStream stream, TwitterChunkedMedia uploadedMedia)
		{
			long chunkSize = 1024 * 512;
			long index = 0;
			byte[] buffer = new byte[chunkSize];

			while (stream.Position < stream.Length)
			{
				int thisChunkSize = (int)Math.Min(stream.Length - stream.Position, chunkSize);
				if (thisChunkSize != chunkSize)
					buffer = new byte[thisChunkSize];

				stream.Read(buffer, 0, thisChunkSize);
				var ms = new System.IO.MemoryStream(buffer);

				service.UploadMediaAppend(new UploadMediaAppendOptions
				{
					MediaId = uploadedMedia.MediaId,
					SegmentIndex = index,
					Media = new MediaFile()
					{
						//FileName = "test_video.mp4",
						Content = ms
					}
				});
				AssertResultWas(service, HttpStatusCode.NoContent);

				index++;
			}
		}

		private static void FinializeMediaAndWaitForProcessing(TwitterService service, TwitterChunkedMedia uploadedMedia)
		{
			var result = service.UploadMediaFinalize(new UploadMediaFinalizeOptions()
			{
				MediaId = uploadedMedia.MediaId
			});

			var done = false;
			while (!done)
			{
				AssertResultWas(service, HttpStatusCode.OK);
				if (result.ProcessingInfo.Error != null)
				{
					throw new InvalidOperationException(result.ProcessingInfo.Error.Code + " - " + result.ProcessingInfo.Error.Message);
				}

				var state = (result.ProcessingInfo?.State ?? TwitterMediaProcessingState.Succeeded);
				done = state == TwitterMediaProcessingState.Succeeded || state == TwitterMediaProcessingState.Failed;
				if (!done)
				{
					System.Threading.Thread.Sleep(Convert.ToInt32((result?.ProcessingInfo?.CheckAfterSeconds ?? 5) * 1000));
					result = service.UploadMediaCheckStatus(new UploadMediaCheckStatusOptions() { MediaId = uploadedMedia.MediaId });
				}
			}
		}

		[Test]
		public async System.Threading.Tasks.Task Can_Upload_VideoChunkedAsync()
		{
			var service = GetAuthenticatedService();
			service.TraceEnabled = true;

			//using (var stream = new FileStream("test_video.mp4", FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"big_buck_bunny.mp4"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				TwitterChunkedMedia uploadedMedia = await InitialiseMediaUploadAsync(service, stream, false, TwitterMediaCategory.Video);

				await UploadMediaChunksAsync(service, stream, uploadedMedia);

				await FinializeMediaAndWaitForProcessingAsync(service, uploadedMedia);

				// Now send a tweet with the media attached
				var twitterStatus = await service.SendTweetAsync(new SendTweetOptions()
				{
					Status = "(c) copyright 2008, Blender Foundation / www.bigbuckbunny.org - " + Guid.NewGuid().ToString(),
					MediaIds = new string[] { uploadedMedia.MediaId.ToString() }
				});

				AssertResultWas(service, HttpStatusCode.OK);
				Assert.IsNotNull(twitterStatus);
			}

		}

		private static async System.Threading.Tasks.Task<TwitterChunkedMedia> InitialiseMediaUploadAsync(TwitterService service, FileStream stream, bool shared, string category)
		{
			var uploadedMedia = await service.UploadMediaInitAsync(new UploadMediaInitOptions
			{
				MediaType = "video/mp4",
				TotalBytes = stream.Length,
				MediaCategory = category,
				Shared = shared
			});

			AssertResultWas(service, HttpStatusCode.Accepted);
			Assert.IsNotNull(uploadedMedia);
			Assert.AreNotEqual(0, uploadedMedia.Value.MediaId);
			return uploadedMedia.Value;
		}

		private static async System.Threading.Tasks.Task UploadMediaChunksAsync(TwitterService service, FileStream stream, TwitterChunkedMedia uploadedMedia)
		{
			long chunkSize = 1024 * 512;
			long index = 0;
			byte[] buffer = new byte[chunkSize];

			while (stream.Position < stream.Length)
			{
				int thisChunkSize = (int)Math.Min(stream.Length - stream.Position, chunkSize);
				if (thisChunkSize != chunkSize)
					buffer = new byte[thisChunkSize];

				stream.Read(buffer, 0, thisChunkSize);
				var ms = new System.IO.MemoryStream(buffer);

				await service.UploadMediaAppendAsync(new UploadMediaAppendOptions
				{
					MediaId = uploadedMedia.MediaId,
					SegmentIndex = index,
					Media = new MediaFile()
					{
						//FileName = "test_video.mp4",
						Content = ms
					}
				});
				AssertResultWas(service, HttpStatusCode.NoContent);

				index++;
			}
		}

		private static async System.Threading.Tasks.Task FinializeMediaAndWaitForProcessingAsync(TwitterService service, TwitterChunkedMedia uploadedMedia)
		{
			var result = (await service.UploadMediaFinalizeAsync(new UploadMediaFinalizeOptions()
			{
				MediaId = uploadedMedia.MediaId
			})).Value;

			var done = false;
			while (!done)
			{
				AssertResultWas(service, HttpStatusCode.OK);
				if (result.ProcessingInfo.Error != null)
				{
					throw new InvalidOperationException(result.ProcessingInfo.Error.Code + " - " + result.ProcessingInfo.Error.Message);
				}

				var state = (result.ProcessingInfo?.State ?? TwitterMediaProcessingState.Succeeded);
				done = state == TwitterMediaProcessingState.Succeeded || state == TwitterMediaProcessingState.Failed;
				if (!done)
				{
					System.Threading.Thread.Sleep(Convert.ToInt32((result?.ProcessingInfo?.CheckAfterSeconds ?? 5) * 1000));
					result = (await service.UploadMediaCheckStatusAsync(new UploadMediaCheckStatusOptions() { MediaId = uploadedMedia.MediaId })).Value;
				}
			}
		}

		[Test]
		public void Can_Send_DirectMessage()
		{
			var service = GetAuthenticatedService();
			var user = service.GetUserProfileFor(new GetUserProfileForOptions() { ScreenName = "yortwdevtest" });
			var result = service.SendDirectMessage
			(
				new SendDirectMessageOptions()
				{
					Text = "Hello! #welcome " + DateTime.Now,
					Recipientid = user.Id,
					Quickreplies = new TwitterQuickReplyOption[]
					{
						new TwitterQuickReplyOption()
						{
							Label= "Hi yourself",
							Description = "Welcome",
							Metadata = "1"
						},
						new TwitterQuickReplyOption()
						{
							Label= "Go away",
							Description = "busy",
							Metadata = "2"
						}
					}
				}
			);

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Event);
			Assert.NotZero(result.Event.Id);

			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);
		}

		[Test]
		public async Task Can_Send_DirectMessageWithMedia()
		{
			var service = GetAuthenticatedService();
			long mediaId = 0;

			using (var stream = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"big_buck_bunny.mp4"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				TwitterChunkedMedia uploadedMedia = await InitialiseMediaUploadAsync(service, stream, false, TwitterMediaCategory.DirectMessage_Video);

				await UploadMediaChunksAsync(service, stream, uploadedMedia);

				await FinializeMediaAndWaitForProcessingAsync(service, uploadedMedia);

				mediaId = uploadedMedia.MediaId;
			}

			var user = service.GetUserProfileFor(new GetUserProfileForOptions() { ScreenName = "yortw" });
			var result = service.SendDirectMessage
			(
				new SendDirectMessageOptions()
				{
					Text = "Hello! #welcome " + DateTime.Now,
					Recipientid = user.Id,
					Mediaid = mediaId,
					Mediatype = "media"
				}
			);

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Event);
			Assert.NotZero(result.Event.Id);

			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);
		}

		[Test]
		public void Can_Get_DirectMessage()
		{
			var service = GetAuthenticatedService();
			var result = service.GetDirectMessage(new GetDirectMessageOptions() { Id = 1041227137464692741 });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Event);
			Assert.NotZero(result.Event.Id);

			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);
		}

		[Test]
		public void Can_List_DirectMessages()
		{
			var service = GetAuthenticatedService();
			var result = service.ListDirectMessages(new ListDirectMessagesOptions() { Count = 50 });

			AssertResultWas(service, HttpStatusCode.OK);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Events);
			Assert.Greater(result.Events.Count(), 0);
			Assert.IsNotNull(result.Apps);
			Assert.Greater(result.Apps.Count, 0);
			Assert.IsTrue(result.Apps.Any());

		}

		[Test]
		public void Can_Delete_DirectMessage()
		{
			var service = GetAuthenticatedService();
			var user = service.GetUserProfileFor(new GetUserProfileForOptions() { ScreenName = "yortwdevtest" });

			long directMessageId = 0;

			#region Create DM

			var result = service.SendDirectMessage
			(
				new SendDirectMessageOptions()
				{
					Text = "Hello! #welcome " + DateTime.Now,
					Recipientid = user.Id,
					Quickreplies = new TwitterQuickReplyOption[]
					{
						new TwitterQuickReplyOption()
						{
							Label= "Hi yourself",
							Description = "Welcome",
							Metadata = "1"
						},
						new TwitterQuickReplyOption()
						{
							Label= "Go away",
							Description = "busy",
							Metadata = "2"
						}
					}
				}
			);

			directMessageId = result.Event.Id;

			#endregion

			service.DeleteDirectMessage(new DeleteDirectMessageOptions() { Id = directMessageId });

			AssertResultWas(service, HttpStatusCode.NoContent);

			var rate = service.Response.RateLimitStatus;
			Assert.IsNotNull(rate);
			Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);
		}

	}
}