using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace TweetSharp
{
#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterSearchResultType
	{
		Mixed,
		Recent,
		Popular
	}

#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterProfileImageSize
	{
		Bigger,
		Normal,
		Mini
	}

#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterEntityType
	{
		HashTag,
		Mention,
		Url,
		Media
	}

#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterPlaceType
	{
		City,
		Neighborhood,
		Country,
		Admin,
		POI
	}

#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterMediaType
	{
		Photo,
		Video,
		AnimatedGif
	}

#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public enum TwitterListMode
	{
		Public,
		Private
	}

	public class TwitterMediaCategory
	{
		private TwitterMediaCategory()
		{
		}

		public const string Image = "tweet_image";
		public const string AnimatedGif = "tweet_gif";
		public const string Video = "tweet_video";
	}

	/// <summary>
	/// Specifies how tweets are returned with regards to changes to the 140 character limit.
	/// </summary>
	/// <remarks>
	/// <para>See https://dev.twitter.com/overview/api/upcoming-changes-to-tweets for more details.</para>
	/// </remarks>
	public class TweetMode
	{

		private TweetMode()
		{
		}

		/// <summary>
		/// Tweet payload will work with all existing integrations, regardless of tweet content.
		/// </summary>
		public const string Compatibility = "compat";
		/// <summary>
		/// Tweet payload contains all information to render tweets that contain more than 140 characters.
		/// </summary>
		public const string Extended = "extended";

	}
}