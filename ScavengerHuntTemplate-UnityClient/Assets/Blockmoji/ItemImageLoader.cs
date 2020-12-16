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
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Enable downloading item icons in the background.
/// </summary>
public class ItemImageLoader : MonoBehaviour
{
	private static Dictionary<Item, Sprite> _sprites = new Dictionary<Item, Sprite>();
	private static Dictionary<Item, Texture2D> _textures = new Dictionary<Item, Texture2D>();
	private static ItemImageLoader _instance;
	public static bool FirstRequestSent { get; private set; } = false;

	private void Awake()
	{
		_instance = this;
	}

	public static void StartLoadingImage(Item item)
	{
		_instance.StartCoroutine(LoadImage(item));
	}

	private static IEnumerator LoadImage(Item item)
	{
		FirstRequestSent = true;
		_sprites[item] = null;
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(item.ImageURL);
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else
		{
			Texture2D tex = ((DownloadHandlerTexture) www.downloadHandler).texture;
			_textures[item] = tex;
			_sprites[item] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		}
	}

	public static Sprite GetSprite(Item item)
	{
		return _sprites[item];
	}

	public static Texture2D GetTexture(Item item)
	{
		return _textures[item];
	}

	public static bool AllLoaded()
	{
		return _sprites.Values.ToList().TrueForAll(t => t != null);
	}
}