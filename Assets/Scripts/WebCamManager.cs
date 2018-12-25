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
    [SerializeField] Text _text;                      //for debug
    [SerializeField] GameObject _mainCanvas;          //シャッターボタンとカメラ切り替えボタンが乗ったキャンバス
    [SerializeField] GameObject _camSwitchDialog;     //カメラ切り替え時の確認ダイアログ
    [SerializeField] GameObject _waitingingIndicator; //カメラ起動・背景取得中のインジケータ
    [SerializeField] GameObject _recordStopCanvas;
    [SerializeField] GameObject _preferences;
    
    Texture2D _quadTex;                    //カメラ映像投影用
    WebCamTextureToMatHelper _toMatHelper; //WebCamTextureをMatに変換する
    ToMatHelperManager _toMatHelperMgr;    //WebCamTextureToMatHelperの初期化等を行う
    CameraSwitcher _cameraSwitcher;        //リア/フロントを切り替える
    InvisibleConverter _invCvtr;
    MovieTaker _movieTaker;
    FpsMonitor _fpsMonitor;
    AudioSource _recordSound;

    bool _isRecording;
    const int TIME_WAIT_CAMERA = 1000;    //カメラ起動を待つ時間（ms）
    float _remainingWaitingTime;
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _toMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _invCvtr = new InvisibleConverter(_text);
        _cameraSwitcher = new CameraSwitcher(_toMatHelper, _invCvtr, _mainCanvas, _camSwitchDialog);
        _fpsMonitor = GetComponent<FpsMonitor>();
        _recordSound = GetComponent<AudioSource>();
        
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
        //アプリ起動時とカメラ切替時，カメラ起動まで待つ
        if (_remainingWaitingTime > 0) {
            _remainingWaitingTime -= Time.deltaTime;
            return;
        }
        _waitingingIndicator.SetActive(false);
        
        if (!_toMatHelper.IsPlaying() || !_toMatHelper.DidUpdateThisFrame()) return;

        //背景を保存する．StartでやるとWebCamのPlayが間に合わない？
        if (!_invCvtr.IsSavedBg) {
            _invCvtr.SaveBgr(_toMatHelper.GetMat());
        }
        
        //透明人間に変換して表示
        var invImg = _invCvtr.CvtToInvisible(_toMatHelper.GetMat());
        if (_isRecording) {
            _movieTaker?.Write(invImg);
        }
        Utils.fastMatToTexture2D(invImg, _quadTex);
    }

    //録画ボタンが押されたら録画開始
    public void OnRecordButtonTouched()
    {
        _recordSound.Play();
        _movieTaker = new MovieTaker(_invCvtr.GetImgSize());
        _mainCanvas.SetActive(false);
        _recordStopCanvas.SetActive(true);
        _isRecording = true;
    }

    //録画停止ボタンが押されたら録画停止
    public void OnStopButtonTouched()
    {
        _recordSound.Play();
        _movieTaker.Close();
        _mainCanvas.SetActive(true);
        _recordStopCanvas.SetActive(false);
        _isRecording = false;
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
        _waitingingIndicator.SetActive(true);
    }

    //RESETが押されたら背景再取得
    public void OnResetButtonTouched()
    {
        _invCvtr.SaveBgr(_toMatHelper.GetMat());
    }

    //キャンセルが押されたらダイアログ消去
    public void OnSwtichCancel()
    {
        _cameraSwitcher.OnSwtichCancel();
    }

    public void OnPreferencesTouched()
    {
        _mainCanvas.SetActive(false);
        _preferences.SetActive(true);
        
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