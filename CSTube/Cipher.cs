using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSTube
{
	/// <summary>
	/// This module countains all logic necessary to decipher the signature.
	/// 
	/// YouTube's strategy to restrict downloading videos is to send a ciphered version
	/// of the signature to the client, along with the decryption algorithm obfuscated in JavaScript. 
	/// For the clients to play the videos, JavaScript must take the ciphered version, 
	/// cycle it through a series of "transform functions", and then signs the media URL with the output.
	/// 
	/// This class is responsible for 
	/// (1) finding and extracting those "transform functions" 
	/// (2) mapping them to C# equivalents and
	/// (3) taking the ciphered signature and decoding it.
	/// </summary>
	internal static class Cipher
	{
		private static Regex
			extractInitialFunctionName = new Regex(@".set ?\( ?""signature"", ?([a-zA-Z0-9$]+) ?\(", RegexOptions.Compiled),
			extractFunctionData = new Regex(@"\w+\.(\w+)\(\w,(\d+)\)", RegexOptions.Compiled);


		private delegate char[] TransformFunc(char[] charArray, int param);

		private static Dictionary<int, Func<string, string>> signatorCache = new Dictionary<int, Func<string, string>>();

		/// <summary>
		/// Decodes the specified ciphered signature.
		/// Analyzes the specified javascript code to get all transformations applied to the signature in code.
		/// The created signator function is cached with the hash of the javascript code for reuse.
		/// </summary>
		public static string DecodeSignature(string js, string cipheredSignature)
		{
			// Try to get cached signator
			int jsHash = js.GetHashCode();
			if (!signatorCache.ContainsKey(jsHash))
			{ // Build signator for the specified js script

				// Sequence of raw javascript transform functions to apply on signature
				string[] tPlan = getTransformPlan(js);

				// Name of object holding the transform functions
				string funcVar = tPlan[0].Split('.')[0];
				// Map function identifier to C# functions
				Dictionary<string, TransformFunc> tMap = getTransformMap(js, funcVar);

				// Compile actual command stack out of plan and map
				List<Tuple<TransformFunc, int>> tStack =
					tPlan.Select(func =>
					{ // Separate func into name and parameter and parse them into a command
						Tuple<string, string> funcData = parseFunction(func);
						return new Tuple<TransformFunc, int>(tMap[funcData.Item1], int.Parse(funcData.Item2));
					}).ToList();

				// Create reusable signator function executing the command stack on the signature
				Func<string, string> signator = (string signatur) =>
				{ // Execute command stack
					char[] charArray = signatur.ToArray();
					foreach (Tuple<TransformFunc, int> cmd in tStack)
						charArray = cmd.Item1.Invoke(charArray, cmd.Item2);
					return new string(charArray);
				};

				// Enter and cache signator
				signatorCache.Add(jsHash, signator);
			}

			// Apply signator function
			return signatorCache[jsHash].Invoke(cipheredSignature);
		}

		/// <summary>
		/// Parses the specified javascript function call and breaks it down into function name and parameter
		/// Format: XX.{name}(X,{num})
		/// Example: XJ.rg(a,2) --> ['rg', '2']
		/// </summary>
		private static Tuple<string, string> parseFunction(string jsFunc)
		{
			return Helpers.DoRegex(extractFunctionData, jsFunc, 1, 2);
		}

		/// <summary>
		/// Extracts the raw transform plan as a list of havascript functions from the specified code.
		/// </summary>
		private static string[] getTransformPlan(string js)
		{
			string name = Regex.Escape(getInitialFunctionName(js));
			string pattern = name + @"=function\([a-z]\)\{[a-z]=[a-z]\.split\(""""\);(.*?);return [a-z].join\(""""\)\};";
			string[] plan = Helpers.DoRegex(pattern, js, 1).Split(';');
			CSTube.Log("Transform Plan " + name + ":" + string.Join(" || ", plan));
			return plan;
		}

		/// <summary>
		/// Extract the name of the function responsible for computing the signature.
		/// Usually: c&&d.set("signature", EE(c));
		/// </summary>
		private static string getInitialFunctionName(string js)
		{
			return Helpers.DoRegex(extractInitialFunctionName, js, 1);
		}


		/// <summary>
		/// Build a lookup table for the functions defined in the javascript object with the specified name.
		/// Key is the identifier as specified in the variable.
		/// Value is the javascript function mapped to a proper C# function.
		/// Usually contains three values only for reverse, splice and swap.
		/// </summary>
		private static Dictionary<string, TransformFunc> getTransformMap(string js, string funcVar)
		{
			string[] transformObject = getTransformObject(js, funcVar);
			Dictionary<string, TransformFunc> mapper = new Dictionary<string, TransformFunc>();
			foreach (string obj in transformObject)
			{ // AJ:function(a){a.reverse()} => AJ, function(a){a.reverse()}
				string[] objSplit = obj.Split(new char[] { ':' }, 2);
				mapper.Add(objSplit[0], mapFunctions(objSplit[1]));
			}
			return mapper;
		}

		/// <summary>
		/// Extracts the transform object from the specified funcVar in the specified javascript code.
		/// Returns a list of function definitions inside the object.
		/// Format: {ID}:{FunctionDef}
		/// </summary>
		private static string[] getTransformObject(string js, string funcVar)
		{
			string pattern = "var " + Regex.Escape(funcVar) + @"=\{(.*?)\};";
			string[] obj = Helpers.DoRegex(pattern, js, 1, RegexOptions.Singleline)
				.Replace("\n", " ")
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim())
				.ToArray();
			CSTube.Log("Transform Object " + funcVar + ": " + string.Join(" || ", obj));
			return obj;
		}

		/// <summary>
		/// For a given JavaScript transform function, return the C# equivalent.
		/// </summary>
		private static TransformFunc mapFunctions(string jsFunc)
		{
			// Check for simplifications only
			if (jsFunc.Contains("reverse"))
				return reverse;
			if (jsFunc.Contains("splice"))
				return splice;
			if (jsFunc.Contains("%") && jsFunc.Contains(".length];"))
				return swap;
			throw new Exception("Could not find C# equivalent function for: " + jsFunc);
		}

		/// <summary>
		/// Reverse elements in a list.
		/// </summary>
		private static char[] reverse(char[] arr, int b)
		{
			return arr.Reverse().ToArray();
		}

		/// <summary>
		/// Variation of splice for positive values of b.
		/// Removes the first b elements and returns the rest.
		/// </summary>
		private static char[] splice(char[] arr, int b)
		{
			char[] result = new char[arr.Length-b];
			Array.Copy(arr, b, result, 0, arr.Length - b);
			return result;
		}

		/// <summary>
		/// Swap values [0] and [b % length].
		/// </summary>
		private static char[] swap(char[] arr, int b)
		{
			b = b % arr.Length;
			char c = arr[0];
			arr[0] = arr[b];
			arr[b] = c;
			return arr;
		}
	}
}