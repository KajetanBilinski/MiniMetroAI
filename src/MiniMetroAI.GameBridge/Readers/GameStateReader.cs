
using System;
using MiniMetroAI.GameBridge.Models;

namespace MiniMetroAI.GameBridge.Readers;

internal static class GameStateReader
{
    public static GameStateSnapshot Read(Game game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        City city = game.City;

        if (city == null)
            throw new InvalidOperationException(
                "Game city is not initialized.");

        var state = new GameStateSnapshot
        {
            Score = game.Score,
            IsPaused = game.IsPaused,
            IsGameOver = game.IsOver,

            Mode = game.Mode.ToString(),

            GameTime = city.Clock.Time,
            Day = city.Clock.Day,

            LineCount = city.LineCount,
            StationCount = city.StationCount
        };

        for (int i = 0; i < city.StationCount; i++)
        {
            Station station = city.GetStation(i);

            if (station == null)
                continue;

            var position = station.Position;

            state.Stations.Add(new StationSnapshot
            {
                Id = station.Id,
                Type = station.Type.ToString(),

                X = position.x,
                Y = position.y,

                WaitingPassengers = station.PeepCount,
                Capacity = station.PeepCapacity,

                LineCount = station.LineCount,

                IsInterchange = station.IsInterchange,
                IsOvercrowded = station.HasOvercrowded,

                OvercrowdingProgress =
                    station.ExpiryTimerCompletion
            });
        }

        return state;
    }
}
