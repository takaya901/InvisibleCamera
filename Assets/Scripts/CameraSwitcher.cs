using System;
using OpenCVForUnityExample;
using UnityEngine;

/// <summary>
/// リア/フロントカメラを切り替える
/// </summary>
public class CameraSwitcher
{
	GameObject _mainCanvas;                //シャッターボタンとカメラ切り替えボタンが乗ったキャンバス
	GameObject _camSwitchDialog;           //カメラ切り替え時の確認ダイアログ
	WebCamTextureToMatHelper _toMatHelper; //WebCamTextureをMatに変換する
	InvisibleConverter _invCvtr;
	
	const string USE_CAMERA_KEY = "USE CAMERA"; //PlayerPrefabsのKey（リア/フロント）
	public bool UseCamera { get; }
	
	public CameraSwitcher(
		WebCamTextureToMatHelper toMatHelper, 
		InvisibleConverter invCvtr, 
		GameObject mainCanvas, GameObject camSwitchDialog)
	{
		_toMatHelper = toMatHelper;
		_invCvtr = invCvtr;
		_mainCanvas = mainCanvas;
		_camSwitchDialog = camSwitchDialog;
		
		UseCamera = Convert.ToBoolean(PlayerPrefs.GetInt(USE_CAMERA_KEY, 0));
	}
	
	//カメラ切り替えボタンが押されたら確認ダイアログを表示
	public void OnSwitcherTouched()
	{
		SwitchCanvas();
	}
    
	//OKが押されたらリアカメラとフロントカメラを切り替える
	public void OnCameraSwitch()
	{
		SwitchCanvas();
		//PlayerPrefsの使用カメラを書き換える
		PlayerPrefs.SetInt(USE_CAMERA_KEY, Convert.ToInt32(!_toMatHelper.requestedIsFrontFacing));
		_invCvtr.IsSavedBgr = false; //背景を保存し直す
		_toMatHelper.requestedIsFrontFacing = !_toMatHelper.requestedIsFrontFacing;
	}

	//キャンセルが押されたらダイアログ消去
	public void OnSwtichCancel()
	{
		SwitchCanvas();
	}
	
	//カメラ切り替えダイアログとメインキャンバスを切り替える
	void SwitchCanvas()
	{
		_mainCanvas.SetActive(!_mainCanvas.activeSelf);
		_camSwitchDialog.SetActive(!_camSwitchDialog.activeSelf);
	}
}
