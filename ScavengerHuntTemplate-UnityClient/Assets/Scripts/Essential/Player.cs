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
using System.Linq;
using UnityEngine;
public static class Player
{
	public static string ID { get; private set; }

	/// <summary>
	/// Set true to register a new player every time
	/// Will generate a random string = virtually guaranteed that it will be a unique string
	/// </summary>
	private static bool _newPlayerEveryTime = false;

	static Player()
	{
		if (_newPlayerEveryTime)
		{
			ID = "RandomNewPlayer_" + RandomString(20);
			return;
		}

		if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
		{
			ID = "Editor2";
			return;
		}

		ID = SystemInfo.deviceUniqueIdentifier;
	}

	private static string RandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
	}
}