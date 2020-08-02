﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monocle {
    class patch_Commands : Commands {

        // We're effectively in Commands, but still need to "expose" private fields to our mod.
        private bool canOpen;
        private KeyboardState currentState;
        private bool underscore;
        private string currentText = "";
        private List<patch_Line> drawCommands;

        private int mouseScroll;
        private int cursorScale;

        [MonoModReplace] // Don't create orig_ method.
        internal void UpdateClosed() {
            if (!canOpen) {
                canOpen = true;
            // Original code only checks OemTillde and Oem8, leaving QWERTZ users in the dark...
            } else if (MInput.Keyboard.Pressed(Keys.OemTilde, Keys.Oem8, Keys.OemPeriod)) {
                Open = true;
                currentState = Keyboard.GetState();
            }

            // Execute F-key actions.
            for (int i = 0; i < FunctionKeyActions.Length; i++)
                if (MInput.Keyboard.Pressed(Keys.F1 + i))
                    ExecuteFunctionKeyAction(i);
        }

        internal void DrawCursor(Vector2 position, int scale, Color color) {
            for (int i = -scale / 2; i <= scale / 2; i++) {
                Draw.Line(position.X - 4f * scale, position.Y + i, position.X - 2f * scale, position.Y + i, color);
                Draw.Line(position.X + 2f * scale - 1f, position.Y + i, position.X + 4f * scale - 1f, position.Y + i, color);
                Draw.Line(position.X + i, position.Y - 4f * scale + 1f, position.X + i, position.Y - 2f * scale + 1f, color);
                Draw.Line(position.X + i, position.Y + 2f * scale, position.X + i, position.Y + 4f * scale, color);
            }
            Draw.Line(position.X - 3f, position.Y, position.X + 2f, position.Y, color);
            Draw.Line(position.X, position.Y - 2f, position.X, position.Y + 3f, color);
        }

        [MonoModReplace]
        internal void Render() {
            int viewWidth = Engine.ViewWidth;
            int viewHeight = Engine.ViewHeight;

            // Vector2 mousePosition = MInput.Mouse.Position;
            // For whatever reason, MInput.Mouse.Position keeps returning 0, 0
            // Let's just use the XNA / FNA MouseState instead.
            MouseState mouseState = Mouse.GetState();
            int mouseScrollDelta = mouseState.ScrollWheelValue - mouseScroll;
            mouseScroll = mouseState.ScrollWheelValue;
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Vector2? mouseSnapPosition = null;

            int viewScale = 1;

            string mouseText = $"Cursor @\n screen: {(int) Math.Round(mousePosition.X)}, {(int) Math.Round(mousePosition.Y)}";

            if (Engine.Scene is Level) {
                Level level = (Level) Engine.Scene;
                Camera cam = level.Camera;
                viewScale = (int) Math.Round(Engine.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth / (float) cam.Viewport.Width);
                Vector2 mouseWorldPosition = mousePosition;
                // Convert screen to world position.
                mouseWorldPosition = Calc.Floor(mouseWorldPosition / viewScale);
                mouseWorldPosition = cam.ScreenToCamera(mouseWorldPosition);
                mouseWorldPosition -= level.LevelOffset;
                mouseText += $"\n level:       {(int) Math.Round(mouseWorldPosition.X)}, {(int) Math.Round(mouseWorldPosition.Y)}";
                // Convert world to world-snap position.
                mouseSnapPosition = mouseWorldPosition;
                mouseSnapPosition = Calc.Floor(mouseSnapPosition.Value / 8f);
                mouseText += $"\n level, /8:   {(int) Math.Round(mouseSnapPosition.Value.X)}, {(int) Math.Round(mouseSnapPosition.Value.Y)}";
                mouseSnapPosition = 8f * mouseSnapPosition;
                mouseText += $"\n level, snap: {(int) Math.Round(mouseSnapPosition.Value.X)}, {(int) Math.Round(mouseSnapPosition.Value.Y)}";
                // Convert world-snap to screen-snap position.
                mouseSnapPosition += new Vector2(4f, 4f); // Center the cursor on the tile.
                mouseSnapPosition += level.LevelOffset;
                mouseSnapPosition = cam.CameraToScreen(mouseSnapPosition.Value);
                mouseSnapPosition *= viewScale;
            }

            Draw.SpriteBatch.Begin();

            // Draw cursor below all other UI.
            if (mouseScrollDelta < 0)
                cursorScale--;
            else if (mouseScrollDelta > 0)
                cursorScale++;
            cursorScale = Calc.Clamp(cursorScale, 1, viewScale);
            if (mouseSnapPosition != null)
                DrawCursor(mouseSnapPosition.Value, cursorScale, Color.Red);
            DrawCursor(mousePosition, cursorScale, Color.Yellow);

            // Draw cursor world position.
            Vector2 mouseTextSize = Draw.DefaultFont.MeasureString(mouseText);
            Draw.Rect(10f, 10f, mouseTextSize.X + 20f, mouseTextSize.Y + 20f, Color.Black * 0.8f);
            Draw.SpriteBatch.DrawString(
                Draw.DefaultFont,
                mouseText,
                new Vector2(20f, 20f),
                Color.White
            );

            // Draw standard console.

            Draw.Rect(10f, viewHeight - 50f, viewWidth - 20f, 40f, Color.Black * 0.8f);

            if (underscore) {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont,
                    ">" + currentText + "_",
                    new Vector2(20f, viewHeight - 42f),
                    Color.White
                );
            } else {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont,
                    ">" + currentText,
                    new Vector2(20f, viewHeight - 42f),
                    Color.White
                );
            }

            if (drawCommands.Count > 0) {
                float height = 10f + 30f * drawCommands.Count;
                Draw.Rect(10f, viewHeight - height - 60f, viewWidth - 20f, height, Color.Black * 0.8f);
                for (int i = 0; i < drawCommands.Count; i++) {
                    Draw.SpriteBatch.DrawString(
                        Draw.DefaultFont,
                        drawCommands[i].Text,
                        new Vector2(20f, viewHeight - 92f - 30f * i),
                        drawCommands[i].Color
                    );
                }
            }

            Draw.SpriteBatch.End();
        }

        // Only required to be defined so that we can access it.
        [MonoModIgnore]
        private struct patch_Line {
            public string Text;
            public Color Color;
            public patch_Line(string text) {
                Text = text;
                Color = Color.White;
            }
            public patch_Line(string text, Color color) {
                Text = text;
                Color = color;
            }
        }

    }
}
