using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class ConsoleSystem : ComponentlessSystem
    {
        protected override void OnCreate()
        {
            ref var consoleState = ref Context.GetSingletonComponent<ConsoleState>();
            consoleState.CommandHistory = new List<string>();
            consoleState.LogLines = new List<string>();
            consoleState.CommandHistoryIndex = -1;
            Debug.OnDebugLog += Debug_OnDebugLog;
        }

        protected override void Update()
        {
            ref var input = ref Context.GetSingletonComponent<InputSingleton>();
            ref var consoleState = ref Context.GetSingletonComponent<ConsoleState>();

            if (input.IsButtonPressed(Buttons.Tilde))
            {
                consoleState.IsShow = !consoleState.IsShow;
                input.InputStringHolder.SetFocus(consoleState.IsShow);
                consoleState.CommandHistoryIndex = consoleState.CommandHistory.Count;
            }

            if (!consoleState.IsShow)
            {
                return;
            }
            HandleCommandsHistory(ref input, ref consoleState);

            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;
            var frame2DData = (Standard2DFrameData)engine.Rendering2DFrameData;
            var commandLine = input.InputStringHolder.GetString();
            var caretPosition = input.InputStringHolder.CaretIndex;

            if (input.IsButtonPressed(Buttons.Submit))
            {
                if (!string.IsNullOrEmpty(commandLine) && engine.ScriptEngine.ExecuteScriptCommand(commandLine))
                {
                    input.InputStringHolder.Clear();
                    consoleState.CommandHistory.Add(commandLine);
                    consoleState.CommandHistoryIndex = consoleState.CommandHistory.Count;
                }
            }

            var fps = 1f / Time.Delta;
            var x = Screen.Width - 250f;
            frame2DData.AddText($"FPS: {MathF.Round(fps)} DeltaTime: {Time.Delta * 1000} ms", new Vector2(x, 0));
            frame2DData.AddText($"DrawCalls: {Statistics.DrawCallsCount}", new Vector2(x, 20));
            frame2DData.AddText($"HDR: {engine.Configuration.Rendering.Configuration.EnableHdr}", new Vector2(x, 60));
            frame2DData.AddText($"BLOOM: {engine.Configuration.Rendering.Configuration.PostProcessing.Bloom.Enable}", new Vector2(x, 80));
            frame2DData.AddText($"MSAA: {engine.Configuration.Rendering.Configuration.EnableMsaa.ToString().ToLower()}", new Vector2(x, 100));

            x = 15;
            var y = 10;
            frame2DData.AddInputText($"$: {commandLine}", new Vector2(x, y), caretPosition + 3);

            y += 20;
            // TODO: Auto layout system + batch + scroll
            var lines = consoleState.LogLines.ToArray().Reverse();
            frame2DData.AddText(string.Join("\n", lines), new Vector2(x, y));
        }

        protected override void OnDestroy()
        {
            Debug.OnDebugLog -= Debug_OnDebugLog;
        }

        private void HandleCommandsHistory(ref InputSingleton input, ref ConsoleState consoleState)
        {
            if (consoleState.CommandHistory.Count > 0)
            {
                if (input.IsButtonPressed(Buttons.ArrowUp))
                {
                    var historyIndex = consoleState.CommandHistoryIndex;
                    historyIndex = historyIndex > 1 ? historyIndex - 1 : 0;
                    consoleState.CommandHistoryIndex = historyIndex;
                    input.InputStringHolder.SetString(consoleState.CommandHistory[consoleState.CommandHistoryIndex]);
                }

                if (input.IsButtonPressed(Buttons.ArrowDown))
                {
                    var historyIndex = consoleState.CommandHistoryIndex;
                    var maxIndex = consoleState.CommandHistory.Count - 1;
                    historyIndex = historyIndex < maxIndex - 1 ? historyIndex + 1 : maxIndex;
                    consoleState.CommandHistoryIndex = historyIndex;

                    input.InputStringHolder.SetString(consoleState.CommandHistory[consoleState.CommandHistoryIndex]);
                }
            }
        }
    
        private void Debug_OnDebugLog(string line)
        {
            ref var consoleState = ref Context.GetSingletonComponent<ConsoleState>();
            consoleState.LogLines.Add(line);
        }
    }
}