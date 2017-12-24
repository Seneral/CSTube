using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace CSTube
{
	/// <summary>
	/// Applies in-place data mutations.
	/// Wrapper for descrambling Stream data into container and for signing stream URLs.
	/// </summary>
	internal static class Mixins
	{
		/// <summary>
		/// Apply the decrypted signature to the stream manifest.
		/// </summary>
		public static void ApplySignature(ObscuredContainer streamContainer, string js)
		{
			int numSignaturesFound = 0;
			foreach (KeyValuePair<string, object> s in streamContainer)
			{ // Iterate over each stream and sign the URLs
				ObscuredContainer stream = (ObscuredContainer)s.Value;
				string URL = stream.GetValue<string>("url");
				if (URL.Contains("signature="))
				{ // Sometimes, signature is provided directly by YT, so we can skip the whole signature descrambling
					numSignaturesFound++;
					continue;
				}

				// Get, decode and save signature
				string cipheredSignature = stream.GetValue<string>("s");
				string signature = Cipher.DecodeSignature(js, cipheredSignature);
				stream["url"] = URL + "&signature=" + signature;

				/*YouTube.Log(string.Format (
					"Descrambled Signature for ITag={0} \n \t s={1} \n \t signature={2}",
					stream.GetValue<string>("itag"),
					cipheredSignature,
					signature));*/
			}

			if (numSignaturesFound > 0)
				CSTube.Log(string.Format ("{0} out of {1} URLs already contained a signature!", numSignaturesFound, streamContainer.Count));
		}

		/// <summary>
		/// Descrambles data of all streams at the given args position and breaks them down into individual parameters.
		/// Result is written into back into the given position as a parsed ObscuredContainer and returned
		/// </summary>
		public static ObscuredContainer ApplyDescrambler(ObscuredContainer args, string streamKey)
		{
			// Break up individual streams
			string[] streamBundle = args.GetValue<string>(streamKey).Split(',');
			ObscuredContainer streamContainer = new ObscuredContainer();
			foreach (string streamRaw in streamBundle)
			{ // Descramble and index stream data
				NameValueCollection streamParsed = HttpUtility.ParseQueryString(streamRaw);
				ObscuredContainer streamInfo = ObscuredContainer.FromDictionary(
					streamParsed.AllKeys.ToDictionary(k => k, k => (object)HttpUtility.UrlDecode(streamParsed[k]))
				);
				streamContainer.Add(streamRaw, streamInfo);
			}
			// Write descrambled data back into args
			args[streamKey] = streamContainer;

			/*YouTube.Log(string.Format (
				"Applying Descrambler: \n \t " +
					string.Join(" \n \t ", streamContainer.Select(s =>
							string.Join("; ", ((ObscuredContainer)s.Value).Select(p => (string)p.Key + "=" + (string)p.Value))
					))
			));*/

			return streamContainer;
		}
	}
}