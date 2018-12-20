using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Imgcodecs;

public class ShutterHandler : MonoBehaviour
{
	[SerializeField] Text _text;
	AudioSource _shutter;
	string _imgSavePath = "";
	const string EXTENSION = ".png";
	const string IMG_SAVE_DIRECTORIY = "/Invisible/";
	
	void Start ()
	{
		_shutter = GetComponent<AudioSource>();
		
		#if !UNITY_EDITOR && UNITY_ANDROID
		// 保存パスを取得
		using(var jcEnvironment = new AndroidJavaClass("android.os.Environment"))
		using(var joPublicDir = jcEnvironment.CallStatic<AndroidJavaObject>(
			"getExternalStoragePublicDirectory",
			jcEnvironment.GetStatic<string>("DIRECTORY_PICTURES"))) {
			var outputPath = joPublicDir.Call<string>("toString");
			_imgSavePath = outputPath + IMG_SAVE_DIRECTORIY;
			if (!Directory.Exists(_imgSavePath)) Directory.CreateDirectory(_imgSavePath);
		}
		#endif
	}
	
	public void OnClick()
	{
		_shutter.Play();
		var webCamMat = WebCamManager.GetWebCamMat();
		var invisible = InvisibleProcessor.ConvertToInvisible(webCamMat);
		cvtColor(invisible, invisible, COLOR_BGR2RGB);

		var imgName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + EXTENSION;
		_imgSavePath += imgName;
		if (imwrite(_imgSavePath, invisible)) {
			_text.text = "success";
		}
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
