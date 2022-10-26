using UnityEngine;
using Vuplex.WebView;
using SkyboxVR;
using System.Threading.Tasks;
using System.Timers;
using Vuplex.WebView.Demos;
using System.Collections;
using System.IO;

namespace SkyboxVR {
    class MyWebViewDemo : SingletonMonoBehaviour<MyWebViewDemo>
    {
        Timer _buttonRefreshTimer = new Timer();
        WebViewPrefab _controlsWebViewPrefab;
        WebViewPrefab _mainWebViewPrefab;
        HardwareKeyboardListener _hardwareKeyboardListener;
        Keyboard _keyBoard;
        string _lastScreenState = "false";
        string _creentScreenState = "false";

        //ֻ�����������µ�mesh�����¸�ֵ
        GameObject _realWebView;
        public Material _sphereMaterial;


        async void Start()
        {
            // ���Խ��ĳЩ��վ�ĵ�¼���⣨ep.Google�����û�������Ϊtrue���� https://developer.vuplex.com/webview/Web#SetUserAgent
            Web.SetUserAgent(true);

            //Windows����Drm
            var wideVineFolderPath = Path.Combine(Application.streamingAssetsPath, "widevine");
            StandaloneWebView.SetCommandLineArguments($"--widevine-cdm-path=\"{wideVineFolderPath}\"");

            transform.localPosition = new Vector3(0, -370, -1500);
            //_mainWebViewPrefab.WebView
            _mainWebViewPrefab = WebViewPrefab.Instantiate(0.6f, 0.3f);
            _mainWebViewPrefab.transform.SetParent(transform);
            _mainWebViewPrefab.transform.localPosition = new Vector3(0, _mainWebViewPrefab.transform.localScale.y, 0.2f);
            _mainWebViewPrefab.transform.localRotation = Quaternion.Euler(0, 180f, 0);


            _controlsWebViewPrefab = WebViewPrefab.Instantiate(0.6f, 0.05f);
            _controlsWebViewPrefab.transform.parent = _mainWebViewPrefab.transform;
            _controlsWebViewPrefab.transform.localPosition = new Vector3(0, 0.06f, 0);
            _controlsWebViewPrefab.transform.localEulerAngles = Vector3.zero;



            // Set up a timer to allow the state of the back / forward buttons to be
            _buttonRefreshTimer.AutoReset = false;
            _buttonRefreshTimer.Interval = 1000;
            _buttonRefreshTimer.Elapsed += ButtonRefreshTimer_Elapsed;

            _setUpKeyboards();

            await Task.WhenAll(new Task[] {
               _mainWebViewPrefab.WaitUntilInitialized(),
               _controlsWebViewPrefab.WaitUntilInitialized()
            });

            _mainWebViewPrefab.ScrollingSensitivity = 0.05f;
            _mainWebViewPrefab.WebView.UrlChanged += MainWebView_UrlChanged;
            //DrmTest:https://bitmovin.com/demos/drm
            //WebXrTest: https://unboring.net/mr    https://www.acfun.cn/   https://www.twitch.tv/ https://mubi.com/showing
            _mainWebViewPrefab.WebView.LoadUrl("https://youtube.com");
            _mainWebViewPrefab.WebView.FocusedInputFieldChanged += MainWebView_FocusedInput;

            _mainWebViewPrefab.Clicked += async (sender, eventArgs) =>
            {
                if (this.gameObject.activeSelf)
                    StartCoroutine(GetFullScreenState());
                Debug.Log("_creentScreenState" + _creentScreenState);
            };

            _controlsWebViewPrefab.WebView.MessageEmitted += Controls_MessageEmitted;
            _controlsWebViewPrefab.WebView.LoadHtml(CONTROLS_HTML);

            init();
            //Android Gecko and UWP w / XR enabled don't support transparent webviews, so set the cutout
            //rect to the entire view so that the shader makes its black background pixels transparent.
            var pluginType = _controlsWebViewPrefab.WebView.PluginType;
            if (pluginType == WebPluginType.AndroidGecko || pluginType == WebPluginType.UniversalWindowsPlatform)
            {
                _controlsWebViewPrefab.SetCutoutRect(new Rect(0, 0, 1, 1));
            }
        }
        void init()
        {
            _realWebView = transform.Find("WebViewPrefab(Clone)/WebViewPrefabResizer/WebViewPrefabView").gameObject;
            _sphereMaterial = _realWebView.GetComponent<MeshRenderer>().material;
        }

        IEnumerator GetFullScreenState()
        {
            yield return new WaitForSeconds(0.2f);
            CompareScreenState();
        }

        public void UpdateWebViewPos()
        {
            if (MediaPlayerCtrl.Singleton.CurrentState == MediaplayerState.CLOSED)
            {
                GameObject _mainWebViewParent = GameObject.Find("SceneRoot/InterfaceRoot/PlayerCanvas/MainInterface/HomeRoot/InterfaceDirRoot");
                transform.SetParent(_mainWebViewParent.transform, true);
                transform.GetComponent<RectTransform>().offsetMax = new Vector2(GetComponent<RectTransform>().offsetMax.x, 0);
                transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                GameObject _mainWebViewParent = GameObject.Find("SceneRoot/TextureMesh");
                transform.SetParent(_mainWebViewParent.transform, true);
                transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 4.0f, 19f);
                transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }
        }
        async void CompareScreenState()
        {
            _creentScreenState = await _mainWebViewPrefab.WebView.ExecuteJavaScript("document.fullscreen");
            Debug.Log("_creentScreenState" + _creentScreenState);
            if (_creentScreenState == "true" || _creentScreenState == "false")
            {
                if (string.Compare(_creentScreenState, _lastScreenState) != 0)
                {
                    if (_creentScreenState == "true")
                    {
                        WebMediaFileInfo info = new WebMediaFileInfo();
                        MediaFileManager.Singleton.AddMediaFileInfo(ScrollViewType.Web, info);
                        InterfaceAnimation.Singleton.Load(info);
                    }
                    else
                    {
                        InterfaceAnimation.Singleton.BackToHome();
                    }
                    _lastScreenState = _creentScreenState;
                }
            }
            else
            {
                _creentScreenState = _lastScreenState;
            }
        }
        void MainWebView_FocusedInput(object sender, FocusedInputFieldChangedEventArgs fieldType)
        {
            if (fieldType.Type == FocusedInputFieldType.Text)
            {
                _keyBoard.gameObject.SetActive(true);

            }
            //�����㲻�ڵ�ǰ����ҳ��ͼʱ�����ؼ���
            else if (fieldType.Type == FocusedInputFieldType.None)
            {
                _keyBoard.gameObject.SetActive(false);
            }
        }

        void MainWebView_UrlChanged(object sender, UrlChangedEventArgs eventArgs)
        {

            _setDisplayedUrl(eventArgs.Url);
            _buttonRefreshTimer.Start();
        }

        void Controls_MessageEmitted(object sender, EventArgs<string> eventArgs)
        {

            if (eventArgs.Value == "CONTROLS_INITIALIZED")
            {
                // The controls UI won't be initialized in time to receive the first UrlChanged event,
                // so explicitly set the initial URL after the controls UI indicates it's ready.
                _setDisplayedUrl(_mainWebViewPrefab.WebView.Url);
                return;
            }
            var message = eventArgs.Value;
            if (message == "GO_BACK")
            {
                _mainWebViewPrefab.WebView.GoBack();
            }
            else if (message == "GO_FORWARD")
            {
                _mainWebViewPrefab.WebView.GoForward();
            }
        }

        void ButtonRefreshTimer_Elapsed(object sender, ElapsedEventArgs eventArgs)
        {

            // Get the main webview's back / forward state and then post a message
            // to the controls UI to update its buttons' state.
            Vuplex.WebView.Internal.Dispatcher.RunOnMainThread(async () =>
            {
                var canGoBack = await _mainWebViewPrefab.WebView.CanGoBack();
                var canGoForward = await _mainWebViewPrefab.WebView.CanGoForward();
                //C#---->JavaScript
                var serializedMessage = $"{{ \"type\": \"SET_BUTTONS\", \"canGoBack\": {canGoBack.ToString().ToLower()}, \"canGoForward\": {canGoForward.ToString().ToLower()} }}";
                _controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
            });
        }

        void _setDisplayedUrl(string url)
        {
            if (_controlsWebViewPrefab.WebView != null)
            {
                //C#---->JavaScript
                var serializedMessage = $"{{ \"type\": \"SET_URL\", \"url\": \"{url}\" }}";
                _controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
            }
        }

        async void _setUpKeyboards()
        {
            await _mainWebViewPrefab.WaitUntilInitialized();
            // Send keys from the hardware (USB or Bluetooth) keyboard to the webview.
            // Use separate KeyDown() and KeyUp() methods if the webview supports
            // it, otherwise just use IWebView.SendKey().
            // https://developer.vuplex.com/webview/IWithKeyDownAndUp

            var webViewWithKeyDownAndUp = _mainWebViewPrefab.WebView as IWithKeyDownAndUp;
            _hardwareKeyboardListener = HardwareKeyboardListener.Instantiate();
            _hardwareKeyboardListener.transform.SetParent(_mainWebViewPrefab.transform, false);
            _hardwareKeyboardListener.KeyDownReceived += (sender, eventArgs) =>
            {
                if (webViewWithKeyDownAndUp != null)
                {
                    webViewWithKeyDownAndUp.KeyDown(eventArgs.Value, eventArgs.Modifiers);
                }
                else
                {
                    _mainWebViewPrefab.WebView.SendKey(eventArgs.Value);
                }
            };
            _hardwareKeyboardListener.KeyUpReceived += (sender, eventArgs) =>
            {
                webViewWithKeyDownAndUp?.KeyUp(eventArgs.Value, eventArgs.Modifiers);
            };

            // Also add an on-screen keyboard under the main webview.
            _keyBoard = Keyboard.Instantiate(2.668f, 0.736f);
            _keyBoard.transform.SetParent(_mainWebViewPrefab.transform, false);
            _keyBoard.transform.localPosition = new Vector3(0f, -1.6f, 0.2f);
            _keyBoard.transform.localEulerAngles = Vector3.zero;
            _keyBoard.Resolution = 280f;
            _keyBoard.InputReceived += (sender, eventArgs) =>
            {
                _mainWebViewPrefab.WebView.SendKey(eventArgs.Value);
            };
            _keyBoard.gameObject.SetActive(false);
        }

        //JS����Controller����������ԣ�͸���ȡ���ɫ�ȡ�
        const string CONTROLS_HTML = @"
            <!DOCTYPE html>
            <html>
                <head>
                    <!-- This transparent meta tag instructs 3D WebView to allow the page to be transparent. -->
                    <meta name='transparent' content='true'>
                    <meta charset='UTF-8'>
                    <style>
                        body {
                            font-family: Helvetica, Arial, Sans-Serif;
                            margin: 0;
                            height: 100vh;
                            color: white;
                        }
                        .controls {
                            display: flex;
                            justify-content: space-between;
                            align-items: center;
                            height: 100%;
                        }
                        .controls > div {
                            background-color: #283237;
                            border-radius: 8px;
                            height: 100%;
                        }
                        .url-display {
                            flex: 0 0 75%;
                            width: 75%;
                            display: flex;
                            align-items: center;
                            overflow: hidden;
                        }
                        #url {
                            width: 100%;
                            white-space: nowrap;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            padding: 0 15px;
                            font-size: 18px;
                        }
                        .buttons {
                            flex: 0 0 20%;
                            width: 20%;
                            display: flex;
                            justify-content: space-around;
                            align-items: center;
                        }
                        .buttons > button {
                            font-size: 40px;
                            background: none;
                            border: none;
                            outline: none;
                            color: white;
                            margin: 0;
                            padding: 0;
                        }
                        .buttons > button:disabled {
                            color: rgba(255, 255, 255, 0.3);
                        }
                        .buttons > button:last-child {
                            transform: scaleX(-1);
                        }
                        /* For Gecko only, set the background color
                        to black so that the shader's cutout rect
                        can translate the black pixels to transparent.*/
                        @supports (-moz-appearance:none) {
                            body {
                                background-color: black;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class='controls'>
                        <div class='url-display'>
                            <div id='url'></div>
                        </div>
                        <div class='buttons'>
                            <button id='back-button' disabled='true' onclick='vuplex.postMessage(""GO_BACK"")'>��</button>
                            <button id='forward-button' disabled='true' onclick='vuplex.postMessage(""GO_FORWARD"")'>��</button>
                        </div>
                    </div>
                    <script>
                        // Handle messages sent from C#
                        function handleMessage(message) {
                            var data = JSON.parse(message.data);
                            if (data.type === 'SET_URL') {
                                document.getElementById('url').innerText = data.url;
                            } else if (data.type === 'SET_BUTTONS') {
                                document.getElementById('back-button').disabled = !data.canGoBack;
                                document.getElementById('forward-button').disabled = !data.canGoForward;
                            }
                        }

                        function attachMessageListener() {
                            window.vuplex.addEventListener('message', handleMessage);
                            window.vuplex.postMessage('CONTROLS_INITIALIZED');
                        }

                        if (window.vuplex) {
                            attachMessageListener();
                        } else {
                            window.addEventListener('vuplexready', attachMessageListener);
                        }
                    </script>
                </body>
            </html>
        ";
    }
}

