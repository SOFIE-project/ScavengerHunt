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
/// Fetch and browse the player's current hunts
/// </summary>
public class StartedHuntsManager : MonoBehaviour
{
	public static StartedHuntsManager Get { get; private set; }

	[SerializeField] private StartedHuntButton _buttonTemplate;
	[SerializeField] private GameObject _noStartedHuntsText;
	[SerializeField] private RectTransform _startedHuntsPanel;
	private List<StartedHunt> _startedHunts = new List<StartedHunt>();
	private ScrollablePool<StartedHuntButton> _buttonsPool;
	private Animator _animator;
	private float _time = 0;

	private void Awake()
	{
		Get = this;
		_buttonsPool = new ScrollablePool<StartedHuntButton>(_buttonTemplate);
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		//So that won't show template boxes before response
		UpdateView();
		if (!LocalTest.Testing)
		{
			ServerHandler.GetStartedHunts(OnReceivedStartedHunts);
		}
	}

	private void Update()
	{
		_time += Time.deltaTime;

		if (_time > 1 && !ShowingCurrentHunt())
		{
			//Refresh started hunts
			ServerHandler.GetStartedHunts(OnReceivedStartedHunts);
			_time = 0;
		}
	}

	private void OnReceivedStartedHunts(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			UpdateView();
			return;
		}

		_startedHunts.Clear();
		foreach (JObject startedHunt in obj["data"])
		{
			HuntInfo info = HuntInfo.FromJObject((JObject) startedHunt["hunt_info"]);

			AddStartedHunt(info, (int) startedHunt["started_hunt"]["current_clue"]);
		}

		UpdateView();
	}

	private void UpdateView()
	{
		//Update boxes
		_buttonsPool.IncreaseUntil(_startedHunts.Count);
		for (int i = 0; i < _buttonsPool.Count; i++)
		{
			var button = _buttonsPool[i];
			if (i < _startedHunts.Count)
			{
				button.gameObject.SetActive(true);
				button.SetHunt(_startedHunts[i].Info);
			}
			else
			{
				button.gameObject.SetActive(false);
			}
		}
		//Set current hunt
		if (_startedHunts.Count > 0)
		{
			_noStartedHuntsText.SetActive(false);
		}
		else
		{
			_noStartedHuntsText.SetActive(true);
		}
	}

	public StartedHunt AddStartedHunt(HuntInfo info, int step = 0)
	{
		StartedHunt startedHunt = new StartedHunt(info, step);
		_startedHunts.Add(startedHunt);
		UpdateView();
		return startedHunt;
	}

	public void RemoveHunt(string huntID)
	{
		_startedHunts.Remove(_startedHunts.Find(x => x.Info.ID == huntID));
		UpdateView();
	}

	public void ShowHunt(string huntID)
	{
		CurrentHuntPanel.Get.SetHunt(_startedHunts.Find(x => x.Info.ID == huntID));
		_animator.SetTrigger("ShowCurrentHunt");
		if (!ClueTutorial.Shown)
		{
			ClueTutorial.Get.Show();
		}
	}

	public void HideHunt()
	{
		_animator.SetTrigger("HideCurrentHunt");
	}

	public bool HasStartedHunt(string huntID)
	{
		return _startedHunts.Exists(x => x.Info.ID == huntID);
	}

	public bool ShowingCurrentHunt()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("Shown");
	}
}