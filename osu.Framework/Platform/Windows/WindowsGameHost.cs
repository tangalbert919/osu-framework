﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using osu.Framework.Platform.Windows.Native;
using osuTK;

namespace osu.Framework.Platform.Windows
{
    public class WindowsGameHost : DesktopGameHost
    {
        private TimePeriod timePeriod;

        public override Clipboard GetClipboard() => new WindowsClipboard();

        public override string UserStoragePath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public override bool CapsLockEnabled => Console.CapsLock;

        internal WindowsGameHost(string gameName, bool bindIPC = false, ToolkitOptions toolkitOptions = default, bool portableInstallation = false, bool useSdl = false)
            : base(gameName, bindIPC, toolkitOptions, portableInstallation, useSdl)
        {
        }

        public override void OpenFileExternally(string filename)
        {
            if (Directory.Exists(filename))
            {
                Process.Start("explorer.exe", filename);
                return;
            }

            base.OpenFileExternally(filename);
        }

        protected override void SetupForRun()
        {
            base.SetupForRun();

            // OnActivate / OnDeactivate may not fire, so the initial activity state may be unknown here.
            // In order to be certain we have the correct activity state we are querying the Windows API here.

            timePeriod = new TimePeriod(1) { Active = true };
        }

        protected override IWindow CreateWindow() =>
            !UseSdl ? (IWindow)new WindowsGameWindow() : new DesktopWindow();

        protected override void Dispose(bool isDisposing)
        {
            timePeriod?.Dispose();
            base.Dispose(isDisposing);
        }

        protected override void OnActivated()
        {
            timePeriod.Active = true;

            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous | Execution.ExecutionState.SystemRequired | Execution.ExecutionState.DisplayRequired);
            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            timePeriod.Active = false;

            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous);
            base.OnDeactivated();
        }
    }
}
