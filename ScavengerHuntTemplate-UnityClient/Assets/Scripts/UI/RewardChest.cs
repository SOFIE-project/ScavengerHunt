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
/// Appears in HuntCompletedWindow. Juicy rewards animation!
/// </summary>
public class RewardChest : MonoBehaviour
{
	[SerializeField] private Image _glow;
	[SerializeField] private Image _lid_closed;
	[SerializeField] private Image _lid_open;
	[SerializeField] private AudioClip _startOpenSound;
	[SerializeField] private AudioClip _openedSound;
	[SerializeField] private GameObject _rewards;
	[SerializeField] private GameObject _rateStars;
	[SerializeField] private GameObject _buttonDone;
	[SerializeField] private GameObject _messageBox;
	[SerializeField] private ParticleSystem _coinParticles;
	[SerializeField] private Transform xp_ball_prefab;
	[SerializeField] private Transform xp_ball_pivot;
	private HuntCompletedWindow _huntCompletedWindow;

	private Animator _animator;
	private float _xp_time = 0;
	private bool _show_xp = false;
	private int _xp_balls_left = 0;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_huntCompletedWindow = GetComponentInParent<HuntCompletedWindow>();
	}

	private void Update()
	{
		if (!_show_xp) return;

		_xp_time += Time.deltaTime;
		if (_xp_time > 0.07f && _xp_balls_left > 0)
		{
			_xp_time = 0;
			_xp_balls_left--;
			Transform ball = Instantiate(xp_ball_prefab, xp_ball_pivot.parent);
			ball.position = xp_ball_pivot.position;
		}
	}

	/// <summary>
	/// Called by animation
	/// </summary>
	public void StartOpening()
	{
		_animator.SetTrigger("StartOpen");
		Audio.PlaySound(_startOpenSound);
	}

	/// <summary>
	/// Called by animation
	/// </summary>
	public void SetOpen()
	{
		_glow.enabled = true;
		_lid_open.enabled = true;
		_lid_closed.enabled = false;
		Audio.PlaySound(_openedSound);
		_coinParticles.Play();
	}

	/// <summary>
	/// Called by animation
	/// </summary>
	public void ShowRewards()
	{
		_rewards.SetActive(true);
		UpdateRewards();
	}

	/// <summary>
	/// Called by animation
	/// </summary>
	private void UpdateRewards()
	{
		HeaderPanel.Get.IncreaseCurrencies(_huntCompletedWindow.CoinsReward, _huntCompletedWindow.GemsReward, _huntCompletedWindow.StarsReward);
	}

	/// <summary>
	/// Called by animation
	/// </summary>
	public void AnimationDone()
	{
		_messageBox.SetActive(true);
		_buttonDone.SetActive(true);

		//Update Level and xp

		int xp = _huntCompletedWindow.XPReward;
		XPBar.Get.AddXP(xp);
		_show_xp = true;
		_xp_balls_left = 10;

		_rateStars.SetActive(true);
	}

}