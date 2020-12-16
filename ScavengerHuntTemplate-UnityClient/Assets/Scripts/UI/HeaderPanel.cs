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
/// The UI panel that is always visible at the top of the screen.
/// Always shows the player's level, XP, and in-game tokens (coins, gems, stars)
/// </summary>
public class HeaderPanel : MonoBehaviour
{

	public int Coins { get { return int.Parse(_coinsText.text); } }
	public int Gems { get { return int.Parse(_gemsText.text); } }
	public int Stars { get { return int.Parse(_starsText.text); } }

	[SerializeField] private Text _coinsText;
	[SerializeField] private Text _gemsText;
	[SerializeField] private Text _starsText;
	public static HeaderPanel Get { get; private set; }

	private void Awake()
	{
		Get = this;
	}

	public void SetCurrencies(int coins, int gems, int stars)
	{
		_coinsText.text = coins.ToString();
		_gemsText.text = gems.ToString();
		_starsText.text = stars.ToString();
	}

	public void IncreaseCurrencies(int coins, int gems, int stars)
	{
		_coinsText.text = (Coins + coins).ToString();
		_gemsText.text = (Gems + gems).ToString();
		_starsText.text = (Stars + stars).ToString();
	}

	private void Update()
	{
		if (Input.GetButtonDown("GiveCurrencies"))
		{
			ServerHandler.CheatCurrencies(OnResponseCheat);
		}
	}

	private void OnResponseCheat(string response)
	{
		ServerHandler.GetCurrentPlayer(OnResponse_GetPlayer);
	}

	private void OnResponse_GetPlayer(string response)
	{
		JObject obj = JObject.Parse(response);

		HeaderPanel.Get.SetCurrencies((int) obj["data"]["coins"], (int) obj["data"]["gems"], (int) obj["data"]["stars"]);
	}

}