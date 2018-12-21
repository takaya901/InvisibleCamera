using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// シャッターが押されたら音を鳴らしてスクリーンショットを保存する
/// </summary>
public class ShutterHandler : MonoBehaviour
{
	[SerializeField] Canvas _canvas;
	AudioSource _shutter;
	string _imgSavePath = "";
	const string EXT = ".jpg";
	const string IMG_SAVE_DIR = "/InvisibleCamera/";
	
	void Start ()
	{
		_shutter = GetComponent<AudioSource>();
		
		#if !UNITY_EDITOR && UNITY_ANDROID
		//ギャラリーに表示される保存パスを取得	https://qiita.com/fukaken5050/items/9619aeeb131120939bc1
		using(var jcEnvironment = new AndroidJavaClass("android.os.Environment"))
		using(var joPublicDir = jcEnvironment.CallStatic<AndroidJavaObject>(
			"getExternalStoragePublicDirectory",
			jcEnvironment.GetStatic<string>("DIRECTORY_PICTURES"))) {
			var outputPath = joPublicDir.Call<string>("toString");
			_imgSavePath = outputPath + IMG_SAVE_DIR;
			if (!Directory.Exists(_imgSavePath)) Directory.CreateDirectory(_imgSavePath);
		}
		#endif
	}
	
	public void OnShutterClick()
	{
		_shutter.Play();
		var imgName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + EXT;
		_imgSavePath += imgName;
		StartCoroutine(CaptureScreenshot());
	}
	
	//UIを非表示にしてからスクリーンショットを保存する
	IEnumerator CaptureScreenshot()
	{
		_canvas.gameObject.SetActive(false);
		var screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		yield return new WaitForEndOfFrame();

		screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
		screenShot.Apply();
		var bytes = screenShot.EncodeToPNG();
		File.WriteAllBytes(_imgSavePath, bytes);
		Destroy(screenShot);
		_canvas.gameObject.SetActive(true);
	}
	
	static void ScanMedia (string fileName)
	{
		if (Application.platform != RuntimePlatform.Android) return;
		#if UNITY_ANDROID
		using (var jcUnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"))
		using (var joActivity = jcUnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity"))
		using (var joContext = joActivity.Call<AndroidJavaObject> ("getApplicationContext"))
		using (var jcMediaScannerConnection = new AndroidJavaClass ("android.media.MediaScannerConnection")){
			jcMediaScannerConnection.CallStatic ("scanFile", joContext, new[] { fileName }, new[] { "image/png" }, null);
		}
//		Handheld.StopActivityIndicator();
		#endif
	}
}
