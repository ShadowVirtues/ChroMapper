﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using Zenject;

public class CustomPlatformSettings : IInitializable
{
    public Dictionary<string, PlatformInfo> CustomPlatformsDictionary = new Dictionary<string, PlatformInfo>();

    private Settings settings;

    [Inject]
    private void Construct(Settings settings)
    {
        this.settings = settings;
    }

    public void Initialize()
    {
        LoadCustomEnvironments();
    }

    public GameObject[] LoadPlatform(string name)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(CustomPlatformsDictionary[name].Info.FullName);

        GameObject[] platformPrefab = bundle.LoadAssetWithSubAssets<GameObject>("_CustomPlatform");

        bundle.Unload(false);

        Debug.Log("Load platform/s: " + name + " " + platformPrefab.Length);
        return platformPrefab;
    }

    private void LoadCustomEnvironments()
    {
        string beatSaberCustomPlatforms = settings.CustomPlatformsFolder;

        if (Directory.Exists(beatSaberCustomPlatforms))
        {
            //FileUtil.ReplaceDirectory(beatSaberCustomPlatforms, customPlatformsDirectory);

            //Then import these platforms from the AssetDirectory
            CustomPlatformsDictionary.Clear();
            foreach (var file in Directory.GetFiles(beatSaberCustomPlatforms))
            {
                FileInfo info = new FileInfo(file);
                if (!info.Extension.ToUpper().Contains("PLAT")) continue;
                //Use AssetBundle. Not AssetDatabase.
                string name = info.Name.Split('.')[0];
                if (CustomPlatformsDictionary.ContainsKey(name))
                {
                    Debug.LogError(":hyperPepega: :mega: YOU HAVE TWO PLATFORMS WITH THE SAME FILE NAME");
                }
                else
                {
                    PlatformInfo platInfo = new PlatformInfo();
                    platInfo.Info = info;
                    using (MD5 md5 = MD5.Create())
                    using (Stream stream = File.OpenRead(info.FullName))
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("X2").ToLower());
                        }
                        platInfo.Md5Hash = sb.ToString();
                        CustomPlatformsDictionary.Add(name, platInfo);
                    }
                }
            }
        }
    }
}

public struct PlatformInfo
{
    public FileInfo Info;
    public string Md5Hash;
}
