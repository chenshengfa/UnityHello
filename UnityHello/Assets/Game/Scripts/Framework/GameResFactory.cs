﻿using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
public class GameResFactory
{
    private static GameResFactory sInstance = null;
    public static GameResFactory Instance()
    {
        if (sInstance == null)
        {
            sInstance = new GameResFactory();
            sInstance.mResManager = AppFacade.Instance.GetManager<ResourceManager>();
        }
        return sInstance;
    }

    internal ResourceManager mResManager;
    private List<GameObject> mUIEffectsList = new List<GameObject>();
    public AssetPacker mAssetPacker = null;

    //private List<GameObject> mUIList = new List<GameObject>();
    //private Dictionary<string, GameObjectCache> mResCaches = new Dictionary<string, GameObjectCache>();

    public Sprite GetResSprite(string spName)
    {
        if (mAssetPacker == null)
        {
            return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        }
        return mAssetPacker.GetSprite(spName);
    }

    public void GetUIPrefab(string assetName, Transform parent, LuaTable luaTable, LuaFunction luaCallBack)
    {
        if (mResManager == null) return;

        string tmpAssetName = assetName;
        if (GameSetting.DevelopMode)
        {
            tmpAssetName = "UIPrefab/" + assetName;
        }
        mResManager.LoadPrefab(assetName + GameSetting.ExtName, tmpAssetName, delegate(UnityEngine.Object[] objs)
        {
            if (objs.Length == 0) return;
            GameObject prefab = objs[0] as GameObject;
            if (prefab == null)
            {
                return;
            }
            GameObject go = UnityEngine.GameObject.Instantiate(prefab) as GameObject;
            go.name = assetName;
            go.layer = LayerMask.NameToLayer("UI");

            Vector3 anchorPos = Vector3.zero;
            Vector2 sizeDel = Vector2.zero;
            Vector3 scale = Vector3.one;

            RectTransform rtTr = go.GetComponent<RectTransform>();
            if (rtTr != null)
            {
                anchorPos = rtTr.anchoredPosition;
                sizeDel = rtTr.sizeDelta;
                scale = rtTr.localScale;
            }

            go.transform.SetParent(parent, false);

            if (rtTr != null)
            {
                rtTr.anchoredPosition = anchorPos;
                rtTr.sizeDelta = sizeDel;
                rtTr.localScale = scale;
            }

            LuaBehaviour tmpBehaviour = go.AddComponent<LuaBehaviour>();
            tmpBehaviour.Init(luaTable);

            if (luaCallBack != null)
            {
                luaCallBack.BeginPCall();
                luaCallBack.Push(go);
                luaCallBack.PCall();
                luaCallBack.EndPCall();
            }
            Debug.Log("CreatePanel::>> " + assetName + " " + prefab);
            //mUIList.Add(go);
        });
    }

    public void DestroyUIPrefab(GameObject go)
    {
        GameObject.Destroy(go);
        //mUIList.Remove(go);
    }

    protected void GetEffectObj(string effname, System.Action<GameObject> callBack)
    {
        if (mResManager == null) return;
        mResManager.LoadPrefab(effname, effname, delegate(UnityEngine.Object[] objs)
        {
            if (objs.Length == 0) return;
            GameObject prefab = objs[0] as GameObject;
            if (prefab == null)
            {
                return;
            }
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            if (callBack != null)
            {
                callBack(go);
            }
        });
    }

    //获取UI特效
    public void GetUIEffect(string effname, LuaFunction luaCallBack)
    {
        GetEffectObj(effname, (Obj) =>
        {
            if (Obj != null)
            {
                mUIEffectsList.Add(Obj);
            }
            if (luaCallBack != null)
            {
                luaCallBack.BeginPCall();
                luaCallBack.Push(Obj);
                luaCallBack.PCall();
                luaCallBack.EndPCall();
            }
        });
    }

    public void DestroyUIEffect(GameObject obj)
    {
        GameObject.Destroy(obj);
        //mUIEffectsList.Remove(obj);
    }

    public void DestroyAllUIEffect()
    {
        for (int i = mUIEffectsList.Count - 1; i >= 0; --i)
        {
            GameObject.Destroy(mUIEffectsList[i]);
        }
        mUIEffectsList.Clear();
    }
}
