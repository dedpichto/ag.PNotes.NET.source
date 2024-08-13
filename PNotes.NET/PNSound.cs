// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;

namespace PNotes.NET
{
    internal class PNSound
    {
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern bool PlaySound(string pszSound, UIntPtr hmod, SoundFlag fdwSound);
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern bool PlaySound(byte[] pszSound, IntPtr hmod, SoundFlag fdwSound);

        [Flags]
        private enum SoundFlag : uint
        {
            //SND_SYNC = 0x0000,
            SndAsync = 0x0001,
            //SND_NODEFAULT = 0x0002,
            //SND_MEMORY = 0x0004,
            SndLoop = 0x0008,
            //SND_NOSTOP = 0x0010,
            //SND_NOWAIT = 0x00002000,
            //SND_ALIAS = 0x00010000,
            //SND_ALIAS_ID = 0x00110000,
            SndFilename = 0x00020000,
            //SND_RESOURCE = 0x00040004,
            //SND_PURGE = 0x0040,
            //SND_APPLICATION = 0x0080
        }

        internal static void PlaySound(string pszSound)
        {
            PlaySound(pszSound, UIntPtr.Zero, SoundFlag.SndFilename | SoundFlag.SndAsync);
        }

        internal static void PlaySound(byte[] pszSound)
        {
            PlaySound(pszSound, IntPtr.Zero, SoundFlag.SndFilename | SoundFlag.SndAsync);
        }

        internal static void PlaySoundInLoop(string pszSound)
        {
            PlaySound(pszSound, UIntPtr.Zero, SoundFlag.SndFilename | SoundFlag.SndAsync | SoundFlag.SndLoop);
        }

        internal static void PlaySoundInLoop(byte[] pszSound)
        {
            PlaySound(pszSound, IntPtr.Zero, SoundFlag.SndFilename | SoundFlag.SndAsync | SoundFlag.SndLoop);
        }

        internal static void PlayDefaultSound()
        {
            var resourceStream = Application.GetResourceStream(new Uri("sounds/type.wav",  UriKind.Relative));
            if (resourceStream == null) return;
            using (var player = new SoundPlayer(resourceStream.Stream))
                player.Play();
        }

        internal static void PlayDefaultSoundInLoop()
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri("sounds/type.wav", UriKind.Relative));
            if (streamResourceInfo == null) return;
            using (var player = new SoundPlayer(streamResourceInfo.Stream))
                player.PlayLooping();
        }

        internal static void PlayMailSound()
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri("sounds/receive.wav", UriKind.Relative));
            if (streamResourceInfo == null) return;
            using (var player = new SoundPlayer(streamResourceInfo.Stream))
                player.Play();
        }

        internal static void StopSound()
        {
            PlaySound(null, UIntPtr.Zero, SoundFlag.SndAsync);
        }


        internal static void PlayAlarmSound(string sound, bool loop)
        {
            try
            {
                if (sound == PNSchedule.DEF_SOUND)
                {
                    if (loop)
                    {
                        PlayDefaultSoundInLoop();
                    }
                    else
                    {
                        PlayDefaultSound();
                    }
                }
                else
                {
                    if (loop)
                    {
                        PlaySoundInLoop(PNPaths.Instance.SoundsDir + @"\" + sound + ".wav");
                    }
                    else
                    {
                        PlaySound(PNPaths.Instance.SoundsDir + @"\" + sound + ".wav");
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
