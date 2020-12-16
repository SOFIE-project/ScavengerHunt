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
using System.Collections.ObjectModel;
using UnityEngine;

public class Equipment : MonoBehaviour
{
	[SerializeField] private List<ItemBox> _itemBoxes;
	private static List<Item> _equippedItems = new List<Item>();
	public static ReadOnlyCollection<Item> EquippedItems => _equippedItems.AsReadOnly();
	private static Dictionary<string, int> _totalAttributes = new Dictionary<string, int>();
	public static IReadOnlyDictionary<string, int> TotalAttributes => _totalAttributes;
	public static int SlotsCount => 6;
	private static Equipment _instance;

	private void Awake()
	{
		_instance = this;
	}
	public static void Equip(Item item, bool updateBackend = true)
	{
		foreach (var equipped in _equippedItems)
		{
			if (equipped.Slot == item.Slot)
			{
				Unequip(equipped, false);
				break;
			}
		}
		_equippedItems.Add(item);
		_instance._itemBoxes[(int) item.Slot].SetItem(item);
		CharacterView.SetItem(item);
		RecalculateAttributes();
		if (updateBackend)
		{
			APIHandler.UpdateEquipped(EquippedItems, r => { });
		}
	}

	public static void Unequip(Item item, bool updateBackend = true)
	{
		_equippedItems.Remove(item);
		_instance._itemBoxes[(int) item.Slot].Clear();
		CharacterView.Clear((int) item.Slot);
		RecalculateAttributes();
		if (updateBackend)
		{
			APIHandler.UpdateEquipped(EquippedItems, r => { });
		}
	}

	public static void RecalculateAttributes()
	{
		_totalAttributes.Clear();
		foreach (var item in _equippedItems)
		{
			foreach (var att in item.Attributes.Keys)
			{
				if (!_totalAttributes.ContainsKey(att))
				{
					_totalAttributes[att] = item.Attributes[att];
				}
				else
				{
					_totalAttributes[att] += item.Attributes[att];
				}
			}
		}
		AttributesDisplay.TotalAttributes.UpdateView(TotalAttributes);
	}

	public static bool IsEquipped(Item item)
	{
		return _equippedItems.Contains(item);
	}

}