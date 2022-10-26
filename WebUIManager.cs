using SkyboxVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebUIManager : MonoBehaviour
{
    bool _isWebViewShow;

    void Update()
    {
        if (ScrollViewManager.Singleton.CurChannelType == ChannelType.Web)
        {
            _isWebViewShow = true;
        }
        else
        {
            _isWebViewShow = false;
        }
        WebViewShowControl(_isWebViewShow);
    }

    void WebViewShowControl(bool isWebViewShow)
    {

        transform.Find("InterfaceDir").gameObject.SetActive(!isWebViewShow);
        if (transform.Find("MyWebViewDemo").gameObject != null)
        {
            transform.Find("MyWebViewDemo").gameObject.SetActive(isWebViewShow);
        }
    }
}