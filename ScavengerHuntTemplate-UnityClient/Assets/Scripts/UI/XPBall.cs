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
/// A visual ball - many of these fly toward the XP bar in the top of the screen
/// after opened the reward chest
/// </summary>
public class XPBall : MonoBehaviour
{
	[SerializeField] private AudioClip _xp_gain_sound;
	private Vector3 _speed;
	private float _time = 0;

	void Awake()
	{
		//transform.parent = GetComponentInParent<Canvas>().transform;
		_speed = 5 * new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
	}

	// Update is called once per frame
	void Update()
	{
		_time += Time.deltaTime;
		transform.position += _speed * Time.deltaTime;
		_speed += 4f * (XPBar.Get.transform.position - transform.position).normalized;

		if ((transform.position - XPBar.Get.transform.position).magnitude < 1 || _time > 2.3f)
		{
			XPBar.Get.GetComponent<BouncyElement>().StartBounce();
			Audio.PlaySound(_xp_gain_sound);
			Destroy(gameObject);
		}
	}
}