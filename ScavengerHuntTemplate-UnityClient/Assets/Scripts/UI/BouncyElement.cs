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
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Used by buttons to create a small visual bounce when pressing
/// </summary>
public class BouncyElement : MonoBehaviour
{
	[SerializeField] private bool _playSound = true;
	[SerializeField] private AudioClip _clickSound;
	private RectTransform _rt;
	private Vector2 _origSize;
	private Sequence _seq;

	// Start is called before the first frame update
	void Awake()
	{
		_rt = GetComponent<RectTransform>();
		_origSize = _rt.sizeDelta;
	}

	public void StartBounce()
	{
		_seq.Kill();
		_seq = DOTween.Sequence();
		_seq.Append(DOTween.To(() => _rt.sizeDelta, x => _rt.sizeDelta = x, _origSize * 1.3f, 0.045f).SetEase(Ease.OutSine));
		_seq.Append(DOTween.To(() => _rt.sizeDelta, x => _rt.sizeDelta = x, _origSize, 0.03f).SetEase(Ease.InSine));
		_seq.Play();
		if (_playSound)
		{
			Audio.PlaySound(_clickSound);
		}
	}

}