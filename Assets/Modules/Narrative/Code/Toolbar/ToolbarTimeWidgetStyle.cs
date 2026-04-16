using FieldDay.UI.Widgets;
using System;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class ToolbarTimeWidgetStyle : GuiCounter.Style {
        public Sprite[] Sprites;
        public ToolbarTimeChunk[] Chunks;

        public override void Populate(in int data, GuiWidgetUpdateFlags flags) {
            int counter = data;
            int maxPerChunk = Sprites.Length - 1;
            for(int i = 0; i < Chunks.Length; i++) {
                int display = Math.Max(0, Math.Min(maxPerChunk, counter));
                counter -= maxPerChunk;
                Chunks[i].Display.sprite = Sprites[display];
            }
        }
    }
}