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
using UnityEngine.UI;

/// <summary>
/// Shows all info of a Hunt visually in a big window.
/// Opened from Nearby Hunts view
/// /// </summary>
public class HuntInfoWindow : MonoBehaviour
{
	public HuntInfo HuntInfo { get; private set; }

	[SerializeField] private Text _nameText;
	[SerializeField] private Text _descriptionText;
	[SerializeField] private Text _rewardText;
	[SerializeField] private List<ItemBox> _itemBoxes;
	private AppearingWindow _appearingWindow;
	private DifficultyIndicator _difficulty;
	private AverageUserRating _rating;

	public static HuntInfoWindow Get { get; private set; }

	private void Awake()
	{
		Get = this;
		_appearingWindow = GetComponent<AppearingWindow>();
		_difficulty = GetComponentInChildren<DifficultyIndicator>();
		_rating = GetComponentInChildren<AverageUserRating>();
	}

	public void Show(HuntInfo info)
	{
		HuntInfo = info;
		_nameText.text = info.Name;
		_descriptionText.text = info.Description;
		_rating.ShowRating(info.UserRating);
		_difficulty.ShowDifficulty(info.OfficialDifficulty);
		_rewardText.text = info.RewardCoins.ToString() + " Coins";
		_appearingWindow.Open();
		foreach (var box in _itemBoxes)
		{
			box.Hide();
		}
		//Assuming 4 item rewards maximum
		for (int i = 0; i < info.RewardItems.Count; i++)
		{
			_itemBoxes[i].Show();
			_itemBoxes[i].SetItem(info.RewardItems[i]);
		}
	}

	public void StartHunt()
	{
		if (!LocalTest.Testing)
		{
			Debug.Log(HuntInfo.Name);
			ServerHandler.StartHunt(HuntInfo.ID, OnStartHuntComplete);
		}
		else
		{
			OnStartHuntComplete("localTest");
		}
	}

	public void OnStartHuntComplete(string response)
	{
		if (response != "localTest")
		{
			JObject obj = JObject.Parse(response);

			if ((int) obj["code"] != 200)
			{
				return;
			}
		}

		_appearingWindow.Close();
		TabManager.Get.SwitchTab(Tab.StartedHunts);
		StartedHunt startedhunt = StartedHuntsManager.Get.AddStartedHunt(HuntInfo);
	}

	private string TimeSpanToString(System.TimeSpan span)
	{
		if (span.TotalDays >= 1)
		{
			return Mathf.Floor((float) span.TotalDays).ToString() + " days left.";
		}
		else if (span.TotalHours <= 1)
		{
			return "Less than 1 hour to start the hunt!";
		}
		else
		{
			return Mathf.Floor((float) span.TotalHours).ToString() + " hours left.";
		}
	}
}