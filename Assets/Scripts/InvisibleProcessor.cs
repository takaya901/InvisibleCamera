using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using OpenCVForUnity;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;

public class InvisibleProcessor
{
    Mat _background;
    static readonly Scalar SKIN_LOWER = new Scalar(0, 70, 0);
    static readonly Scalar SKIN_UPPER = new Scalar(40, 255, 255);

    public void SaveBackground(Mat webcamMat)
    {
        _background = webcamMat.clone();
    }
    
    public Mat ConvertToInvisible(Mat webcamMat)
    {
        //カメラ映像ををHSVに変換
        var blured = new Mat();
        GaussianBlur(webcamMat, blured, new Size(19, 19), 0);
        var hsv = new Mat();
        cvtColor(blured, hsv, COLOR_RGBA2RGB);
        cvtColor(hsv, hsv, COLOR_RGB2HSV);
        
        //肌色領域が0のマスクを作成
        var skinMask = new Mat();
        inRange(hsv, SKIN_LOWER, SKIN_UPPER, skinMask);
        morphologyEx(skinMask, skinMask, MORPH_OPEN, new Mat(), new Point(-1, -1), 3);
//        morphologyEx(skinMask, skinMask, MORPH_CLOSE, new Mat(), new Point(-1, -1), 3);
        
        //肌色領域を置換する背景マスクを作成
        var backMask = new Mat();
        bitwise_and(_background, _background, backMask, skinMask);
        
        //カメラ映像の肌色領域を黒にする
        bitwise_not(skinMask, skinMask);
        var withoutSkin = new Mat();
        bitwise_and(webcamMat, webcamMat, withoutSkin, skinMask);
        
        //カメラ映像の肌色領域を背景で置換
        var invisible = new Mat();
        bitwise_or(withoutSkin, backMask, invisible);
        
        return invisible;
    }
    
}
