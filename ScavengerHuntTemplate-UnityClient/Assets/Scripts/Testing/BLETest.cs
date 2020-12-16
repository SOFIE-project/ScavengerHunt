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
/// Can be used basic detection of BLE beacons, once the iBeaon plugin is installed.
/// </summary>
public class BLETest : MonoBehaviour
{
	private Text _text;

	private void Awake()
	{
		_text = GetComponent<Text>();
	}

	private void Start()
	{
		iBeaconReceiver.Scan();
		iBeaconReceiver.BeaconRangeChangedEvent += OnBeaconRangeChanged;
	}

	private void OnBeaconRangeChanged(Beacon[] beacons)
	{

		_text.text = "";
		foreach (Beacon beacon in beacons)
		{
			_text.text += "\n" + beacon.UUID + "\n Major: " + beacon.major.ToString() + "  Minor: " + beacon.minor.ToString() + "\n";
		}
	}
}