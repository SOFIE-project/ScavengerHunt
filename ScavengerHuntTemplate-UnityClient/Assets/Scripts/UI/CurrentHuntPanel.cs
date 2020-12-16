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
/// Open by clicking a started hunt from started hunts view.
/// Has a list of ClueTask boxes.
/// This is the interface the player sees the most when playing the game.
/// The player sees their current progress here, the current clue or task
/// and enters task answers here.
/// </summary>
public class CurrentHuntPanel : MonoBehaviour
{
	public static CurrentHuntPanel Get { get; private set; }

	[SerializeField] private Text _nameText;
	[SerializeField] private Sprite _lockIcon;
	[SerializeField] private ClueTaskBox _clueTaskBox_template;
	public GameObject CorrectLocation_box;
	public StartedHunt CurrentHunt { get; private set; }
	private ScrollablePool<ClueTaskBox> _boxesPool;
	private int _lastTaskRequest;

	private void Awake()
	{
		Get = this;
		_boxesPool = new ScrollablePool<ClueTaskBox>(_clueTaskBox_template);
		CorrectLocation_box.SetActive(false);
	}

	private void Start()
	{
		iBeaconReceiver.Scan();
		iBeaconReceiver.BeaconRangeChangedEvent += OnBeaconDetected;
	}

	private void Update()
	{
		CorrectLocation_box.transform.GetChild(0).localPosition = 5 * Vector3.up * Mathf.Sin(2.5f * Time.time);
		//Test in editor in LocalTest
		if (LocalTest.Testing && Input.GetButtonDown("CheatComplete"))
		{
			ShowDummyTask();
		}

		//Cheating on PC with Server, works only with first hunt
		if (!LocalTest.Testing && Input.GetButtonDown("CheatBeacon"))
		{
			_lastTaskRequest = CurrentHunt.CurrentStep;
			ServerHandler.GetTask(CurrentHunt.Info.ID, CurrentHunt.CurrentStep, (CurrentHunt.CurrentStep + 1).ToString("X12"), OnReceivedTask);
		}

		if (Input.GetButtonDown("CheatComplete"))
		{
			_boxesPool[CurrentHunt.CurrentStep].AdvanceHunt("Correct!");
		}
	}

	private void OnBeaconDetected(Beacon[] beacons)
	{
		foreach (var beacon in beacons)
		{

			//rssi and strength seem to be in decibels, although with high variance even when standing stationary

			if (InRange(beacon))
			{
				if (!LocalTest.Testing)
				{
					//We are using the server
					_lastTaskRequest = CurrentHunt.CurrentStep;
					ServerHandler.GetTask(CurrentHunt.Info.ID, CurrentHunt.CurrentStep, beacon.instance, OnReceivedTask);
				}
				else
				{
					//Testing without server. Still needs to be on mobile for BT
					//Else, press "e" to show task on computer or upper right hidden button on mobile if no correct beacon in range

					//For some reason when LocalTesting, task doesn't always show up when a beacon is detected
					ShowDummyTask();
				}
			}
		}
	}

	public void ShowDummyTask()
	{
		if (!LocalTest.Testing)
		{
			return;
		}
		_boxesPool[CurrentHunt.CurrentStep].ShowTask("This is a dummy task question. There is no correct answer.");
	}

	private void OnReceivedTask(string response)
	{
		if (_lastTaskRequest != CurrentHunt.CurrentStep)
		{
			//Without this, upon loading the next clue, it might sometimes turn green with the previous task
			return;
		}
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}

		string task = (string) obj["data"];

		_boxesPool[CurrentHunt.CurrentStep].ShowTask(task);

	}

	public void SetHunt(StartedHunt startedHunt)
	{
		CurrentHunt = startedHunt;
		_nameText.text = startedHunt.Info.Name;
		//Show corrent number of boxes
		_boxesPool.IncreaseUntil(CurrentHunt.Info.NumClues);
		for (int i = 0; i < _boxesPool.Count; i++)
		{
			var box = _boxesPool[i];
			if (i < CurrentHunt.Info.NumClues)
			{
				box.gameObject.SetActive(true);
				if (i < CurrentHunt.CurrentStep)
				{
					box.SetCompleted();
				}
				else if (i == CurrentHunt.CurrentStep)
				{
					//Show empty before clue is actually loaded
					box.ShowEmptyClueTemp();
				}
				else
				{
					box.SetLocked();
				}
			}
			else
			{
				box.gameObject.SetActive(false);
			}
		}

		StartShowingClue();
	}

	private void StartShowingClue()
	{
		if (!LocalTest.Testing)
		{
			ServerHandler.GetClue(CurrentHunt.Info.ID, CurrentHunt.CurrentStep, OnResponseClue);
		}
		else if (CurrentHunt.CurrentStep < CurrentHunt.Info.NumClues)
		{
			//Have to check for clue num as SetHunt must also be called on Hunt completion.
			ShowClueText(CurrentHunt.CurrentStep, "This is a serverless test clue. Click on the right side of the hunt name text to pretend to find a beacon.");
		}
	}

	private void OnResponseClue(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}

		string clue = (string) obj["data"];

		ShowClueText(CurrentHunt.CurrentStep, clue);
	}

	private void ShowClueText(int clueNum, string text)
	{
		_boxesPool[clueNum].ShowClue(text);
	}

	private bool InRange(Beacon beacon)
	{
		return beacon.range == BeaconRange.IMMEDIATE || beacon.range == BeaconRange.NEAR;
	}

}