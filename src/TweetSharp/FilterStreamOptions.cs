using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSharp
{
#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
	public class FilterStreamOptions 
	{
		public bool Delimited { get; set; }
		public bool StallWarnings { get; set; }
		public StreamFilterLevel FilterLevel { get; set; }
		public string Language { get; set; }
		public IEnumerable<long> Follow { get; set; }
		public IEnumerable<string> Track { get; set; }
		public TwitterGeoLocation.GeoCoordinates[] Locations { get; set; }
		public StreamWith With { get; set; }
		public bool StringifyFriendIds { get; set; }
		public bool Replies { get; set; }
	}

	public enum StreamFilterLevel
	{
		None,
		Low,
		Medium
	}

	public enum StreamWith
	{
		User,
		Followings
	}
}