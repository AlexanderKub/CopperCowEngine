using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CopperCowEngine.Rendering.Data
{
    public struct Text2D
    {
        public Vector2 Position;
        public string Value;
    }

    public struct TextInput2D
    {
        public Vector2 Position;
        public string Value;
        public int CaretPosition;
    }

    public class Standard2DFrameData : Frame2DData
    {
        public readonly List<Text2D> TextElements;
        public readonly List<TextInput2D> TextInputElements;

        public Standard2DFrameData()
        {
            TextElements = new List<Text2D>();
            TextInputElements = new List<TextInput2D>();
        }

        public void AddText(string value, Vector2 position)
        {
            TextElements.Add(new Text2D
            {
                Position = position,
                Value = value,
            });
        }

        public void AddInputText(string value, Vector2 position, int caretPosition)
        {
            TextInputElements.Add(new TextInput2D
            {
                Position = position,
                Value = value,
                CaretPosition = caretPosition,
            });
        }

        public override void Reset()
        {
            TextElements.Clear();
            TextInputElements.Clear();
        }

        public override void Finish()
        {
            
        }
    }
}
