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
/// A UI element found in HuntCompletedWindow.
/// User can click on the amount of stars /5 they want to rate the hunt.
/// Will be sent to the backend, after pressing "Done" in the HuntCompletedWindow
/// </summary>
public class RateStars : MonoBehaviour
{
	[SerializeField] private List<Image> _stars;
	[SerializeField] private Sprite _star_empty;
	[SerializeField] private Sprite _star_full;
	[SerializeField] private AudioClip _rateSound;

	private HuntCompletedWindow _huntCompletedWindow;

	private void Awake()
	{
		_huntCompletedWindow = GetComponentInParent<HuntCompletedWindow>();
	}

	/// <summary>
	/// int rating must be in range [1, 5]
	/// </summary>
	/// <param name="rating"></param>
	public void Rate(int rating)
	{
		_huntCompletedWindow.Rating = rating;
		for (int i = 0; i < 5; i++)
		{
			if (i < rating)
			{
				_stars[i].sprite = _star_full;
			}
			else
			{
				_stars[i].sprite = _star_empty;
			}
		}
		Audio.PlaySound(_rateSound);
	}
}