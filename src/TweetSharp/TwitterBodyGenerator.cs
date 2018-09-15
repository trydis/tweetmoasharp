using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSharp
{
	public abstract class TwitterBodyGenerator<T>
	{
		public abstract string ContentType { get; }

		public abstract byte[] GenerateBody(T args);
	}
}