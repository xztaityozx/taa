using System;
using System.Collections.Generic;
using ShellProgressBar;

namespace taa {
    public class MultiProgressBar : IDisposable {
        private readonly IProgressBar parentBar;
        private readonly Map<string, IProgressBar> childBars;

        public MultiProgressBar(string name) {
            parentBar = new ProgressBar(0, name, ConsoleColor.DarkGreen);
            childBars = new Map<string, IProgressBar>();
        }

        public MultiProgressBar(IProgressBar parent) => parentBar = parent;

        public void AddProgressBar(string name, int maxTick) {
            parentBar.MaxTicks++;
            childBars.Add(name,parentBar.Spawn(maxTick, name, new ProgressBarOptions {
                ForegroundColor = ConsoleColor.Green
            }));
        }

        public void Tick(string name, string message = "") {
            childBars[name].Tick(message);
        }

        public void Dispose() {
            foreach (var (_, v) in childBars) {
                v.Dispose();
            }

            parentBar.Dispose();
        }
    }
}