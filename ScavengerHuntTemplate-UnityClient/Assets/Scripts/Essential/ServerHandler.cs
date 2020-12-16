/*
Copyright 2020 SOFIE. All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class for communicating with the game server.
/// </summary>
public class ServerHandler : MonoBehaviour
{
	private static string _host_address = "YOUR URL HERE WITH A SLASH / IN THE END";
	private static ServerHandler _instance;

	/// <summary>
	/// List of put and post functions that are awaiting for a response. A new put request that is already waiting for a response will not send the request.
	/// </summary>
	/// <typeparam name="string"></typeparam>
	/// <returns></returns>
	private static List<string> _waitingRequests = new List<string>();

	private void Awake()
	{
		_instance = this;
	}

	#region Public static interface

	/// <summary>
	/// Set _server_uri to https://postman-echo.com/ and test get requests with this
	/// </summary>
	public static void GetTest(int test, Action<string> onComplete)
	{
		string uri = "get?testParam=" + test.ToString();
		_instance.StartCoroutine(GetRequest(uri, onComplete));
		Debug.Log("Sent request");
	}

	public static void Ping(Action<string> onResponse)
	{
		string uri = "";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	/// <summary>
	/// Call this every once in awhile when in Nearby tab, while not viewing a hunt info window.
	/// </summary>
	/// <param name="latitude"></param>
	/// <param name="longitude"></param>
	/// <param name="radiusKM"></param>
	/// <param name="onComplete"></param>
	public static void GetNearbyHunts(float latitude, float longitude, float radiusKM, Action<string> onResponse)
	{
		string uri = Player.ID + "/hunt/nearby?latitude=" + latitude.ToString() + "&longitude=" + longitude.ToString() + "&radius=" + radiusKM.ToString();
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetPlayer(string playerID, Action<string> onResponse)
	{
		string uri = "player/" + playerID;
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetCurrentPlayer(Action<string> onResponse)
	{
		string uri = "player/" + Player.ID;
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetStartedHunts(Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/startedHunts";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetCompletedHunts(Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/completedHunts";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetHunt(string huntID, Action<string> onResponse)
	{
		string uri = "hunt/" + huntID;
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetClue(string huntID, int stepNum, Action<string> onResponse)
	{
		string uri = "hunt/" + huntID + "/clues/" + stepNum.ToString();
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void GetHint(string huntID, int stepNum, Action<string> onResponse)
	{
		string uri = "hunt/" + huntID + "/clues/" + stepNum.ToString() + "/hint";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	/// <summary>
	/// Call this every time a new beacon gets in range.
	/// </summary>
	/// <param name="huntID"></param>
	/// <param name="stepNum"></param>
	/// <param name="beaconID"></param>
	/// <param name="task"></param>
	/// <param name="onResponse"></param>
	public static void GetTask(string huntID, int stepNum, string instanceID, Action<string> onResponse)
	{
		//raw string instanceID is of form "0x000000000001"
		string cut_insance = instanceID.Substring(2);
		string uri = "hunt/" + huntID + "/task/" + stepNum.ToString() + "?instanceID=" + cut_insance;
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void HasUsedStar(string huntID, Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/" + huntID + "/star";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void CheatCurrencies(Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/addreward";
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void CheckTaskAnswerAndAdvance(string huntID, int taskNum, string playerAnswer, Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/" + huntID + "/" + taskNum + "/answer";
		JObject obj = new JObject();
		obj["answer"] = playerAnswer;
		string data = JsonConvert.SerializeObject(obj);
		_instance.StartCoroutine(PutRequest(uri, data, onResponse, "CheckTaskAnswerAndAdvance"));
	}

	/// <summary>
	/// Sends hunt rating to server.
	/// Not checking for User, because each user gets only 1 chance to rate.
	/// </summary>
	public static void PutHuntRating(string huntID, int rating, Action<string> onResponse)
	{
		string uri = "hunt/" + huntID + "/rating/put";
		string data = "{ user_rating: " + rating.ToString() + " }";
		_instance.StartCoroutine(PutRequest(uri, data, onResponse, "PutHuntRating"));
	}

	public static void StartHunt(string huntID, Action<string> onResponse)
	{
		string uri = Player.ID + "/" + huntID + "/start";
		string data = "{}";
		_instance.StartCoroutine(PutRequest(uri, data, onResponse, "StartHunt"));
	}

	public static void UseStar(string huntID, Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/" + huntID + "/star";
		Debug.Log(uri);
		string data = "{}";
		_instance.StartCoroutine(PutRequest(uri, data, onResponse, "UseStar"));
	}

	public static void UseHint(string huntID, int stepNum, Action<string> onResponse)
	{
		string uri = "player/" + Player.ID + "/" + huntID + "/clues/" + stepNum.ToString() + "/hint";
		string data = "{}";
		_instance.StartCoroutine(PutRequest(uri, data, onResponse, "UseHint"));
	}

	public static void RegisterOrSignInPlayer(Action<string> onResponse)
	{
		string uri = "players";
		JObject obj = new JObject();
		obj["androidId"] = Player.ID;
		string data = JsonConvert.SerializeObject(obj);
		_instance.StartCoroutine(PostRequest(uri, data, onResponse, "RegisterPlayer"));
	}

	#endregion

	#region Private instance request processing

	private static IEnumerator GetRequest(string uri, Action<string> onComplete)
	{
		UnityWebRequest request = UnityWebRequest.Get(_host_address + uri);
		yield return request.SendWebRequest();

		if (request.isNetworkError || request.isHttpError)
		{
			Debug.Log("Get request error: " + uri + "\n" + request.error);
		}
		else
		{
			onComplete.Invoke(request.downloadHandler.text);
		}
	}

	private static IEnumerator PutRequest(string uri, string data, Action<string> onComplete, string functionName)
	{
		//Do nothing if waiting for response from this function
		if (_waitingRequests.Contains(functionName))
		{
			Debug.Log("Already waiting for a response from this put request function: " + functionName);
			yield break;
		}

		//Else, proceed normally
		_waitingRequests.Add(functionName);

		UnityWebRequest request = new UnityWebRequest(_host_address + uri, "PUT");
		byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
		request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();

		_waitingRequests.Remove(functionName);

		//onComplete should handle all responses
		onComplete.Invoke(request.downloadHandler.text);
	}

	private static IEnumerator PostRequest(string uri, string data, Action<string> onComplete, string functionName)
	{
		//Do nothing if waiting for response from this function
		if (_waitingRequests.Contains(functionName))
		{
			Debug.Log("Already waiting for a response from this post request function: " + functionName);
			yield break;
		}

		//Else, proceed normally
		_waitingRequests.Add(functionName);

		UnityWebRequest request = new UnityWebRequest(_host_address + uri, "POST");
		byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
		request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();

		_waitingRequests.Remove(functionName);
		//onComplete should handle all responses
		onComplete.Invoke(request.downloadHandler.text);

	}

	#endregion

}