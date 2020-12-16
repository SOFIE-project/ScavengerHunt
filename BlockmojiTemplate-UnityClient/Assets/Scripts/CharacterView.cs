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

public class CharacterView : MonoBehaviour
{
	[SerializeField] private List<Image> _itemImages;
	private static CharacterView _instance;
	private void Awake()
	{
		_instance = this;
		ClearAll();
	}

	public static void Clear(int slotIndex)
	{
		Image image = _instance._itemImages[slotIndex];
		image.color = new Color(1, 1, 1, 0);
	}

	public static void ClearAll()
	{
		for (int i = 0; i < Equipment.SlotsCount; i++)
		{
			Clear(i);
		}
	}

	public static void SetItem(Item item)
	{
		Image image = _instance._itemImages[(int) item.Slot];
		image.sprite = ItemImageLoader.GetSprite(item);
		image.color = new Color(1, 1, 1, 1);
	}
}