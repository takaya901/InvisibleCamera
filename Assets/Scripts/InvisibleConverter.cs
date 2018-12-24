using OpenCVForUnity;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;
using Text = UnityEngine.UI.Text;

public class InvisibleConverter
{
    Mat _bg;    //背景
    Text _text; //for debug
    static readonly Scalar SKIN_LOWER = new Scalar(0, 0, 50);
    static readonly Scalar SKIN_UPPER = new Scalar(20, 200, 255);
    static readonly Scalar HAIR_LOWER = new Scalar(0, 0, 0);    
    static readonly Scalar HAIR_UPPER = new Scalar(180, 120, 100);

    /// <summary>背景を保存したかどうか</summary>
    public bool IsSavedBg { get; set; }

    public InvisibleConverter(Text text)
    {
        _text = text;
    }

    public void SaveBgr(Mat webcamMat)
    {
        _bg = webcamMat.clone();
        IsSavedBg = true;
//        _text.text = _bg.size().ToString();
    }
    
    /// <summary>肌と髪の領域を背景で置換する</summary>
    /// <param name="webcamMat">カメラからのRGBA画像</param>
    /// <returns>置換後のRGBA画像</returns>
    public Mat CvtToInvisible(Mat webcamMat)
    {
        var rgbWebcam = new Mat(); var rgbBg = new Mat(); var rgbDiff = new Mat(); var binDiff = new Mat();
        var blurred = new Mat(); var hsv = new Mat();
        var skinMask = new Mat(); var hairMask = new Mat(); var skinAndHairMask = new Mat();
        var bgOnSkinAndHair = new Mat(); var invisible = new Mat();
        
        //背景とカメラ映像の差分を取る（アルファチャンネル以外）
        cvtColor(webcamMat, rgbWebcam, COLOR_RGBA2RGB);
        cvtColor(_bg, rgbBg, COLOR_RGBA2RGB);
        absdiff(rgbWebcam, rgbBg, rgbDiff);
        cvtColor(rgbDiff, binDiff, COLOR_RGB2GRAY);
        threshold(binDiff, binDiff, 50, 255, THRESH_BINARY);
        
        //肌と髪の領域と同じ位置の背景領域を抽出
        GaussianBlur(webcamMat, blurred, new Size(7, 7), 0);
        cvtColor(blurred, hsv, COLOR_RGBA2RGB);
        cvtColor(hsv, hsv, COLOR_RGB2HSV);
        inRange(hsv, SKIN_LOWER, SKIN_UPPER, skinMask);
        inRange(hsv, HAIR_LOWER, HAIR_UPPER, hairMask);
        
        bitwise_or(skinMask, hairMask, skinAndHairMask);
        bitwise_and(skinAndHairMask, binDiff, skinAndHairMask);
        bitwise_and(_bg, _bg, bgOnSkinAndHair, skinAndHairMask);
        
        //カメラ映像から肌と髪以外の領域を抽出
        bitwise_not(skinAndHairMask, skinAndHairMask);
        var withoutSkinAndHair = new Mat();
        bitwise_and(webcamMat, webcamMat, withoutSkinAndHair, skinAndHairMask);
        
        //カメラ映像の肌と髪の領域を背景で置換
        bitwise_or(withoutSkinAndHair, bgOnSkinAndHair, invisible);
        
        return invisible;
    }

    public Size GetImgSize()
    {
        return _bg.size();
    }
}
