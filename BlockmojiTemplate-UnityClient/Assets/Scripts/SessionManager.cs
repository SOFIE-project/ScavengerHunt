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

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(LoadCharacter(false));
	}

	private IEnumerator LoadCharacter(bool test)
	{
		JObject jcharacter;

		if (!test)
		{
			string response = null;
			APIHandler.GetCharacter(r => response = r);

			while (response == null)
			{
				yield return null;
			}
			Debug.Log("Got character response");

			JArray jresp = JArray.Parse(response);
			jcharacter = (JObject) jresp[0];
		}
		else
		{
			Debug.Log("Using mock character json for testing.");
			string mockCharacter = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/mockCharacter.json");
			jcharacter = JObject.Parse(mockCharacter);
		}

		//Create all owned items
		JArray jitems = (JArray) jcharacter["ownedItems"];
		foreach (JObject jitem in jitems)
		{
			Item.FromJObject(jitem);
		}

		//Item image loading now started (called from each Item's constructor)
		Debug.Log("Loading all item images...");
		while (!ItemImageLoader.AllLoaded())
		{
			yield return null;
		}
		Debug.Log("All item images loaded.");
		Inventory.Update();

		//Based on response jresp, equip correct items
		try
		{
			JArray jequippedItems = (JArray) jcharacter["equippedItems"];
			foreach (var jitemID in jequippedItems)
			{
				Equipment.Equip(Item.Get((string) jitemID), updateBackend : false);
			}
		}
		catch (System.InvalidCastException)
		{
			Debug.Log("equippedItems was null and not an empty list, which means no items were equipped.");
		}

	}
}