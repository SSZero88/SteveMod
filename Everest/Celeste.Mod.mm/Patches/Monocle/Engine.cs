﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;
using System;

namespace Monocle {
    class patch_Engine : Engine {

        public static int ViewWidth { get; private set; }
        public static void SetViewWidth(int value) => ViewWidth = value;

        public static int ViewHeight { get; private set; }
        public static void SetViewHeight(int value) => ViewHeight = value;

        public static Viewport Viewport { get; private set; }
        public static void SetViewport(Viewport value) => Viewport = value;

        public patch_Engine(int width, int height, int windowWidth, int windowHeight, string windowTitle, bool fullscreen)
            : base(width, height, windowWidth, windowHeight, windowTitle, fullscreen) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        [MonoModReplace]
        public new void RunWithLogging() {
            try {
                Run();
            } catch (Exception e) {
                e.LogDetailed();
                ErrorLog.Write(e);
                ErrorLog.Open();
            }
        }

    }
    public static class EngineExt {

        // Mods can't access patch_ classes directly.
        // We thus expose any new members through extensions.

        public static int ViewWidth {
            get {
                return Engine.ViewWidth;
            }
            set {
                patch_Engine.SetViewWidth(value);
            }
        }

        public static int ViewHeight {
            get {
                return Engine.ViewHeight;
            }
            set {
                patch_Engine.SetViewHeight(value);
            }
        }

        public static Viewport Viewport {
            get {
                return Engine.Viewport;
            }
            set {
                patch_Engine.SetViewport(value);
            }
        }

    }
}
