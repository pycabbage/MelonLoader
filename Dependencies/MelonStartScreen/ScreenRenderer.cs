﻿using MelonLoader.MelonStartScreen.NativeUtils;
using MelonLoader.MelonStartScreen.UI;
using System;
using UnityEngine;
using UnityEngine.CoreModule;
using UnityPlayer;

namespace MelonLoader.MelonStartScreen
{
    internal static class ScreenRenderer
    {
        private const float logoRatio = 1.2353f;

        private delegate void SetupPixelCorrectCoordinates(bool param_1); 

#pragma warning disable 0649
        #region m_SetupPixelCorrectCoordinates Signatures
        [NativeSignature(01, NativeSignatureFlags.X86, "55 8b ec 83 ec 60 56 e8 ?? ?? ?? ?? 8b f0 8b 45 08 50 8d 4d f0 51 e8", "2017.1.0")]
        [NativeSignature(02, NativeSignatureFlags.X86, "55 8b ec 83 ec 60 53 56 57 e8 ?? ?? ?? ?? ff 75 08 8b d8 8d 45 f0 50 e8", "2017.3.0", "2018.1.0")]
        [NativeSignature(03, NativeSignatureFlags.X86, "55 8b ec 83 ec 60 53 56 57 e8 ?? ?? ?? ?? 8b d8 e8 ?? ?? ?? ?? ff 75 08 8d 4d f0", "2019.1.0")]
        [NativeSignature(01, NativeSignatureFlags.X64, "48 89 5c 24 08 57 48 81 ec a0 00 00 00 8b d9 e8 ?? ?? ?? ?? 48 8b f8 e8", "2017.1.0")]
        #endregion
        private static SetupPixelCorrectCoordinates m_SetupPixelCorrectCoordinates;
#pragma warning restore 0649

        public static bool disabled = false;

        private static uint shouldCallWFLPAGT = 0;

        private static Mesh melonloaderversionTextmesh;
        private static ProgressBar progressBar;

        internal static void Init()
        {
            if (disabled)
                return;

            try
            {
                MelonDebug.Msg("Initializing UIStyleValues");
                UIStyleValues.Init();
                MelonDebug.Msg("UIStyleValues Initialized");

                if (UIConfig.VersionText.Enabled)
                {
                    TextGenerationSettings settings = new TextGenerationSettings();
                    settings.textAnchor = UIConfig.VersionText.Anchor;
                    settings.color = UIConfig.VersionText.TextColor;
                    settings.generationExtents = new Vector2(540, 47.5f);
                    settings.richText = UIConfig.VersionText.RichText;
                    settings.font = UIStyleValues.TextFont;
                    settings.pivot = new Vector2(0.5f, 0.5f);
                    settings.fontSize = UIConfig.VersionText.FontSize;
                    settings.fontStyle = UIConfig.VersionText.Style;
                    settings.verticalOverflow = VerticalWrapMode.Overflow;
                    settings.scaleFactor = UIConfig.VersionText.Scale;
                    settings.lineSpacing = UIConfig.VersionText.LineSpacing;
                    MelonDebug.Msg("TextGenerationSettings settings set");

                    string versionText = UIConfig.VersionText.Text;
                    versionText = versionText.Replace("<loaderName/>", (MelonLaunchOptions.Console.Mode == MelonLaunchOptions.Console.DisplayMode.LEMON) ? "LemonLoader" : "MelonLoader");
                    versionText = versionText.Replace("<loaderVersion/>", BuildInfo.Version);
                    versionText = versionText.Replace("LemonLoader", "<color=#FFCC4D>LemonLoader</color>");
                    versionText = versionText.Replace("MelonLoader", "<color=#78f764>Melon</color><color=#ff3c6a>Loader</color>");

                    melonloaderversionTextmesh = TextMeshGenerator.Generate(versionText, settings);
                }

                if (UIConfig.ProgressBar.Enabled || UIConfig.ProgressText.Enabled)
                    progressBar = new ProgressBar();
                //progressBar = new ProgressBar(width: 540, height: 36);

                uint graphicsDeviceType = SystemInfo.GetGraphicsDeviceType();
                MelonDebug.Msg("Graphics Device Type: " + graphicsDeviceType);
                shouldCallWFLPAGT = NativeSignatureResolver.IsUnityVersionOverOrEqual(MelonLoader.InternalUtils.UnityInformationHandler.EngineVersion.ToStringWithoutType(), new[] { "2020.2.7", "2020.3.0", "2021.1.0" })
                    && (graphicsDeviceType == /*DX11*/2 || graphicsDeviceType == /*DX12*/18)
                    ? graphicsDeviceType : 0;
            }
            catch (Exception e)
            {
                MelonLogger.Error("Exception while init rendering: " + e);
                disabled = true;
            }
        }

        internal static unsafe void Render()
        {
            if (disabled)
                return;

            try
            {
                m_SetupPixelCorrectCoordinates(false);

                int sw = Screen.width;
                int sh = Screen.height;

                int logoHeight = (int)(sh * 0.4f);
                int logoWidth = (int)(logoHeight * logoRatio);

                Graphics.DrawTexture(new Rect(0, 0, sw, sh), UIStyleValues.BackgroundTexture);

                if (UIStyleValues.BackgroundImage != null)
                    UIStyleValues.BackgroundImage.Render(0, sh, sw, -sh);

                if (UIConfig.LogoImage.AutoAlign)
                    UIStyleValues.LogoImage?.Render((sw - logoWidth) / 2, sh - ((sh - logoHeight) / 2 - 46), logoWidth, -logoHeight);
                else
                    UIStyleValues.LogoImage?.Render(UIConfig.LogoImage.CustomPosition.Item1, sh - UIConfig.LogoImage.CustomPosition.Item2, logoWidth, -logoHeight);


                if (UIConfig.LoadingImage.AutoAlign)
                    UIStyleValues.LoadingImage?.Render(sw - 200, 200, 132);
                else
                    UIStyleValues.LoadingImage?.Render(UIConfig.LoadingImage.CustomPosition.Item1, sh - UIConfig.LoadingImage.CustomPosition.Item2, 132);

                UIStyleValues.TextFont.material.SetPass(0);

                if (melonloaderversionTextmesh != null)
                {
                    if (UIConfig.VersionText.AutoAlign)
                        Graphics.DrawMeshNow(melonloaderversionTextmesh, new Vector3(sw / 2, sh - (sh / 2 + (logoHeight / 2) - 35), 0), Quaternion.identity);
                    else
                        Graphics.DrawMeshNow(melonloaderversionTextmesh, new Vector3(UIConfig.VersionText.CustomPosition.Item1, sh - UIConfig.VersionText.CustomPosition.Item2, 0), Quaternion.identity);
                }

                if (progressBar != null)
                {
                    int x, y, width, height = 0;
                    width = 540;
                    height = 36;

                    if (UIConfig.ProgressBar.AutoAlign)
                    {
                        x = (sw - 540) / 2;
                        y = sh - ((sh - 36) / 2 + (logoHeight / 2) + 50);
                    }
                    else
                    {
                        x = UIConfig.ProgressBar.CustomPosition.Item1;
                        y = UIConfig.ProgressBar.CustomPosition.Item2;
                    }
                    
                    progressBar.Render(x, y, width, height);
                }

                GfxDevice.PresentFrame();
                if (shouldCallWFLPAGT != 0)
                    GfxDevice.WaitForLastPresentationAndGetTimestamp(shouldCallWFLPAGT);
            }
            catch (Exception e)
            {
                MelonLogger.Error("Exception while rendering: " + e);
                disabled = true;
            }
        }

        internal static void UpdateMainProgress(string text, float progress)
        {
            if (progressBar == null)
                return;

            progressBar.text = text;
            progressBar.progress = progress;
        }

        internal static void UpdateProgressFromLog(string msg)
        {
            if (progressBar == null)
                return;

            progressBar.progress = ProgressParser.GetProgressFromLog(msg, ref progressBar.text, progressBar.progress);
        }

        internal static void UpdateProgressFromMod(string modname)
        {
            if (progressBar == null)
                return;

            progressBar.progress = ProgressParser.GetProgressFromMod(modname, ref progressBar.text);
        }

        internal static void UpdateProgressState(ModLoadStep step)
        {
            if (progressBar == null)
                return;

            progressBar.progress = ProgressParser.SetModState(step, ref progressBar.text);
        }
    }
}
