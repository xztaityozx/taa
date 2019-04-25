using System;
using System.Collections.Generic;
using NLua;
using System.Linq;

namespace taa {
    public class CounterLua : IDisposable {
        private readonly Lua lua;
        private int index;

        public CounterLua() {
            lua=new Lua();
            index = 0;

            lua.DoString("box={}");
        }

        public void AddStatus(string s) {
            lua.DoString($"box[{index}]={s}");
            index++;
        }

        public IEnumerable<bool> GetResults() {
            var box = (LuaTable) lua["box"];
            return Enumerable.Range(0, index).Select(i => (bool) box[i]);
        }

        public bool this[int idx] => (bool) lua.DoString($"return box[{idx}]")[0];

        public void Dispose() {
            lua?.Dispose();
        }
    }
}