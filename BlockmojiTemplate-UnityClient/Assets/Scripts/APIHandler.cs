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
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class for communicating with the blockchain through server.
/// </summary>
public class APIHandler : MonoBehaviour
{
	private static string _host_address = "YOUR URL HERE WITH A SLASH / IN THE END";

	private static string CharacterID;
	private static APIHandler _instance;

	private void Awake()
	{
		_instance = this;
		CharacterID = SystemInfo.deviceUniqueIdentifier;
	}

	#region Public functions
	public static void GetCharacter(Action<string> onResponse)
	{
		string uri = "useritems/" + CharacterID;
		_instance.StartCoroutine(GetRequest(uri, onResponse));
	}

	public static void UpdateEquipped(ReadOnlyCollection<Item> equippedItems, Action<string> onResponse)
	{
		string uri = "updateEquip/";
		JObject jobj = new JObject();
		jobj["username"] = CharacterID;
		JArray jarr = new JArray();
		foreach (var item in equippedItems)
		{
			jarr.Add(item.UniqueID);
		}
		jobj["EquippedItems"] = jarr;
		string data = JsonConvert.SerializeObject(jobj);
		_instance.StartCoroutine(PostRequest(uri, data, onResponse));
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

	private static IEnumerator PostRequest(string uri, string data, Action<string> onComplete)
	{
		UnityWebRequest request = new UnityWebRequest(_host_address + uri, "POST");
		byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
		request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();

		//onComplete should handle all responses
		onComplete.Invoke(request.downloadHandler.text);
	}

	#endregion
}