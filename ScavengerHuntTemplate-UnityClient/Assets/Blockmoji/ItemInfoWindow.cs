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

public class ItemInfoWindow : MonoBehaviour
{
	[SerializeField] private GameObject _window;
	[SerializeField] private Image _itemImage;
	[SerializeField] private Text _itemName;
	[SerializeField] private Text _source;
	[SerializeField] private Text _description;
	[SerializeField] private AudioClip _showHideSound;
	[SerializeField] private AttributesDisplay _attributesDisplay;
	private Animator _animator;
	private bool _showing = false;
	public static Item Item { get; private set; }
	public static ItemInfoWindow Get;

	private void Awake()
	{
		Get = this;
		_animator = GetComponentInChildren<Animator>();
	}

	private void Start()
	{
		Hide();
	}

	public void Show(Item item)
	{
		_showing = true;
		_window.SetActive(true);
		Audio.PlaySound(_showHideSound);
		Item = item;
		_itemName.text = item.Name;
		_source.text = item.Source;
		_description.text = item.Description;
		_itemImage.sprite = ItemImageLoader.GetSprite(item);
		_attributesDisplay.UpdateView(item.Attributes);
	}

	public void Hide()
	{
		_showing = false;
		_window.SetActive(false);
		Audio.PlaySound(_showHideSound);
	}

}