using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TweetSharp
{
	public class SendDirectMessageBodyGenerator : TwitterBodyGenerator<SendDirectMessageOptions>
	{
		public override string ContentType { get { return "application/json"; } }

		public override byte[] GenerateBody(SendDirectMessageOptions args)
		{
			using (var tw = new System.IO.StringWriter())
			using (var writer = new Newtonsoft.Json.JsonTextWriter(tw))
			{
				writer.WriteStartObject();

				writer.WritePropertyName("event");
				writer.WriteStartObject();

				writer.WritePropertyName("type");
				writer.WriteValue("message_create");

				writer.WritePropertyName("message_create");
				writer.WriteStartObject();

				writer.WritePropertyName("target");
				writer.WriteStartObject();
				writer.WritePropertyName("recipient_id");
				writer.WriteValue(args.Recipientid);
				writer.WriteEndObject();

				writer.WritePropertyName("message_data");
				writer.WriteStartObject();
				writer.WritePropertyName("text");
				writer.WriteValue(args.Text);

				if (args.Mediaid != null)
				{
					writer.WritePropertyName("attachment");
					writer.WriteStartObject();

					writer.WritePropertyName("type");
					writer.WriteValue(args.Mediatype);

					writer.WritePropertyName("media");
					writer.WriteStartObject();
					writer.WritePropertyName("id");
					writer.WriteValue(args.Mediaid);
					writer.WriteEndObject();

					writer.WriteEndObject();
				}

				if (args.Quickreplies?.Any() ?? false)
				{
					writer.WritePropertyName("quick_reply");
					writer.WriteStartObject();

					writer.WritePropertyName("type");
					writer.WriteValue("options");

					writer.WritePropertyName("options");
					writer.WriteStartArray();

					foreach (var qr in args.Quickreplies)
					{
						writer.WriteStartObject();

						writer.WritePropertyName("label");
						writer.WriteValue(qr.Label);
						writer.WritePropertyName("description");
						writer.WriteValue(qr.Description);
						writer.WritePropertyName("metdata");
						writer.WriteValue(qr.Metadata);

						writer.WriteEndObject();
					}

					writer.WriteEndArray();

					writer.WriteEndObject();
				}

				writer.WriteEndObject();

				writer.WriteEndObject();

				writer.WriteEndObject();

				writer.WriteEndObject();

				return System.Text.UTF8Encoding.UTF8.GetBytes(tw.ToString());
			}
		}
	}
}