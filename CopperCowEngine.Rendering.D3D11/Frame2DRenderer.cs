using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering.Data;
using SharpDX.Mathematics.Interop;
using Factory = SharpDX.DirectWrite.Factory;

namespace CopperCowEngine.Rendering.D3D11
{
    internal class Frame2DRenderer
    {
        private D3D11RenderBackend _renderBackend;

        #region Resources
        private SolidColorBrush _commandColorBrush;

        private TextFormat _textFormat;
        private TextLayout _textLayout;
        private TextLayout _caretTextLayout;

        private RoundedRectangleGeometry _rectangleGeometry;
        private Brush _solidColorBrush;
        #endregion

        private RenderTarget RenderTarget2D => _renderBackend.DisplayRef.RenderTarget2D;

        private Factory FactoryDWrite => _renderBackend.DisplayRef.FactoryDWrite;

        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Initialize(D3D11RenderBackend backend)
        {
            _renderBackend = backend;
            if (RenderTarget2D == null) {
                return;
            }

            _commandColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, Color.Yellow));

            _textFormat = ToDispose(new TextFormat(FactoryDWrite, "Roboto", 15)
            {
                TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near
            });

            _rectangleGeometry = ToDispose(new RoundedRectangleGeometry(_renderBackend.DisplayRef.Factory2D, new RoundedRectangle()
            {
                RadiusX = 0,
                RadiusY = 0,
                Rect = new RectangleF(0, 0, 500, 1200)
            }));

            _textLayout = ToDispose(new TextLayout(FactoryDWrite, "", _textFormat, 500, 20));
            _caretTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", _textFormat, 500, 20));
            _solidColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, new Color(new Vector4(0.12f, 0.12f, 0.12f, 0.75f))));
        }

        public void Draw(Standard2DFrameData frameData)
        {
            foreach (var textInput2d in frameData.TextInputElements)
            {
                _rectangleGeometry?.Dispose();
                _rectangleGeometry = new RoundedRectangleGeometry(_renderBackend.DisplayRef.Factory2D, new RoundedRectangle()
                {
                    RadiusX = 5,
                    RadiusY = 5,
                    Rect = new RectangleF(textInput2d.Position.X - 5, textInput2d.Position.Y - 2, 505, 24)
                });
                RenderTarget2D.FillGeometry(_rectangleGeometry, _solidColorBrush, null);

                _textLayout?.Dispose();
                _textLayout = new TextLayout(FactoryDWrite, textInput2d.Value, _textFormat, 500, 20);
                RenderTarget2D.DrawTextLayout(new RawVector2(textInput2d.Position.X, textInput2d.Position.Y), 
                    _textLayout, _commandColorBrush, DrawTextOptions.None);

                
                if (DateTime.Now.Millisecond % 1000 >= 500)
                {
                    _caretTextLayout?.Dispose();
                    _caretTextLayout = ToDispose(new TextLayout(FactoryDWrite, 
                        textInput2d.Value.Substring(0, textInput2d.CaretPosition), _textFormat, 500, 20));

                    var c = textInput2d.Position.X + (int)_caretTextLayout.Metrics.WidthIncludingTrailingWhitespace;
                    var y = textInput2d.Position.Y;
                    var h = _caretTextLayout.Metrics.Height;
                    RenderTarget2D.DrawRectangle(new RawRectangleF(c, y, c, y + h), _commandColorBrush);
                }
            }

            foreach (var text2d in frameData.TextElements)
            {
                _textLayout?.Dispose();
                _textLayout = new TextLayout(FactoryDWrite, text2d.Value, _textFormat, 500, 20);
                RenderTarget2D.DrawTextLayout(new RawVector2(text2d.Position.X, text2d.Position.Y), 
                    _textLayout, _commandColorBrush, DrawTextOptions.None);
            }
        }

        private T ToDispose<T>(T disposable) where T : IDisposable
        {
            _disposables.Add(disposable);
            return disposable;
        }

        public void Dispose()
        {
            foreach (var item in _disposables) 
            {
                item?.Dispose();
            }
            
            _rectangleGeometry?.Dispose();
            _textLayout?.Dispose();
            _caretTextLayout?.Dispose();

            _disposables.Clear();
            _disposables = null;
        }
    }
}
