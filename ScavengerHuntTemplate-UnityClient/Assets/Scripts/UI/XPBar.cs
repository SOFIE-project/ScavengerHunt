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
/// Shows current Xp and Level of the player
/// Visible in the HeaderPanel on the top of the mobile screen
/// </summary>
public class XPBar : MonoBehaviour
{
	public static XPBar Get { get; private set; }

	[SerializeField] private Text _text;
	[SerializeField] private RectTransform _fill;
	[SerializeField] private AudioClip _levelUp_sound;

	public static int[] XpReqs = new int[] { 100, 110, 121, 133, 146, 161, 177, 194, 214, 235, 259, 285, 313, 345, 379 };

	public int Level { get; private set; } = 1;
	public int XP { get; private set; } = 0;

	private int _xpDelayReserve = 0;
	private float _time = 0;

	private BouncyElement _bouncyElement;

	private void Awake()
	{
		Get = this;
		_bouncyElement = GetComponent<BouncyElement>();
		SetXPFill(0);
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_xpDelayReserve <= 0)
		{
			return;
		}

		//1 xp per frame
		_xpDelayReserve--;
		XP++;

		while (XP >= XpReqs[Level - 1])
		{
			XP -= XpReqs[Level - 1];
			Level++;
			LevelUpEffects();
		}
		UpdateLevelText();
		UpdateXPFill();
	}

	private void SetXPFill(float t)
	{
		_fill.localScale = new Vector3(1, t, 1);
	}

	private void UpdateLevelText()
	{
		_text.text = "Level " + Level.ToString();
	}

	private void UpdateXPFill()
	{
		float t = (float) XP / (float) XpReqs[Level - 1];
		SetXPFill(t);
	}

	private void LevelUpEffects()
	{
		_bouncyElement.StartBounce();
		Audio.PlaySound(_levelUp_sound);
	}

	public void AddXP(int xp)
	{
		_xpDelayReserve += xp;
	}

	/// <summary>
	/// Called when need a hard update from server
	/// </summary>
	/// <param name="level"></param>
	/// <param name="xp"></param>
	public void SetLevelAndXP(int level, int xp)
	{
		Level = level;
		XP = xp;
		UpdateLevelText();
		UpdateXPFill();
	}

}