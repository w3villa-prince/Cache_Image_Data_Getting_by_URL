using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEditor;
using System;

public class GetImageByURL : MonoBehaviour
{



    Texture2D tempTexture;
    [SerializeField] RawImage sprite;
    float _maxCacheTimeLimit = 1;
    bool _isLoaddata;
    public static Dictionary<string, DateTime> _imageDic = new Dictionary<string, DateTime>();
    void Start()
    {
        Initialize();


    }
    void Update()
    {
        if (_isLoaddata && tempTexture == null)
        {

            GettingTextureByURL("http://graph.facebook.com/10150001787221949/picture?type=normal", ImageRespectWith.Player);
        }

    }

    private void Initialize()
    {
        _isLoaddata = true;


        SetLocalDataLoading();



    }
    private void SetLocalDataLoading()
    {
        string localdata = PlayerPrefs.GetString("LocalDic", null);
        _imageDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(localdata);
        Debug.Log(_imageDic + "Load Data Sucees");




    }
    private void SaveLocalDic()
    {
        string value = Newtonsoft.Json.JsonConvert.SerializeObject(_imageDic);
        PlayerPrefs.SetString("LocalDic", value);
    }

    private void GettingTextureByURL(string url, ImageRespectWith imageRespectWith)
    {
        //Debug.Log("PAth Setup");
        string fileName = TextToHex(url);
        if (!Directory.Exists(Application.persistentDataPath + "/Player/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + "/Player/");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/Game/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + "/Game/");
        }


        string filePath = Application.persistentDataPath + "/None" + fileName;
        if (imageRespectWith == ImageRespectWith.Player)
        {
            filePath = Application.persistentDataPath + "/player/" + fileName;

        }
        else if (imageRespectWith == ImageRespectWith.Game)
        {
            filePath = Application.persistentDataPath + "/game/" + fileName;

        }
        LoadTextureFromFileOrDownload(url, filePath);

    }

    public void LoadTextureFromFileOrDownload(string url, string filePath)
    {

        string fileName = TextToHex(url);


        if (File.Exists(filePath) && _isLoaddata)
        {
            //Debug.Log("local file getting "+ _imageDic[fileName]); 
            if (DateTime.Now <= _imageDic[fileName])
            {
                StartCoroutine(LoadTextureFromSave(filePath));
                //Debug.Log("Old data getting "+ _imageDic[fileName]+ "\n"+DateTime.Now);

            }
            else
            {
                StartCoroutine(LoadTextureFromURI(url, filePath));
                DateTime _UpadateCurrentTime = DateTime.Now;
                _imageDic[fileName] = _UpadateCurrentTime.AddMinutes(_maxCacheTimeLimit);
                //Debug.Log(" update Old data getting " +_UpadateCurrentTime +"\n"+ DateTime.Now);

            }



        }
        else
        {
            DateTime _dateTime = DateTime.Now;
            _dateTime.AddMinutes(_maxCacheTimeLimit);
            _imageDic.Clear();
            _imageDic.Add(fileName, _dateTime);
            //Debug.Log("Store Value"+ _imageDic[fileName] +fileName);
            StartCoroutine(LoadTextureFromURI(url, filePath));
        }


        SaveLocalDic();

    }


    string TextToHex(string text)
    {
        byte[] stringByte = System.Text.Encoding.Default.GetBytes(text);
        string txt = System.BitConverter.ToString(stringByte);
        txt = txt.Replace("-", "");
        return txt;
    }


    IEnumerator LoadTextureFromURI(string uri, string savePath)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(uri);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            tempTexture = DownloadHandlerTexture.GetContent(req);
            sprite.texture = tempTexture;
            Debug.Log("Loaded image url");
            StartCoroutine(SaveTexture(savePath));
        }
    }


    IEnumerator SaveTexture(string savePath)
    {
        byte[] texturebyte = tempTexture.EncodeToJPG();
        try
        {
            File.WriteAllBytes(savePath, texturebyte);
        }
        catch { }
        yield return null;
    }


    IEnumerator LoadTextureFromSave(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        Texture2D loadTexture = new Texture2D(1, 1);
        try
        {
            loadTexture.LoadImage(bytes);
            tempTexture = loadTexture;
            sprite.texture = loadTexture;

            Debug.Log("Loaded from file");
            Debug.Log(filePath);
        }
        catch (System.Exception e)
        {
            Debug.Log("Catch call" + e);
        }
        yield return null;
    }
    public void DeleteAssets(ImageRespectWith imageRespectWith)
    {
        string path = Application.persistentDataPath + "/";

        if (imageRespectWith == ImageRespectWith.Player)
        {
            path = Application.persistentDataPath + "/player/";

        }
        else if (imageRespectWith == ImageRespectWith.Game)
        {
            path = Application.persistentDataPath + "/game/";

        }
        Debug.Log($"Clear File Data {path} ");
        // File.Delete(path);
        FileUtil.DeleteFileOrDirectory(path);
    }

    public void CleanUnUseFile()
    {
        if (_imageDic != null)
        {
            foreach (KeyValuePair<string, DateTime> Imagedata in _imageDic)
            {
                if (DateTime.Now >= _imageDic[Imagedata.Key])
                {
                    string _playerDataPath = Application.persistentDataPath + "/player/" + Imagedata.Key;
                    string _gameDataPath = Application.persistentDataPath + "/game/" + Imagedata.Key;

                    if (File.Exists(_playerDataPath))
                    {

                        Debug.Log($"Clear File Data {_playerDataPath} ");
                        // File.Delete(path);
                        FileUtil.DeleteFileOrDirectory(_playerDataPath);
                    }
                    else if (File.Exists(_gameDataPath))
                    {

                        Debug.Log($"Clear File Data {_gameDataPath} ");
                        // File.Delete(path);
                        FileUtil.DeleteFileOrDirectory(_gameDataPath);
                    }


                }

            }
        }
    }

    public enum ImageRespectWith
    {
        None,
        Player,
        Game
    }
}
