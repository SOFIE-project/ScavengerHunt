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
/// Fetch and display hunts that can be started based on current GPS location
/// </summary>
public class NearbyManager : MonoBehaviour
{
	[SerializeField] private HuntInfoButton _buttonTemplate;
	[SerializeField] private GameObject _noNearbyHuntsText;

	private float _time;
	private float _refreshInterval = 5f;
	private bool _waitingForResponse = false;
	private List<HuntInfo> _nearbyHunts = new List<HuntInfo>();

	private ScrollablePool<HuntInfoButton> _buttonsPool;

	private void Awake()
	{
		_time = _refreshInterval;
		_buttonsPool = new ScrollablePool<HuntInfoButton>(_buttonTemplate);
	}

	private void Start()
	{
		//So that dont see lots of template buttons before retrieved hunts
		UpdateInfoButtons();
	}

	private void OnEnable()
	{
		//On switched tab to here
		StartRefreshingHunts();
	}

	private void Update()
	{
		_time += Time.time;
		if (_time >= _refreshInterval && !_waitingForResponse)
		{
			StartRefreshingHunts();
			_time = 0;
		}
	}

	private void StartRefreshingHunts()
	{
		if (!LocalTest.Testing)
		{
			float latitude;
			float longitude;

			if (Application.isEditor)
			{
				latitude = 60.1767436f;
				longitude = 24.8337087f;
			}
			else
			{
				if (Input.location.status != LocationServiceStatus.Running || Input.location.status != LocationServiceStatus.Initializing)
				{
					//Permission was asked before first Start().
					//This will make sure location works on first play session
					Input.location.Start();
				}
				LocationInfo location = Input.location.lastData;
				latitude = location.latitude;
				longitude = location.longitude;
			}

			ServerHandler.GetNearbyHunts(latitude, longitude, 12f, OnReceivedNearbyHunts);

			_waitingForResponse = true;
		}
		else
		{
			_nearbyHunts.AddRange(HuntInfo.DummyHunts);
			if (StartedHuntsManager.Get != null)
			{
				_nearbyHunts.RemoveAll(x => StartedHuntsManager.Get.HasStartedHunt(x.ID));
			}

			if (CompletedHuntsManager.Get != null)
			{
				_nearbyHunts.RemoveAll(x => CompletedHuntsManager.Get.HasCompletedHunt(x.ID));
			}

			UpdateInfoButtons();
		}
	}

	private void OnReceivedNearbyHunts(string response)
	{
		_waitingForResponse = false;

		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}

		_nearbyHunts = HuntInfosFromJContainer((JContainer) obj["data"]);

		//Hunts are now in _nearbyHunts
		UpdateInfoButtons();
	}

	private List<HuntInfo> HuntInfosFromJContainer(JContainer jContainer)
	{
		var list = new List<HuntInfo>();
		foreach (var hunt in jContainer)
		{
			HuntInfo info = HuntInfo.FromJObject((JObject) hunt);

			list.Add(info);
		}

		return list;
	}

	private void UpdateInfoButtons()
	{
		//Update infos
		_buttonsPool.IncreaseUntil(_nearbyHunts.Count);
		for (int i = 0; i < _buttonsPool.Count; i++)
		{
			var button = _buttonsPool[i];
			if (i < _nearbyHunts.Count)
			{
				button.gameObject.SetActive(true);
				button.SetInfo(_nearbyHunts[i]);
			}
			else
			{
				button.gameObject.SetActive(false);
			}
		}
		if (_nearbyHunts.Count == 0)
		{
			_noNearbyHuntsText.SetActive(true);
		}
		else
		{
			_noNearbyHuntsText.SetActive(false);
		}
	}

}