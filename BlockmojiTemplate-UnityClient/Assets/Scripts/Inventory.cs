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

public class Inventory : MonoBehaviour
{
	[SerializeField] private GameObject _headerPrefab;
	[SerializeField] private GameObject _boxSectionPrefab;
	[SerializeField] private Transform _content;
	private Dictionary<string, ItemBoxSection> _categories = new Dictionary<string, ItemBoxSection>();
	private static Inventory _instance;
	private void Awake()
	{
		_instance = this;
	}
	public static void AddItem(Item item)
	{
		//For now, only sorting by slot
		if (!_instance._categories.ContainsKey(item.Slot.ToString()))
		{
			_instance.AddCategory(item.Slot.ToString());
		}
		_instance._categories[item.Slot.ToString()].AddBox(item);
	}

	private void AddCategory(string category)
	{
		//Add header
		GameObject header = Instantiate(_headerPrefab, _content);
		header.GetComponent<Text>().text = category;
		//Add box set
		GameObject boxSection = Instantiate(_boxSectionPrefab, _content);
		//Remember this category for future use
		_categories[category] = boxSection.GetComponent<ItemBoxSection>();
	}

	public static void ClearAll()
	{
		foreach (Transform child in _instance._content.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	public static void Update()
	{
		Inventory.ClearAll();
		foreach (var item in Item.All)
		{
			Inventory.AddItem(item);
		}

	}
}