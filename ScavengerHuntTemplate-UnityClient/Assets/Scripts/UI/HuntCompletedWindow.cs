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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This appears when the player completes the last task in a hunt.
/// Displays rewards gotten from the hunt.
/// </summary>
public class HuntCompletedWindow : MonoBehaviour
{
	public int CoinsReward { get; private set; }
	public int GemsReward { get; private set; }
	public int StarsReward { get; private set; }
	public int XPReward { get; private set; }

	[SerializeField] private Text _huntNameText;
	[SerializeField] private Text _coinsText;
	[SerializeField] private Text _gemsText;
	[SerializeField] private Text _starsText;
	[SerializeField] private Text _xpText;
	[SerializeField] private Text _messageText;
	[SerializeField] private List<ItemBox> _itemBoxes;

	private string _huntID;

	public int Rating = 0;

	private AppearingWindow _appearingWindow;

	private void Awake()
	{
		_appearingWindow = GetComponent<AppearingWindow>();
	}

	// Start is called before the first frame update
	void Start()
	{
		_appearingWindow.Open();
	}

	public void Close()
	{
		if (Rating > 0)
		{
			if (!LocalTest.Testing)
			{
				ServerHandler.PutHuntRating(_huntID, Rating, (string x) => { });
			}
		}
		_appearingWindow.Close();
	}

	public void SetHuntRewards(HuntInfo huntInfo, string huntName, int numCoins, int numGems, int numStars, int xp, string message)
	{
		_huntID = huntInfo.ID;

		CoinsReward = numCoins;
		GemsReward = numGems;
		StarsReward = numStars;
		XPReward = xp;

		_huntNameText.text = "You completed " + huntName + "!";

		_coinsText.text = numCoins.ToString();
		_gemsText.text = numGems.ToString();
		_starsText.text = numStars.ToString();
		_xpText.text = xp.ToString() + " XP";
		_messageText.text = message;

		foreach (var box in _itemBoxes)
		{
			box.Hide();
		}
		//Assuming 4 item rewards maximum
		for (int i = 0; i < huntInfo.RewardItems.Count; i++)
		{
			_itemBoxes[i].Show();
			_itemBoxes[i].SetItem(huntInfo.RewardItems[i]);
		}
	}
}