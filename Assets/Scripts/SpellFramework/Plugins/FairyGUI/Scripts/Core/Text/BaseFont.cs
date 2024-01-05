﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// Base class for all kind of fonts. 
    /// </summary>
    public class BaseFont
    {
        /// <summary>
        /// The name of this font object.
        /// </summary>
        public string name;

        /// <summary>
        /// The texture of this font object.
        /// </summary>
        public NTexture mainTexture;

        /// <summary>
        ///  Can this font be tinted? Will be true for dynamic font and fonts generated by BMFont.
        /// </summary>
        public bool canTint;

        /// <summary>
        /// If true, it will use extra vertices to enhance bold effect
        /// </summary>
        public bool customBold;

        /// <summary>
        /// If true, it will use extra vertices to enhance bold effect ONLY when it is in italic style.
        /// </summary>
        public bool customBoldAndItalic;

        /// <summary>
        /// If true, it will use extra vertices(4 direction) to enhance outline effect
        /// </summary>
        public bool customOutline;

        /// <summary>
        /// The shader for this font object.
        /// </summary>
        public string shader;

        /// <summary>
        /// Keep text crisp.
        /// </summary>
        public bool keepCrisp;

        /// <summary>
        /// 
        /// </summary>
        public int version;

        protected internal static bool textRebuildFlag;

        protected const float SupScale = 0.58f;
        protected const float SupOffset = 0.33f;

        virtual public void UpdateGraphics(NGraphics graphics)
        {
        }

        virtual public void SetFormat(TextFormat format, float fontSizeScale)
        {
        }

        virtual public void PrepareCharacters(string text)
        {
        }

        virtual public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            width = 0;
            height = 0;
            baseline = 0;
            return false;
        }

        virtual public int DrawGlyph(float x, float y,
            List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            return 0;
        }

        virtual public int DrawLine(float x, float y, float width, int fontSize, int type,
            List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            return 0;
        }

        virtual public bool HasCharacter(char ch)
        {
            return false;
        }

        virtual public int GetLineHeight(int size)
        {
            return 0;
        }

        virtual public void Dispose()
        {
        }
    }
}
