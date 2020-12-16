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
using UnityEngine;

/// <summary>
/// Runtime representation of a Hunt
/// </summary>
public class HuntInfo
{
	public string ID { get; private set; }
	public string Name { get; private set; }
	public string Description { get; private set; }
	public int OfficialDifficulty { get; private set; }
	public float UserRating { get; private set; }
	public int RewardCoins { get; private set; }
	public List<Item> RewardItems { get; private set; }
	public int NumClues { get; private set; }

	//Mock hunts if LocalTest is True:

	public static readonly List<HuntInfo> DummyHunts = new List<HuntInfo>()
	{
		new HuntInfo("200", "Office Hunt", "Get introduced to the game and get your first rewards!", 3, 3.8f, 5000, 5),

			new HuntInfo("201", "Advanced Hunt", "This one will make you think hard.", 4, 4.1f, 6000, 4),

			new HuntInfo("202", "Theft Mystery", "Something valuable was stolen. By following the clues can you solve what was stolen from the building?", 5, 4.6f, 11500, 5),

			new HuntInfo("203", "Big Trouble in A Little Town", "This hunt will take you all around Keilaniemi.", 5, 4.6f, 11500, 7),

			new HuntInfo("204", "Another Hunt", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("205", "And Another One", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("206", "Can You Find Jimmy?", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("207", "The Explorer", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("208", "There and Back Again", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("209", "Can You Find Jimmy, Part II", "This is a dummy hunt.", 5, 3.6f, 5000, 4),

			new HuntInfo("210", "The Gambit", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("211", "Mystery Road", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("212", "Old Time Road", "This is a dummy hunt.", 5, 2.6f, 5000, 4),

			new HuntInfo("213", "About to Expire Hunt", "Expires in 20 sec after started game.", 5, 2.6f, 5000, 4)

	};

	public HuntInfo(string id, string name, string description, int officialDifficulty, float userRating, int rewardCoins, int numClues, List<Item> rewardItems = null)
	{
		ID = id;
		Name = name;
		Description = description;
		OfficialDifficulty = officialDifficulty;
		UserRating = userRating;
		RewardCoins = rewardCoins;
		NumClues = numClues;
		if (rewardItems != null)
		{
			RewardItems = rewardItems;
		}
		else
		{
			RewardItems = new List<Item>();
		}
	}

	public static HuntInfo FromJObject(JObject obj)
	{
		List<Item> items = new List<Item>();
		JArray jitems = (JArray) obj["assetRewards"];
		foreach (JObject jitem in jitems)
		{
			items.Add(Item.FromJObjectOrExisting(jitem));
		}

		//Item image loading now started (called from each Item's constructor)
		return new HuntInfo((string) obj["huntID"], (string) obj["name"], (string) obj["description"], (int) obj["difficulty"], (float) obj["user_rating"], (int) obj["fixedcoinreward"], ((JContainer) obj["clues"]).Count, items);
	}

}