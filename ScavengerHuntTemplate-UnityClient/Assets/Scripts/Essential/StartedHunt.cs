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
/// An object that contains a hunt's information, as well as the player's current progress in it
/// </summary>
public class StartedHunt
{
	public HuntInfo Info { get; private set; }

	/// <summary>
	/// 0 is first, 1 second as so on.
	/// </summary>
	/// <value></value>
	public int CurrentStep { get; private set; }

	public StartedHunt(HuntInfo info, int currentStep)
	{
		Info = info;
		CurrentStep = currentStep;
	}

	/// <summary>
	/// Increases step num to show next clue, or completes whole Hunt
	/// if this was the last step.
	/// </summary>
	public void CompleteStep()
	{
		CurrentStep++;
	}

}