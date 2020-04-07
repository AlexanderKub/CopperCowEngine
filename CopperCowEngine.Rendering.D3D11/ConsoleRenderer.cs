// using SharpDX;
// using SharpDX.Direct2D1;
// using SharpDX.DirectWrite;
// using System;
// using System.Collections.Generic;
// using Factory = SharpDX.DirectWrite.Factory;
// using SharpDX.Mathematics.Interop;
//
// namespace CopperCowEngine.Rendering.D3D11
// {
//     internal class ConsoleRenderer
//     {
//         private D3D11RenderBackend _renderBackend;
//
//         #region Resources
//         private SolidColorBrush _historyColorBrush;
//         private SolidColorBrush _errorColorBrush;
//         private SolidColorBrush _commandColorBrush;
//
//         private TextFormat _textFormat;
//         private TextFormat _fpsTextFormat;
//         private TextLayout _historyTextLayout;
//         private TextLayout _textLayout;
//         private TextLayout _fpsTextLayout;
//         private TextLayout _caretTextLayout;
//
//         private RoundedRectangleGeometry _rectangleGeometry;
//         private Brush _solidColorBrush;
//         #endregion
//
//         private RenderTarget RenderTarget2D => _renderBackend.DisplayRef.RenderTarget2D;
//
//         private Factory FactoryDWrite => _renderBackend.DisplayRef.FactoryDWrite;
//
//         private float _drawCaretTime;
//
//         private List<IDisposable> _disposables = new List<IDisposable>();
//
//         public void Initialize(D3D11RenderBackend backend)
//         {
//             _renderBackend = backend;
//             if (RenderTarget2D == null) {
//                 return;
//             }
//
//             _historyColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, Color.White));
//             _errorColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, Color.Red));
//             _commandColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, Color.Yellow));
//
//             _textFormat = ToDispose(new TextFormat(FactoryDWrite, "Roboto", 15)
//             {
//                 TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near
//             });
//             _fpsTextFormat = ToDispose(new TextFormat(FactoryDWrite, "Roboto", 20)
//             {
//                 TextAlignment = TextAlignment.Trailing, ParagraphAlignment = ParagraphAlignment.Near
//             });
//
//             _rectangleGeometry = ToDispose(new RoundedRectangleGeometry(_renderBackend.DisplayRef.Factory2D, new RoundedRectangle()
//             {
//                 RadiusX = 0,
//                 RadiusY = 0,
//                 Rect = new RectangleF(0, 0, 500, 1200)
//             }));
//
//             _textLayout = ToDispose(new TextLayout(FactoryDWrite, "", _textFormat, 500, 20));
//             _historyTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", _textFormat, 500, 200));
//             _caretTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", _textFormat, 500, 20));
//
//             _solidColorBrush = ToDispose(new SolidColorBrush(RenderTarget2D, new Color(new Vector4(0.7f, 0.7f, 0.7f, 0.6f))));
//             _fpsTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", _fpsTextFormat, 100, 100));
//         }
//
//         public void UpdateTextLayout()
//         {
//             if (FactoryDWrite.IsDisposed)
//             {
//                 return;
//             }
//
//             if (!_renderBackend.EngineRef.GetEngineConsole.IsDirty)
//             {
//                 return;
//             }
//
//             _renderBackend.EngineRef.GetEngineConsole.IsDirty = true;
//
//             var console = _renderBackend.EngineRef.GetEngineConsole;
//
//             _textLayout?.Dispose();
//             _historyTextLayout?.Dispose();
//             _textLayout = ToDispose(new TextLayout(FactoryDWrite, ">" + console.CurrentCommand, _textFormat, 500, 20));
//             _historyTextLayout = ToDispose(new TextLayout(FactoryDWrite, console.FullLog, _textFormat, 500, 200));
//             _caretTextLayout?.Dispose();
//             _caretTextLayout = ToDispose(new TextLayout(FactoryDWrite, ">" + console.CarretLayout, _textFormat, 500, 20));
//         }
//
//         public void Draw()
//         {
//             if (!_renderBackend.EngineRef.GetEngineConsole.IsShownConsole) {
//                 if (!_renderBackend.EngineRef.GetEngineConsole.IsShownStatsMonitor) {
//                     return;
//                 }
//                 DrawStatsMonitor();
//                 return;
//             }
//             UpdateTextLayout();
//             DrawStatsMonitor();
//
//             //TODO: rect shell & error coloring
//             RenderTarget2D.FillGeometry(_rectangleGeometry, _solidColorBrush, null);
//             RenderTarget2D.DrawTextLayout(new Vector2(0, 0), _textLayout, _commandColorBrush, DrawTextOptions.None);
//             RenderTarget2D.DrawTextLayout(new Vector2(0, 20), _historyTextLayout, _historyColorBrush, DrawTextOptions.None);
//
//             _drawCaretTime += 1f * _renderBackend.EngineRef.Time.DeltaTime;
//             if (_drawCaretTime > 0.75)
//             {
//                 var c = (int)_caretTextLayout.Metrics.WidthIncludingTrailingWhitespace;
//                 RenderTarget2D.DrawRectangle(new RawRectangleF(c, 3, c, 20), _commandColorBrush);
//             }
//             if (_drawCaretTime > 1.2)
//             {
//                 _drawCaretTime = 0;
//             }
//         }
//
//         private void DrawStatsMonitor()
//         {
//             var txt = "NO PROFILED";
//             if (_renderBackend.EngineRef.Profiler.IsUpdated || _renderBackend.EngineRef.Statistics.IsUpdated)
//             {
//                 _fpsTextLayout?.Dispose();
//                 txt = _renderBackend.EngineRef.Profiler?.Report() ?? txt;
//                 txt += $"\nDrawcalls: {_renderBackend.EngineRef.Statistics.DrawCallsCount}";
//                 _fpsTextLayout = ToDispose(new TextLayout(FactoryDWrite, txt, _fpsTextFormat, 300, 100));
//             }
//             RenderTarget2D.DrawTextLayout(
//                 new Vector2((int)RenderTarget2D.Size.Width - 310, 60),
//                 _fpsTextLayout, _commandColorBrush, DrawTextOptions.None);
//         }
//
//         private T ToDispose<T>(T disposable) where T : IDisposable
//         {
//             _disposables.Add(disposable);
//             return disposable;
//         }
//
//         public void Dispose()
//         {
//             foreach (var item in _disposables) {
//                 item?.Dispose();
//             }
//             _disposables.Clear();
//             _disposables = null;
//         }
//     }
// }
