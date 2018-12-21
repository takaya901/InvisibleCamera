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
    Texture2D _quadTex;                     //カメラ映像投影用
    WebCamTextureToMatHelper _toMatHelper;  //WebCamTextureをMatに変換する
    ToMatHelperManager _toMatHelperManager; //WebCamTextureToMatHelperの初期化・破棄を行う
    InvisibleConverter _invCvtr;            //透明人間に変換する
    FpsMonitor _fpsMonitor;

    const float TIME_WAIT_CAMERA = 5f;           //カメラ起動を待つ時間
    float _remainingWaitTime = TIME_WAIT_CAMERA; //残り待ち時間
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _invCvtr = new InvisibleConverter(_text);
        _fpsMonitor = GetComponent<FpsMonitor>();
        _toMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _toMatHelperManager = new ToMatHelperManager(gameObject, _toMatHelper, _fpsMonitor);
        _toMatHelper.Initialize();
    }
    
    void Update()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        //起動時とカメラ切替時にカメラ起動まで待つ
        if (_remainingWaitTime > 0) {
            _remainingWaitTime -= Time.deltaTime;
            return;
        }
        #endif
        if (!_toMatHelper.IsPlaying() || !_toMatHelper.DidUpdateThisFrame()) return;

        //背景を保存する．Startでやるとカメラ起動前に実行されてしまう
        if (!_invCvtr.HasSavedBgr) {
            _invCvtr.SaveBgr(_toMatHelper.GetMat());
        }
        
        var webCamMat = _invCvtr.CvtToInvisible(_toMatHelper.GetMat());
        Utils.fastMatToTexture2D(webCamMat, _quadTex);
    }

    //ボタンが押されたらリアカメラとフロントカメラを切り替える
    public void OnCameraSwitch()
    {
        _invCvtr.HasSavedBgr = false;          //背景を保存し直す
        _remainingWaitTime = TIME_WAIT_CAMERA; //カメラ起動するまで待つ
        _toMatHelper.requestedIsFrontFacing = !_toMatHelper.requestedIsFrontFacing;
    }

    //WebCamTextureToMatHelperの初期化・破棄
    public void OnWebCamTextureToMatHelperInitialized(){
        //QuadのTextureにWebCamTextureを設定する
        _toMatHelperManager.OnWebCamTextureToMatHelperInitialized(ref _quadTex);
    }

    public void OnWebCamTextureToMatHelperDisposed (){
        _toMatHelperManager.OnWebCamTextureToMatHelperDisposed(ref _quadTex);
    }
    
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
        _toMatHelperManager.OnWebCamTextureToMatHelperErrorOccurred(errorCode);
    }
}