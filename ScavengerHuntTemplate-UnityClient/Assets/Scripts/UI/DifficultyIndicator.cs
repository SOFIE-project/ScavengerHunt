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
/// Shows /5 stars how difficult the hunt designer has rated this hunt.
/// </summary>
public class DifficultyIndicator : MonoBehaviour
{
	[SerializeField] private List<Image> _indicators;
	public void ShowDifficulty(int difficulty)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < difficulty)
			{
				_indicators[i].color = Color.white;
			}
			else
			{
				_indicators[i].color = Color.black;
			}
		}
	}
}