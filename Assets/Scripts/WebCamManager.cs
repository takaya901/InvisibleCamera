using System;
using OpenCVForUnity;
using OpenCVForUnityExample;
using UnityEngine;
using Text = UnityEngine.UI.Text;

/// <summary>
/// カメラ映像を透明人間に変換してQuadに投影する．
/// ボタンが押されたらリア/フロントを切り替える
/// </summary>
[RequireComponent(typeof(WebCamTextureToMatHelper), typeof(FpsMonitor))]
public class WebCamManager : MonoBehaviour
{
    [SerializeField] Text _text;
    [SerializeField] GameObject _mainCanvas;      //シャッターボタンとカメラ切り替えボタンが乗ったキャンバス
    [SerializeField] GameObject _camSwitchDialog; //カメラ切り替え時の確認ダイアログ
    
    Texture2D _quadTex;                    //カメラ映像投影用
    WebCamTextureToMatHelper _toMatHelper; //WebCamTextureをMatに変換する
    ToMatHelperManager _toMatHelperMgr;    //WebCamTextureToMatHelperの初期化等を行う
    CameraSwitcher _cameraSwitcher;        //リア/フロントを切り替える
    InvisibleConverter _invCvtr;
    FpsMonitor _fpsMonitor;

    const float TIME_WAIT_CAMERA = 5f;              //カメラ起動を待つ時間
    float _remainingWaitingTime = TIME_WAIT_CAMERA; //残り待ち時間
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _invCvtr = new InvisibleConverter(_text);
        _fpsMonitor = GetComponent<FpsMonitor>();
        _toMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _cameraSwitcher = new CameraSwitcher(_toMatHelper, _invCvtr, _mainCanvas, _camSwitchDialog);
        
        //リア/フロントをPlayerPrefabsから読み込む
        var useCamera = _cameraSwitcher.UseCamera;
        _toMatHelper.requestedIsFrontFacing = useCamera;
        _toMatHelperMgr = new ToMatHelperManager(gameObject, _toMatHelper, _fpsMonitor);
        _toMatHelper.Initialize();
    }
    
    void Update()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        //起動時とカメラ切替時にカメラ起動まで待つ
        if (_remainingWaitingTime > 0) {
            _remainingWaitingTime -= Time.deltaTime;
            return;
        }
        #endif
        if (!_toMatHelper.IsPlaying() || !_toMatHelper.DidUpdateThisFrame()) return;

        //背景を保存する．Startでやるとカメラ起動前に実行されてしまう
        if (!_invCvtr.HasSavedBgr) {
            _invCvtr.SaveBgr(_toMatHelper.GetMat());
        }
        
        //透明人間に変換して表示
        var webCamMat = _invCvtr.CvtToInvisible(_toMatHelper.GetMat());
        Utils.fastMatToTexture2D(webCamMat, _quadTex);
    }

    //カメラ切り替えボタンが押されたら確認ダイアログを表示
    public void OnSwitcherTouched()
    {
        _cameraSwitcher.OnSwitcherTouched();
    }
    
    //OKが押されたらリアカメラとフロントカメラを切り替える
    public void OnCameraSwitch()
    {
        ResetWaitingTime(); //カメラ起動するまで待つ
        _cameraSwitcher.OnCameraSwitch();
    }

    //キャンセルが押されたらダイアログ消去
    public void OnSwtichCancel()
    {
        _cameraSwitcher.OnSwtichCancel();
    }

    void ResetWaitingTime()
    {
        _remainingWaitingTime = TIME_WAIT_CAMERA;
    }

    //WebCamTextureToMatHelperの初期化・破棄・エラー処理
    public void OnWebCamTextureToMatHelperInitialized(){
        //QuadのTextureにWebCamTextureを設定する
        _toMatHelperMgr.OnWebCamTextureToMatHelperInitialized(ref _quadTex);
    }

    public void OnWebCamTextureToMatHelperDisposed (){
        _toMatHelperMgr.OnWebCamTextureToMatHelperDisposed(ref _quadTex);
    }
    
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
        _toMatHelperMgr.OnWebCamTextureToMatHelperErrorOccurred(errorCode);
    }
}