using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{

#if !SILVERLIGHT && !WINRT
	/// <summary>
	/// Represents an event for a private direct message between two users.
	/// </summary>
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class TwitterDirectMessageResult : ITwitterModel
	{
#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("event")]
		public TwitterDirectMessageModels Event { get; set; }
		public string RawSource { get; set; }
	}

#if !SILVERLIGHT && !WINRT
		/// <summary>
		/// Represents an event for a private direct message between two users.
		/// </summary>
		[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class TwitterDirectMessageModels : TwitterDirectMessageEvent
	{

		private TwitterDirectMessage _message;

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("message_create")]
		public virtual TwitterDirectMessage Message
		{
			get { return _message; }
			set
			{
				if (_message == value)
				{
					return;
				}

				_message = value;
				OnPropertyChanged("Message");
			}
		}

	}

#if !SILVERLIGHT && !WINRT
	/// <summary>
	/// Represents a list of sent and received direct messages, along with an optional next cursor for pagination.
	/// </summary>
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class TwitterDirectMessageListResult : ITwitterModel
	{

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("next_cursor")]
		public string NextCursor { get; set; }

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("events")]
		public IEnumerable<TwitterDirectMessageEvent> Events { get; set; }

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("apps")]
		public Dictionary<string, TwitterApp> Apps { get; set; }

		public string RawSource { get; set; }
	}


#if !SILVERLIGHT && !WINRT
	/// <summary>
	/// Represents an event for a private direct message between two users.
	/// </summary>
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class TwitterApp : ITwitterModel
	{

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("id")]
		public long Id { get; set; }

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("name")]
		public string Name { get; set; }

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("url")]
		public Uri Url { get; set; }

		public string RawSource { get; set; }
	}

}