
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MiniMetroAI.GameBridge.Patches;

namespace MiniMetroAI.GameBridge.Commands;

internal sealed class CommandResult
{
    public int StatusCode { get; }
    public string Json { get; }

    public CommandResult(int statusCode, string json)
    {
        StatusCode = statusCode;
        Json = json;
    }
}

internal sealed class PauseCommand
{
    private int _cancelled;

    public bool Paused { get; }

    public bool IsCancelled =>
        Volatile.Read(ref _cancelled) == 1;

    public TaskCompletionSource<CommandResult> Completion { get; }
        = new TaskCompletionSource<CommandResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    public PauseCommand(bool paused)
    {
        Paused = paused;
    }

    public void Cancel()
    {
        Interlocked.Exchange(ref _cancelled, 1);
    }
}

internal static class GameCommandQueue
{
    private static readonly ConcurrentQueue<PauseCommand> Queue
        = new ConcurrentQueue<PauseCommand>();

    public static PauseCommand EnqueuePause(bool paused)
    {
        var command = new PauseCommand(paused);
        Queue.Enqueue(command);

        return command;
    }

    public static void ProcessPending()
    {
        while (Queue.TryDequeue(out var command))
        {
            if (command.IsCancelled)
                continue;

            try
            {
                var main = Main.Instance;
                var game = GameUpdatePatch.CurrentGame;

                if (main == null ||
                    !main.IsInGame ||
                    game == null ||
                    game.City == null)
                {
                    command.Completion.TrySetResult(
                        new CommandResult(
                            409,
                            "{\"error\":\"no_active_game\"}"));

                    continue;
                }

                if (game.IsPaused != command.Paused)
                {
                    game.HudScreen.ClockWidget.TogglePause();
                }

                bool success = game.IsPaused == command.Paused;

                command.Completion.TrySetResult(
                    new CommandResult(
                        success ? 200 : 409,
                        success
                            ? "{\"paused\":" +
                              (game.IsPaused ? "true" : "false") +
                              "}"
                            : "{\"error\":\"pause_state_not_changed\"}"));
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError(ex);

                command.Completion.TrySetResult(
                    new CommandResult(
                        500,
                        "{\"error\":\"command_failed\"}"));
            }
        }
    }
}
