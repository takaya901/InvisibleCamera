using System;
using System.IO;
using OpenCVForUnity;
using UnityEngine;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.VideoWriter;


//http://akagi13213.hatenablog.com/entry/2017/02/11/004422
public class MovieTaker
{
    VideoWriter _writer;
    string _fileName;
    string _filePath = "";
    const string EXT = ".avi";
    const int FPS = 15;
    string a;

    public MovieTaker(Size imgSize)
    {
        _fileName = "Inv_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + EXT;
        #if UNITY_EDITOR
        _filePath += _fileName;
        var size = new Size(1280, 720);
        #elif UNITY_ANDROID
        _filePath = Application.persistentDataPath + "/Inv_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + EXT;
        var size = new Size(360, 640);
        #endif
        _writer = new VideoWriter(_filePath, fourcc('M','J','P','G'), FPS, size);
    }

    public void Write(Mat img)
    {
        _writer.write(img);
    }

    public void Close()
    {
        _writer.release();
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (!File.Exists(_filePath)) return;
        try {
            NativeGallery.SaveVideoToGallery(_filePath, "InvisibleCamera", _fileName);
            File.Delete(_filePath);
        }
        catch (Exception e) {
            a = e.Message;
        }
        #endif
    }
}
