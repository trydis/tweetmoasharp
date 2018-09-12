using System;
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
	public class TwitterCreateDirectMessageEvent : TwitterDirectMessageEvent
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
}