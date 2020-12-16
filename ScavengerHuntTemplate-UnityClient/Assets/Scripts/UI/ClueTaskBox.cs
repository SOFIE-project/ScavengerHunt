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
/// A visual box that can display a hunt's clue or task.
/// When playing a hunt, the player sees a list of these, arranged vertically.
/// Only one is active at a time. The ones before it are completed
/// and the ones after this are locked.
/// </summary>
public class ClueTaskBox : MonoBehaviour
{
	public int coinsAmount { get; private set; }
	public int gemsAmount { get; private set; }
	public int starsAmount { get; private set; }
	public int xpAmount { get; private set; }
	public string message { get; private set; }

	[SerializeField] private Color _clueColor;
	[SerializeField] private Color _taskColor;
	[SerializeField] private Color _completedColor;
	[SerializeField] private Color _lockedColor;
	[SerializeField] private Sprite _completedIcon;
	[SerializeField] private Sprite _lockedIcon;
	[SerializeField] private Image _lockedCompleteImage;
	[SerializeField] private GameObject _inputField_gobj;
	[SerializeField] private GameObject _sendButton_gobj;
	[SerializeField] private Text _playerInput_text;
	[SerializeField] private GameObject _huntCompletedWindow_prefab;
	[SerializeField] private Text _hintText;
	[SerializeField] private GameObject _powerButtons_gobj;
	[SerializeField] private GameObject _hintButton_gobj;
	[SerializeField] private GameObject _skipButton_gobj;
	private Text _clueText;
	private Image _boxImage;
	private bool _init = false;

	private enum State { Clue, Task, Locked, Completed }

	private State _state = State.Clue;

	private void Awake()
	{
		InitializeIfNeeded();
	}

	/// <summary>
	/// Other methods than Awake also call this. That's because those
	/// other functions might get called before Awake, so need to initialize.
	/// </summary>
	private void InitializeIfNeeded()
	{
		if (_init)
		{
			return;
		}
		_clueText = GetComponentInChildren<Text>();
		_boxImage = GetComponent<Image>();
		_init = true;
	}

	private void Update()
	{
		if (LocalTest.Testing)
		{
			if (Input.GetButtonDown("GiveCurrencies"))
			{
				HeaderPanel.Get.IncreaseCurrencies(100, 10, 1);
			}
		}

		//Pulsate color if showing task
		if (_state == State.Task)
		{
			_boxImage.color = _taskColor * (1.1f + 0.1f * Mathf.Sin(2 * Time.time));
		}
	}

	public void SetCompleted()
	{
		InitializeIfNeeded();
		_state = State.Completed;
		_lockedCompleteImage.gameObject.SetActive(true);
		_lockedCompleteImage.sprite = _completedIcon;
		_clueText.gameObject.SetActive(false);
		_boxImage.color = _completedColor;
		_inputField_gobj.SetActive(false);
		_sendButton_gobj.SetActive(false);

		ResetHintAndPowersButtons();
	}

	public void SetLocked()
	{
		InitializeIfNeeded();
		_state = State.Locked;
		_lockedCompleteImage.gameObject.SetActive(true);
		_lockedCompleteImage.sprite = _lockedIcon;
		_clueText.gameObject.SetActive(false);
		_boxImage.color = _lockedColor;
		_inputField_gobj.SetActive(false);
		_sendButton_gobj.SetActive(false);

		ResetHintAndPowersButtons();
	}

	public void ShowClue(string clue)
	{
		InitializeIfNeeded();
		_state = State.Clue;
		_lockedCompleteImage.gameObject.SetActive(false);
		_clueText.gameObject.SetActive(true);
		_clueText.text = clue;
		_boxImage.color = _clueColor;
		_inputField_gobj.SetActive(false);
		_sendButton_gobj.SetActive(false);

		//Only one box is showing Clue or Task at a time
		//so it's ok to toggle CorrectLocation_box from here
		CurrentHuntPanel.Get.CorrectLocation_box.SetActive(false);

		ResetHintAndPowersButtons();
		ShowSkipButtonIfHasStarsAndHaventUsed();
		ShowHintOrButtonIfCan();
	}

	public void ShowEmptyClueTemp()
	{
		InitializeIfNeeded();
		_state = State.Clue;
		_lockedCompleteImage.gameObject.SetActive(false);
		_clueText.gameObject.SetActive(true);
		_clueText.text = "";
		_boxImage.color = _clueColor;
		_inputField_gobj.SetActive(false);
		_sendButton_gobj.SetActive(false);

		//Only one box is showing Clue or Task at a time
		//so it's ok to toggle CorrectLocation_box from here
		CurrentHuntPanel.Get.CorrectLocation_box.SetActive(false);

		ResetHintAndPowersButtons();
	}

	public void ShowTask(string task)
	{
		InitializeIfNeeded();
		_state = State.Task;
		if (_clueText.text == task)
		{
			//Prevents resetting InputField input while typing.
			return;
		}

		_lockedCompleteImage.gameObject.SetActive(false);
		_clueText.gameObject.SetActive(true);
		_clueText.text = task;
		_boxImage.color = _taskColor;
		_inputField_gobj.SetActive(true);
		_inputField_gobj.GetComponent<InputField>().text = "";
		_sendButton_gobj.SetActive(true);

		//Only one box is showing Clue or Task at a time
		//so it's ok to toggle CorrectLocation_box from here
		CurrentHuntPanel.Get.CorrectLocation_box.transform.parent = this.transform;
		CurrentHuntPanel.Get.CorrectLocation_box.SetActive(true);
		CurrentHuntPanel.Get.CorrectLocation_box.transform.SetAsFirstSibling();

		ResetHintAndPowersButtons();
		ShowSkipButtonIfHasStarsAndHaventUsed();
	}

	private void ResetHintAndPowersButtons()
	{
		HideHint();
		HidePowerButtons();
	}

	private void HideHint()
	{
		_hintText.gameObject.SetActive(false);
	}

	private void HidePowerButtons()
	{
		_powerButtons_gobj.SetActive(false);
		_hintButton_gobj.SetActive(false);
		_skipButton_gobj.SetActive(false);
	}

	private void ShowHintOrButtonIfCan()
	{
		//Note: will have to pay for Hint again if exit CurrentHuntPanel and come back
		if (!LocalTest.Testing)
		{
			ServerHandler.GetHint(CurrentHuntPanel.Get.CurrentHunt.Info.ID, CurrentHuntPanel.Get.CurrentHunt.CurrentStep, OnGetHintResponse);
		}
		else
		{
			_hintText.text = "This is a placeholder hint.";
			ShowHintButtonIfHaveGems();
		}
	}

	private void OnGetHintResponse(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}
		string hint = (string) obj["data"];

		//If this clue has no hint, don't attempt to show it nor the hint button
		if (hint == null)
		{
			return;
		}

		//Hint text is still disabled
		_hintText.text = hint;
		ShowHintButtonIfHaveGems();
	}

	private void ShowHintButtonIfHaveGems()
	{
		if (HeaderPanel.Get.Gems < 10)
		{
			return;
		}
		//We already know that hasn't used Hint for this step yet:
		ShowHintButton();
	}

	private void ShowHintButton()
	{
		_powerButtons_gobj.SetActive(true);
		_hintButton_gobj.SetActive(true);
	}

	private void ShowSkipButtonIfHasStarsAndHaventUsed()
	{
		if (HeaderPanel.Get.Stars == 0)
		{
			return;
		}
		if (!LocalTest.Testing)
		{
			ServerHandler.HasUsedStar(CurrentHuntPanel.Get.CurrentHunt.Info.ID, HasUsedStarOnResponse);
		}
		else
		{
			ShowSkipButton();
		}
	}

	private void HasUsedStarOnResponse(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}
		bool hasUsedStar = (bool) obj["data"];

		if (!hasUsedStar)
		{
			ShowSkipButton();
		}
	}

	private void ShowSkipButton()
	{
		_powerButtons_gobj.SetActive(true);
		_skipButton_gobj.SetActive(true);
	}

	/// <summary>
	/// Button calls this on pressed
	/// </summary>
	public void SendTaskAnswer()
	{
		StartedHunt hunt = CurrentHuntPanel.Get.CurrentHunt;
		if (!LocalTest.Testing)
		{
			ServerHandler.CheckTaskAnswerAndAdvance(hunt.Info.ID, hunt.CurrentStep, _playerInput_text.text, OnTaskAnswerResponse);
		}
		else
		{
			AdvanceHunt("Correct!");
		}
	}

	public void SkipStep()
	{
		if (!LocalTest.Testing)
		{
			ServerHandler.UseStar(CurrentHuntPanel.Get.CurrentHunt.Info.ID, OnUseStarResponse);
		}
		else
		{
			UseStar_Client();
		}
	}

	private void OnUseStarResponse(string response)
	{
		Debug.Log(response);
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}

		UseStar_Client();

	}

	private void UseStar_Client()
	{
		HeaderPanel.Get.IncreaseCurrencies(0, 0, -1);
		AdvanceHunt("Skip!");
	}

	public void UseHint()
	{
		if (!LocalTest.Testing)
		{
			ServerHandler.UseHint(CurrentHuntPanel.Get.CurrentHunt.Info.ID, CurrentHuntPanel.Get.CurrentHunt.CurrentStep, OnUseHintResponse);
		}
		else
		{
			UseGems_Client();
		}
	}

	private void OnUseHintResponse(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			return;
		}

		if ((string) obj["message"] == "Success")
		{
			UseGems_Client();
		}
	}

	private void UseGems_Client()
	{
		//Hint text is already set
		_hintText.gameObject.SetActive(true);
		_hintButton_gobj.SetActive(false);
		HeaderPanel.Get.IncreaseCurrencies(0, -10, 0);
	}

	private void OnTaskAnswerResponse(string response)
	{
		JObject obj = JObject.Parse(response);
		if ((int) obj["code"] != 200)
		{
			if ((string) obj["message"] == "Incorrect answer")
			{
				PopupMessageManager.Show("Not correct!");
			}
			return;
		}
		AdvanceHunt("Correct!", response);

	}

	/// <summary>
	/// This is called after we've got a successful response from server.
	/// If this was the last task, server has already marked whole Hunt as complete.
	/// </summary>
	public void AdvanceHunt(string popupMessage = "", string response = "")
	{
		PopupMessageManager.Show(popupMessage);

		CurrentHuntPanel.Get.CurrentHunt.CompleteStep();
		//Update view of current hunt panel
		CurrentHuntPanel.Get.SetHunt(CurrentHuntPanel.Get.CurrentHunt);

		HuntInfo completedHunt = CurrentHuntPanel.Get.CurrentHunt.Info;

		//Check if completed whole Hunt
		if (CurrentHuntPanel.Get.CurrentHunt.CurrentStep == completedHunt.NumClues)
		{
			CompletedHuntsManager.Get.AddCompletedHunt(completedHunt);

			var completedWindow = Instantiate(_huntCompletedWindow_prefab, GetComponentInParent<Canvas>().transform);

			if (!LocalTest.Testing)
			{
				Debug.Log(response);
				JObject obj = JObject.Parse(response);
				coinsAmount = (int) obj["data"]["coins_reward"];
				gemsAmount = (int) obj["data"]["gems_reward"];
				starsAmount = (int) obj["data"]["stars_reward"];
				xpAmount = (int) obj["data"]["total_xp"];
				message = (string) obj["data"]["message"];
			}
			else
			{
				coinsAmount = 117;
				gemsAmount = 10;
				starsAmount = Random.Range(0, 3) == 0 ? 1 : 0;
				xpAmount = 150;
			}

			completedWindow.GetComponent<HuntCompletedWindow>().SetHuntRewards(completedHunt, completedHunt.Name, coinsAmount, gemsAmount, starsAmount, xpAmount, message);

			StartedHuntsManager.Get.RemoveHunt(completedHunt.ID);
			StartedHuntsManager.Get.HideHunt();
		}
	}

}