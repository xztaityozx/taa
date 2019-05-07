using System;
using System.Collections;
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

            var rt = new List<bool>();

            for (var i = 0; i < index; i++) {
                rt.Add((bool)box[i]);
            }

            return rt;
        }

        public bool this[int idx] => (bool) lua.DoString($"return box[{idx}]")[0];

        public void Dispose() {
            lua?.Dispose();
        }
    }
}