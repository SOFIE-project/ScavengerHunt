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
/// Shows if task answer is correct or wrong, when player attempts to answer in a task.
/// Can also be used for printing debug text on mobile!
/// </summary>
public class PopupMessage : MonoBehaviour
{
	[SerializeField] AudioClip _showSound;
	private Image _image;
	private Text _text;
	private float _time = 0;

	private void Awake()
	{
		_image = GetComponent<Image>();
		Audio.PlaySound(_showSound);
	}

	private void Update()
	{
		_time += Time.deltaTime;
		transform.position += Vector3.up * 2.5f * Time.deltaTime;
		if (_time > 2)
		{
			Color color = _image.color;
			color.a -= 0.4f * Time.deltaTime;
			_image.color = color;
		}
		if (_time > 8)
		{
			Destroy(this);
		}
	}

}