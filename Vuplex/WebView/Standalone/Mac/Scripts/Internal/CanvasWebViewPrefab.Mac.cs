// Copyright (c) 2022 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_STANDALONE_OSX
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    partial class CanvasWebViewPrefab {

        partial void OnInit() {

            if (_canvas?.renderMode == RenderMode.ScreenSpaceOverlay) {
                WebViewLogger.LogWarning("Unity's macOS player currently has a bug that sometimes prevents 3D WebView's external textures from appearing properly in a \"Screen Space - Overlay\" Canvas (https://issuetracker.unity3d.com/issues/external-texture-is-not-visible-in-player-slash-build-when-canvas-render-mode-is-set-to-screen-space-overlay). If you encounter this issue, please either switch the Canvas's render mode to \"Screen Space - Camera\" or add a script to temporarily resize the player's window with Screen.SetResolution().");
            }
        }
    }
}
#endif
