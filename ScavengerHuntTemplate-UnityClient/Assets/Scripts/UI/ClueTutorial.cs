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
/// Appears once in CurrentHuntPanel, when playing the first hunt.
/// </summary>
public class ClueTutorial : MonoBehaviour
{
	public static bool Shown { get; private set; } = false;
	[SerializeField] private Transform _pivot;

	private AppearingWindow _appearingWindow;

	public static ClueTutorial Get { get; private set; }

	private void Awake()
	{
		Get = this;
		_appearingWindow = GetComponent<AppearingWindow>();
	}

	private void Update()
	{
		_pivot.localPosition = Vector3.up * 6 * Mathf.Sin(3 * Time.time);
	}

	public void Show()
	{
		_appearingWindow.Open();
		Shown = true;
	}
}