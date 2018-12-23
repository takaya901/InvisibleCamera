using System.Threading.Tasks;
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
    [SerializeField] Text _text;                        //for debug
    [SerializeField] GameObject _mainCanvas;            //シャッターボタンとカメラ切り替えボタンが乗ったキャンバス
    [SerializeField] GameObject _camSwitchDialog;       //カメラ切り替え時の確認ダイアログ
    [SerializeField] GameObject _camSwitchingIndicator; //カメラ切り替え中のインジケータ
    
    Texture2D _quadTex;                    //カメラ映像投影用
    WebCamTextureToMatHelper _toMatHelper; //WebCamTextureをMatに変換する
    ToMatHelperManager _toMatHelperMgr;    //WebCamTextureToMatHelperの初期化等を行う
    CameraSwitcher _cameraSwitcher;        //リア/フロントを切り替える
    InvisibleConverter _invCvtr;
    FpsMonitor _fpsMonitor;

    const int TIME_WAIT_CAMERA = 5000;    //カメラ起動を待つ時間（ms）
    float _remainingWaitingTime;
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _toMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _invCvtr = new InvisibleConverter(_text);
        _cameraSwitcher = new CameraSwitcher(_toMatHelper, _invCvtr, _mainCanvas, _camSwitchDialog);
        _fpsMonitor = GetComponent<FpsMonitor>();
        
        //リア/フロントをPlayerPrefabsから読み込む
        _toMatHelper.requestedIsFrontFacing = _cameraSwitcher.UseCamera;
        _toMatHelperMgr = new ToMatHelperManager(gameObject, _toMatHelper, _fpsMonitor);
        _toMatHelper.Initialize();

        //スマホの場合カメラ起動まで指定秒待つ
        #if !UNITY_EDITOR && UNITY_ANDROID
            Task.Run(WaitCamStartup).Wait();
        #endif
    }
    
    void Update()
    {
        //カメラ起動まで待つ
        if (_remainingWaitingTime > 0) {
            _remainingWaitingTime -= Time.deltaTime;
            return;
        }
        _camSwitchingIndicator.SetActive(false);
        
        if (!_toMatHelper.IsPlaying() || !_toMatHelper.DidUpdateThisFrame()) return;

        //背景を保存する．StartでやるとWebCamのPlayが間に合わない？
        if (!_invCvtr.IsSavedBgr) {
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
        _cameraSwitcher.OnCameraSwitch();
        _remainingWaitingTime = TIME_WAIT_CAMERA / 1000f;    //カメラ起動まで待つ
        _camSwitchingIndicator.SetActive(true);
    }

    //キャンセルが押されたらダイアログ消去
    public void OnSwtichCancel()
    {
        _cameraSwitcher.OnSwtichCancel();
    }

    static async Task WaitCamStartup()
    {
        await Task.Delay(TIME_WAIT_CAMERA);
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