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
/// Press in Nearby Hnts view to view this hunt's info in a bigger window
/// </summary>
public class HuntInfoButton : MonoBehaviour
{
	public HuntInfo HuntInfo { get; private set; }

	[SerializeField] private Text _nameText;

	public void SetInfo(HuntInfo huntInfo)
	{
		HuntInfo = huntInfo;
		_nameText.text = huntInfo.Name;
	}

	public void ShowHuntInfoWindow()
	{
		HuntInfoWindow.Get.Show(HuntInfo);
	}
}