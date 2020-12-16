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
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Is displayed after application is launched.
/// Loading wheel spins while GETting player info / registering for the first time
/// </summary>
public class SignInWindow : MonoBehaviour
{
	[SerializeField] private Transform _spinner;
	private AppearingWindow _appearingWindow;

	private float _close_time = 0;
	private bool _close = false;

	private void Awake()
	{
		_appearingWindow = GetComponent<AppearingWindow>();
	}

	private void Start()
	{
		if (LocalTest.Testing)
		{
			return;
		}

		_appearingWindow.Open();
		ServerHandler.RegisterOrSignInPlayer(OnResponse_RegisterOrSignInPlayer);
	}

	private void LogScreen(string condition, string stackTrace, LogType type)
	{
		StillMessageManager.Show(condition);
	}

	private void Update()
	{
		_spinner.Rotate(Vector3.forward, 100 * Time.deltaTime);
		if (_close)
		{
			if (ItemImageLoader.FirstRequestSent && ItemImageLoader.AllLoaded())
			{
				_appearingWindow.Close();
				_close = false;
			}

		}
	}

	private void OnResponse_RegisterOrSignInPlayer(string response)
	{

		JObject obj = JObject.Parse(response);

		try
		{
			if ((int) obj["code"] != 200)
			{
				//Try again, probably a connection issue
				PopupMessageManager.Show((string) obj["message"]);
				ServerHandler.RegisterOrSignInPlayer(OnResponse_RegisterOrSignInPlayer);
				return;
			}
		}
		catch (Exception e)
		{
			PopupMessageManager.Show(e.Message);
		}

		TabManager.Get.SwitchTab(2);
		ServerHandler.GetCurrentPlayer(OnResponse_GetPlayer);
	}

	private void OnResponse_Ping(string response)
	{
		Debug.Log(response);
		JObject obj = JObject.Parse(response);
		PopupMessageManager.Show((string) obj["code"]);
	}

	private void OnResponse_GetPlayer(string response)
	{
		JObject obj = JObject.Parse(response);

		HeaderPanel.Get.SetCurrencies((int) obj["data"]["coins"], (int) obj["data"]["gems"], (int) obj["data"]["stars"]);

		//Set level and XP

		int totalXP = (int) obj["data"]["total_xp"];
		int level = 1;
		while (totalXP >= XPBar.XpReqs[level - 1])
		{
			totalXP -= XPBar.XpReqs[level - 1];
			level++;
		}
		XPBar.Get.SetLevelAndXP(level, totalXP);

		//Will close after a while:
		_close = true;
	}
}