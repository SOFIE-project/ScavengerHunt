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
/// Displays /5 stars based on old user ratings of this hunt
/// </summary>
public class AverageUserRating : MonoBehaviour
{
	[SerializeField] private List<Image> _stars;
	[SerializeField] private Sprite _star_empty;
	[SerializeField] private Sprite _star_full;
	public void ShowRating(float rating)
	{
		int intRating = Mathf.RoundToInt(rating);
		for (int i = 0; i < 5; i++)
		{
			if (i < intRating)
			{
				_stars[i].sprite = _star_full;
			}
			else
			{
				_stars[i].sprite = _star_empty;
			}
		}
	}
}