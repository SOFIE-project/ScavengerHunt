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
/// A clickable box with an item sprite. Click to see the item's info.
/// Functions with "0 references" are called from buttons' OnClick,
/// set in the buttons' inspector window.
/// </summary>
public class ItemBox : MonoBehaviour
{
	public Item Item { get; private set; }
	private Image _backgroundImage;
	private Color _origBackgroundColor;
	private Sprite _originalSprite;
	[SerializeField] private Image _itemImage;

	private void Awake()
	{
		_backgroundImage = GetComponent<Image>();
		_origBackgroundColor = _backgroundImage.color;
		_originalSprite = _itemImage.sprite;
	}
	public void SetItem(Item item)
	{
		Item = item;
		//Texture will not be null, because ItemBox.SetItem() is called only
		//after ItemImageLoader.AllLoaded() is true
		_itemImage.sprite = ItemImageLoader.GetSprite(item);
		Color averageColor = AverageColor(ItemImageLoader.GetTexture(item));
		_backgroundImage.color = Color.Lerp(_origBackgroundColor, averageColor, 0.8f);
	}
	public void Hide()
	{
		_itemImage.enabled = false;
		_backgroundImage.enabled = false;
	}

	public void Show()
	{
		_itemImage.enabled = true;
		_backgroundImage.enabled = true;
	}

	/// <summary>
	/// Used to create a suitable background color for the item box
	/// Note: ignores transparent pixels
	/// </summary>
	/// <param name="tex"></param>
	/// <returns></returns>
	private Color AverageColor(Texture2D tex)
	{
		float totalRed = 0;
		float totalGreen = 0;
		float totalBlue = 0;
		int count = 0;
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < tex.height; y++)
			{
				var pixel = tex.GetPixel(x, y);
				if (pixel.a < 0.5f)
				{
					continue;
				}
				totalRed += pixel.r;
				totalGreen += pixel.g;
				totalBlue += pixel.b;
				count++;
			}
		}
		return new Color(totalRed / count, totalGreen / count, totalBlue / count, 1);
	}

	public void Clear()
	{
		_backgroundImage.color = _origBackgroundColor;
		_itemImage.sprite = _originalSprite;
		Item = null;
	}

	public void ShowInfo()
	{
		ItemInfoWindow.Get.Show(Item);
	}
}