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
/// Used for recording and browsing completed hunts
/// </summary>
public class CompletedHuntsManager : MonoBehaviour
{
	[SerializeField] private CompletedHuntBox _boxTemplate;
	[SerializeField] private GameObject _noCompletedHuntsText;
	private List<HuntInfo> _completedHunts = new List<HuntInfo>();
	private ScrollablePool<CompletedHuntBox> _boxesPool;
	public static CompletedHuntsManager Get { get; private set; }
	private float _time = 0;

	public void Initialize()
	{
		Get = this;
		_boxesPool = new ScrollablePool<CompletedHuntBox>(_boxTemplate);
	}

	private void Start()
	{
		//So that dont see lots of template buttons before retrieved hunts
		UpdateView();
		UpdateHunts();
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > 3)
		{
			UpdateHunts();
			_time = 0;
		}
	}

	private void UpdateHunts()
	{
		if (LocalTest.Testing)
		{
			UpdateView();
		}
		else
		{
			ServerHandler.GetCompletedHunts(OnReceived_CompletedHunts);
		}
	}

	private void OnReceived_CompletedHunts(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			UpdateView();
			return;
		}

		_completedHunts.Clear();
		foreach (JObject completedHunt in obj["data"])
		{
			HuntInfo info = HuntInfo.FromJObject((JObject) completedHunt["hunt_info"]);

			_completedHunts.Add(info);
		}

		UpdateView();
	}

	private void UpdateView()
	{
		//Update boxes
		_boxesPool.IncreaseUntil(_completedHunts.Count);
		for (int i = 0; i < _boxesPool.Count; i++)
		{
			var box = _boxesPool[i];
			if (i < _completedHunts.Count)
			{
				box.gameObject.SetActive(true);
				box.SetHunt(_completedHunts[i]);
			}
			else
			{
				box.gameObject.SetActive(false);
			}
		}
		if (_completedHunts.Count == 0)
		{
			_noCompletedHuntsText.SetActive(true);
		}
		else
		{
			_noCompletedHuntsText.SetActive(false);
		}
	}

	public void AddCompletedHunt(HuntInfo info)
	{
		_completedHunts.Add(info);
		UpdateView();
	}

	public bool HasCompletedHunt(string huntID)
	{
		return _completedHunts.Exists(x => x.ID == huntID);
	}

}