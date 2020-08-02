﻿using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod {
    public static class Extensions {

        /// <summary>
        /// Create a new MemoryStream for a given ZipEntry, which is safe to use in outside contexts.
        /// </summary>
        /// <param name="entry">The input ZipEntry.</param>
        /// <returns>The MemoryStream holding the extracted data of the ZipEntry.</returns>
        public static MemoryStream ExtractStream(this ZipEntry entry) {
            MemoryStream ms = new MemoryStream();
            entry.Extract(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <summary>
        /// Create a hexadecimal string for the given bytes.
        /// </summary>
        /// <param name="data">The input bytes.</param>
        /// <returns>The output hexadecimal string.</returns>
        public static string ToHexadecimalString(this byte[] data)
            => BitConverter.ToString(data).Replace("-", string.Empty);

        /// <summary>
        /// Invokes all delegates in the invocation list, passing on the result to the next.
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="md">The multicast delegate.</param>
        /// <param name="val">The initial value and first parameter.</param>
        /// <param name="args">Any other arguments that may be passed.</param>
        /// <returns>The result of all delegates.</returns>
        public static T InvokePassing<T>(this MulticastDelegate md, T val, params object[] args) {
            if (md == null)
                return val;

            object[] args_ = new object[args.Length + 1];
            args_[0] = val;
            Array.Copy(args, 0, args_, 1, args.Length);

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++)
                args_[0] = ds[i].DynamicInvoke(args_);

            return (T) args_[0];
        }

        /// <summary>
        /// Invokes all delegates in the invocation list, as long as the previously invoked delegate returns true.
        /// </summary>
        public static bool InvokeWhileTrue(this MulticastDelegate md, params object[] args) {
            if (md == null)
                return true;

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++)
                if (!((bool) ds[i].DynamicInvoke(args)))
                    return false;

            return true;
        }

        /// <summary>
        /// Invokes all delegates in the invocation list, as long as the previously invoked delegate returns false.
        /// </summary>
        public static bool InvokeWhileFalse(this MulticastDelegate md, params object[] args) {
            if (md == null)
                return false;

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++)
                if ((bool) ds[i].DynamicInvoke(args))
                    return true;

            return false;
        }

        /// <summary>
        /// Invokes all delegates in the invocation list, as long as the previously invoked delegate returns null.
        /// </summary>
        public static T InvokeWhileNull<T>(this MulticastDelegate md, params object[] args) where T : class {
            if (md == null)
                return null;

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++) {
                T result = (T) ds[i].DynamicInvoke(args);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Split PascalCase words to become Pascal Case instead.
        /// </summary>
        /// <param name="input">PascalCaseString</param>
        /// <returns>Pascal Case String</returns>
        public static string SpacedPascalCase(this string input) {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                if (i > 0 && char.IsUpper(c))
                    builder.Append(' ');
                builder.Append(c);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Escape some common strings from a given string for usage with the Dialog class.
        /// The following characters get replaced with an underscore: /-+
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The Dialog-compatible key.</returns>
        public static string DialogKeyify(this string input)
            => input.Replace('/', '_').Replace('-', '_').Replace('+', '_');

        /// <summary>
        /// Get the dialog string for the given input key.
        /// </summary>
        /// <param name="input">The dialog key.</param>
        /// <returns>The resolved dialog string.</returns>
        public static string DialogClean(this string input, Language language = null)
            => Dialog.Clean(input, language);

        /// <summary>
        /// Get the dialog string for the given input key.
        /// </summary>
        /// <param name="input">The dialog key.</param>
        /// <returns>The resolved dialog string or null.</returns>
        public static string DialogCleanOrNull(this string input, Language language = null) {
            if (Dialog.Has(input, language))
                return Dialog.Clean(input, language);
            else
                return null;
        }

        /// <summary>
        /// Get a Vector2 from any float[] with a length of 2.
        /// </summary>
        /// <param name="a">The input array.</param>
        /// <returns>The output Vector2 or null if the length doesn't match.</returns>
        public static Vector2? ToVector2(this float[] a) {
            if (a == null || a.Length != 2)
                return null;
            return new Vector2(a[0], a[1]);
        }

        /// <summary>
        /// Get a Vector3 from any float[] with a length of 3.
        /// </summary>
        /// <param name="a">The input array.</param>
        /// <returns>The output Vector3 or null if the length doesn't match.</returns>
        public static Vector3? ToVector3(this float[] a) {
            if (a == null || a.Length != 3)
                return null;
            return new Vector3(a[0], a[1], a[2]);
        }

        /// <summary>
        /// Add an Enter and Leave handler, notifying the user that a relaunch is required to apply the changes.
        /// </summary>
        /// <param name="option">The input TextMenu.Item option.</param>
        /// <param name="needsRelaunch">This method does nothing if this is set to false.</param>
        /// <returns>The passed option.</returns>
        public static TextMenu.Item NeedsRelaunch(this TextMenu.Item option, bool needsRelaunch = true) {
            if (!needsRelaunch)
                return option;
            return option
            .Enter(() => {
                // TODO: Show "needs relaunch" warning.
            })
            .Leave(() => {
                // TODO: Hide "needs relaunch" warning.
            });
        }

        // Celeste already ships with this.
        /*
        public static string ReadNullTerminatedString(this BinaryReader stream) {
            string text = "";
            char c;
            while ((c = stream.ReadChar()) > '\0') {
                text += c.ToString();
            }
            return text;
        }
        */

        /// <summary>
        /// Write the string to the BinaryWriter in a C-friendly format.
        /// </summary>
        /// <param name="stream">The output which the method writes to.</param>
        /// <param name="text">The input string.</param>
        public static void WriteNullTerminatedString(this BinaryWriter stream, string text) {
            if (text != null) {
                for (int i = 0; i < text.Length; i++) {
                    char c = text[i];
                    stream.Write(c);
                }
            }
            stream.Write('\0');
        }

        /// <summary>
        /// Cast a delegate from one type to another. Compatible with delegates holding an invocation list (combined delegates).
        /// </summary>
        /// <param name="source">The input delegate.</param>
        /// <param name="type">The wanted output delegate type.</param>
        /// <returns>The output delegate.</returns>
        public static Delegate CastDelegate(this Delegate source, Type type) {
            if (source == null)
                return null;
            Delegate[] delegates = source.GetInvocationList();
            if (delegates.Length == 1)
                return Delegate.CreateDelegate(type, delegates[0].Target, delegates[0].Method);
            Delegate[] delegatesDest = new Delegate[delegates.Length];
            for (int i = 0; i < delegates.Length; i++)
                delegatesDest[i] = delegates[i].CastDelegate(type);
            return Delegate.Combine(delegatesDest);
        }

        /// <summary>
        /// Map the list of buttons to the given virtual button.
        /// </summary>
        /// <param name="vbtn">The virtual button to map the buttons to.</param>
        /// <param name="buttons">The buttons to map.</param>
        public static void AddButtons(this VirtualButton vbtn, List<Buttons> buttons) {
            foreach (Buttons btn in buttons) {
                if (btn == Buttons.LeftTrigger) {
                    vbtn.Nodes.Add(new VirtualButton.PadLeftTrigger(Input.Gamepad, 0.25f));
                    continue;
                }

                if (btn == Buttons.RightTrigger) {
                    vbtn.Nodes.Add(new VirtualButton.PadRightTrigger(Input.Gamepad, 0.25f));
                    continue;
                }

                vbtn.Nodes.Add(new VirtualButton.PadButton(Input.Gamepad, btn));
            }
        }

        /// <summary>
        /// Is the given touch state "down" (pressed or moved)?
        /// </summary>
        public static bool IsDown(this TouchLocationState state)
            => state == TouchLocationState.Pressed || state == TouchLocationState.Moved;

        /// <summary>
        /// Is the given touch state "up" (released or invalid)?
        /// </summary>
        public static bool IsUp(this TouchLocationState state)
            => state == TouchLocationState.Released || state == TouchLocationState.Invalid;

    }
}