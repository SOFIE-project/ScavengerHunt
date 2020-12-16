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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

/// <summary>
/// A client representation of an item token
/// </summary>
public class Item
{
	#region Fields from token
	public string UniqueID { get; private set; }
	public string Name { get; private set; }
	public string Source { get; private set; }
	public string Description { get; private set; }
	public Slot Slot { get; private set; }
	private Dictionary<string, int> _attributes = new Dictionary<string, int>();
	public IReadOnlyDictionary<string, int> Attributes => _attributes;
	public string ImageURL { get; private set; }
	#endregion
	public bool ImageLoaded { get; private set; } = false;
	private static List<Item> _all = new List<Item>();
	public static ReadOnlyCollection<Item> All => _all.AsReadOnly();

	/// <summary>
	/// To prevent incorrect public construction.
	/// </summary>
	private Item() { }

	/// <summary>
	/// Creates item object from Jobject, adds it to Item.All, starts coroutine to load image, and returns reference to Item
	/// </summary>
	/// <param name="jobj"></param>
	/// <returns></returns>
	public static Item FromJObject(JObject jobj)
	{
		Item item = new Item();
		item.UniqueID = (string) jobj["UniqueID"];
		item.Name = (string) jobj["Name"];
		item.Source = (string) jobj["Source"];
		item.Description = (string) jobj["Description"];
		item.Slot = (Slot) Enum.Parse(typeof(Slot), (string) jobj["Slot"]);
		List<string> attributeNames = ((JObject) jobj["Attributes"]).Properties().Select(p => p.Name).ToList();
		foreach (string attName in attributeNames)
		{
			item._attributes[attName] = (int) jobj["Attributes"][attName];
		}
		item.ImageURL = (string) jobj["ImageUrl"];
		ItemImageLoader.StartLoadingImage(item);
		_all.Add(item);
		return item;
	}

	public static Item Get(string uniqueID)
	{
		return _all.Find(i => i.UniqueID == uniqueID);
	}

}