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
/// A generic class for showing lots of similar UI elements in a scrollable view
/// Used for browsing nearby hunts, completed hunts and started hunts
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScrollablePool<T>
	where T : MonoBehaviour
	{
		public int Count { get { return _list.Count; } }
		private T _template;

		private List<T> _list = new List<T>();

		public ScrollablePool(T template)
		{
			_template = template;
			_list.Add(template);
			IncreasePool(5);
		}

		public void IncreasePool(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				GameObject gobj = GameObject.Instantiate(_template.gameObject, _template.transform.parent);
				//Positions controlled automatically by Content object's GridLayoutGroup component
				_list.Add(gobj.GetComponent<T>());
			}
		}

		public void IncreaseUntil(int count)
		{
			while (Count < count)
			{
				IncreasePool(1);
			}
		}

		public T this [int i]
		{
			get
			{
				while (_list.Count <= i)
				{
					IncreasePool(5);
				}
				return _list[i];
			}
		}

	}