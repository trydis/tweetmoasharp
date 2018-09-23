using Hammock.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TweetSharp
{
#if !SILVERLIGHT && !WINRT
	/// <summary>
	/// Represents an event about a direct message.
	/// </summary>
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	public class TwitterDirectMessageEvent : PropertyChangedBase, ITwitterModel, IComparable<TwitterDirectMessageEvent>, IEquatable<TwitterDirectMessageEvent>
	{
		private string _type;
		private long _id;
		private DateTime _createdAt;

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("id")]
		public virtual long Id
		{
			get { return _id; }
			set
			{
				if (_id == value)
				{
					return;
				}

				_id = value;
				OnPropertyChanged("Id");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("type")]
		public virtual string Type
		{
			get { return _type; }
			set
			{
				if (_type == value)
				{
					return;
				}

				_type = value;
				OnPropertyChanged("Type");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("created_at")]
		public virtual DateTime CreatedAt
		{
			get { return _createdAt; }
			set
			{
				if (_createdAt == value)
				{
					return;
				}

				_createdAt = value;
				OnPropertyChanged("CreatedAt");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string RawSource { get; set; }


		#region IComparable<TwitterDirectMessage> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(TwitterDirectMessageEvent other)
		{
			return other.Id == Id ? 0 : other.Id > Id ? -1 : 1;
		}

		#endregion

		#region IEquatable<TwitterDirectMessage> Members

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="obj"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(TwitterDirectMessageEvent obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.Id == Id;
		}

		#endregion

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.GetType() == typeof(TwitterDirectMessageModels) && Equals((TwitterDirectMessageModels)obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TwitterDirectMessageEvent left, TwitterDirectMessageEvent right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TwitterDirectMessageEvent left, TwitterDirectMessageEvent right)
		{
			return !Equals(left, right);
		}

	}

	/// <summary>
	/// Represents information about what started a <see cref="TwitterDirectMessageEvent"/>.
	/// </summary>
	public class TwitterDirectMessageEventInitiatedVia : PropertyChangedBase
	{

		private long _tweetId;
		private long _welcomeMessageId;

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("tweet_id")]
		public virtual long TweetId
		{
			get { return _tweetId; }
			set
			{
				if (_tweetId == value)
				{
					return;
				}

				_tweetId = value;
				OnPropertyChanged("TweetId");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("welcome_message_id")]
		public virtual long WelcomeMessageId
		{
			get { return _welcomeMessageId; }
			set
			{
				if (_welcomeMessageId == value)
				{
					return;
				}

				_welcomeMessageId = value;
				OnPropertyChanged("WelcomeMessageId");
			}
		}

	}

#if !SILVERLIGHT && !WINRT
	/// <summary>
	/// Represents a private direct message between two users.
	/// </summary>
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	public class TwitterDirectMessage : PropertyChangedBase
	{

		private DirectMessageTarget _target;
		private long _senderId;
		private long _sourceAppId;
		private TwitterDirectMessageContent _message;
		private TwitterQuickReplyResponse _quickReplyResponse;
		private TwitterAttachment _attachment;

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual DirectMessageTarget Target
		{
			get { return _target; }
			set
			{
				if (_target == value)
				{
					return;
				}

				_target = value;
				OnPropertyChanged("Target");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("sender_id")]
		public virtual long SenderId
		{
			get { return _senderId; }
			set
			{
				if (_senderId == value)
				{
					return;
				}

				_senderId = value;
				OnPropertyChanged("SenderId");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("source_app_id")]
		public virtual long SourceAppId
		{
			get { return _sourceAppId; }
			set
			{
				if (_sourceAppId == value)
				{
					return;
				}

				_sourceAppId = value;
				OnPropertyChanged("SourceAppId");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("message_data")]
		public virtual TwitterDirectMessageContent Message
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

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("quick_reply_response")]
		public virtual TwitterQuickReplyResponse QuickReplyResponse
		{
			get { return _quickReplyResponse; }
			set
			{
				if (_quickReplyResponse == value)
				{
					return;
				}

				_quickReplyResponse = value;
				OnPropertyChanged("QuickReplyResponse");
			}
		}


#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterAttachment Attachment
		{
			get { return _attachment; }
			set
			{
				if (_attachment == value)
				{
					return;
				}

				_attachment = value;
				OnPropertyChanged("Attachment");
			}
		}
	}

	/// <summary>
	/// Represents the recipient of a direct message.
	/// </summary>
	public class DirectMessageTarget : PropertyChangedBase
	{
		private long _recipientid;

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("recipient_id")]
		public virtual long RecipientId
		{
			get { return _recipientid; }
			set
			{
				if (_recipientid == value)
				{
					return;
				}

				_recipientid = value;
				OnPropertyChanged("Recipientid");
			}
		}
	}

	/// <summary>
	/// Represents the actual text and embedded entities of a direct message.
	/// </summary>
	public class TwitterDirectMessageContent : PropertyChangedBase
	{
		private string _text;
		private TwitterEntities _entities;

#if !Smartphone && !NET20
		[DataMember]
#endif
		[JsonProperty("text")]
		public virtual string Text
		{
			get { return _text; }
			set
			{
				if (_text == value)
				{
					return;
				}

				_text = value;
				OnPropertyChanged("Text");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterEntities Entities
		{
			get { return _entities; }
			set
			{
				if (_entities == value)
				{
					return;
				}

				_entities = value;
				OnPropertyChanged("Entities");
			}
		}
	}

	/// <summary>
	/// Represents a quick (precanned) reply to a direct message.
	/// </summary>
	public class TwitterQuickReplyResponse : PropertyChangedBase
	{
		private string _type = "options";
		private string _metadata;
		private string _label;
		private string _description;

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Type
		{
			get { return _type; }
			set
			{
				if (_type == value)
				{
					return;
				}

				_type = value;
				OnPropertyChanged("Type");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Label
		{
			get { return _label; }
			set
			{
				if (_label == value)
				{
					return;
				}

				_label = value;
				OnPropertyChanged("Label");
			}
		}


#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Description
		{
			get { return _description; }
			set
			{
				if (_description == value)
				{
					return;
				}

				_description = value;
				OnPropertyChanged("Description");
			}
		}


#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Metadata
		{
			get { return _metadata; }
			set
			{
				if (_metadata == value)
				{
					return;
				}

				_metadata = value;
				OnPropertyChanged("Metadata");
			}
		}

	}

	/// <summary>
	/// Represents a piece of media as an attachment.
	/// </summary>
	public class TwitterMediaAttachment : PropertyChangedBase
	{
		private long _mediaId;

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual long Id
		{
			get { return _mediaId; }
			set
			{
				if (_mediaId == value)
				{
					return;
				}

				_mediaId = value;
				OnPropertyChanged("Id");
			}
		}

	}

	/// <summary>
	/// Represents a quick (precanned) reply to a direct message.
	/// </summary>
	public class TwitterAttachment : PropertyChangedBase
	{
		private string _type = "media";
		private TwitterMediaAttachment _media;

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Type
		{
			get { return _type; }
			set
			{
				if (_type == value)
				{
					return;
				}

				_type = value;
				OnPropertyChanged("Type");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterMediaAttachment Media
		{
			get { return _media; }
			set
			{
				if (_media == value)
				{
					return;
				}

				_media = value;
				OnPropertyChanged("Media");
			}
		}

	}

}