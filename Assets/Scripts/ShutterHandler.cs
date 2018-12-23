using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// シャッターが押されたら音を鳴らしてスクリーンショットを保存する
/// </summary>
public class ShutterHandler : MonoBehaviour
{
	[SerializeField] Canvas _mainCanvas; //シャッターボタンとカメラ切り替えボタンが乗ったキャンバス
	AudioSource _shutter;
	const string EXT = ".jpg";
	const string IMG_SAVE_DIR = "InvisibleCamera";
	
	void Start()
	{
		_shutter = GetComponent<AudioSource>();
	}
	
	public void OnShutterTouched()
	{
		_shutter.Play();
		StartCoroutine(CaptureScreenshot());
	}
	
	//UIを非表示にしてからスクリーンショットを保存する
	IEnumerator CaptureScreenshot()
	{
		_mainCanvas.gameObject.SetActive(false);
		var screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		yield return new WaitForEndOfFrame();

		screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
		screenShot.Apply();

		#if UNITY_EDITOR
			var imgName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + EXT;
			var bytes = screenShot.EncodeToPNG();
			File.WriteAllBytes(imgName, bytes);
		#elif UNITY_ANDROID
			NativeGallery.SaveImageToGallery(screenShot, IMG_SAVE_DIR, "Inv{0}" + EXT);
		#endif
		Destroy(screenShot);
		_mainCanvas.gameObject.SetActive(true);
	}
}
