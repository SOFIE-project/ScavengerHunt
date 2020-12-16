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

/// <summary>
/// UI element always visible in the bottom of the screen.
/// Use to switch between the main views of the game.
/// </summary>
public class TabManager : MonoBehaviour
{
	public const int NumTabs = 4;
	[SerializeField] private List<RectTransform> _icons;
	[SerializeField] private List<GameObject> _tabs;
	[SerializeField] private CompletedHuntsManager _completedHuntsManager;
	public static Tab CurrentTab { get; private set; } = Tab.StartedHunts;
	public static TabManager Get { get; private set; }
	private List<Vector2> _origSizes = new List<Vector2>();

	private void Awake()
	{
		Get = this;
		_completedHuntsManager.Initialize();
	}

	private void Start()
	{
		for (int i = 0; i < NumTabs; i++)
		{
			_origSizes.Add(_icons[i].sizeDelta);
			bool active = false;
			//In live, will switch to Nearby tab after signed in
			if (LocalTest.Testing)
			{
				active = i == (int) CurrentTab;
			}
			_tabs[i].SetActive(active);
		}
	}

	private void Update()
	{
		//Selected tab icon is bigger than others
		float scaleSpeed = 600f;
		for (int i = 0; i < NumTabs; i++)
		{
			Vector2 selectedSize = _origSizes[i] * 1.3f;
			RectTransform rt = _icons[i];
			if (i == (int) CurrentTab)
			{
				rt.sizeDelta += Vector2.one * scaleSpeed * Time.deltaTime;
				if (rt.sizeDelta.x > selectedSize.x)
				{
					rt.sizeDelta = selectedSize;
				}
			}
			else
			{
				rt.sizeDelta -= Vector2.one * scaleSpeed * Time.deltaTime;
				if (rt.sizeDelta.x < _origSizes[i].x)
				{
					rt.sizeDelta = _origSizes[i];
				}
			}
		}
	}

	public void SwitchTab(int tabIndex)
	{
		if (CurrentTab == (Tab) tabIndex)
		{
			if (CurrentTab == Tab.StartedHunts && StartedHuntsManager.Get.ShowingCurrentHunt())
			{
				//If showing current Hunt, go back to all started challenges
				StartedHuntsManager.Get.HideHunt();
			}
			else
			{
				return;
			}
		}
		CurrentTab = (Tab) tabIndex;
		for (int i = 0; i < NumTabs; i++)
		{
			_tabs[i].SetActive(i == tabIndex);
		}
	}

	public void SwitchTab(Tab tab)
	{
		SwitchTab((int) tab);
	}

}