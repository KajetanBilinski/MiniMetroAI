
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

        for (int i = 0; i < city.LineCount; i++)
        {
            Line line = city.GetLine(i);

            if (line == null)
                continue;

            var lineSnapshot = new LineSnapshot
            {
                Index = line.Index,
                TrainCount = line.TrainCount,
                IsMothballed = line.IsMothballed
            };

            for (int j = 0; j < line.Count; j++)
            {
                Link link = line[j];

                if (link == null)
                    continue;

                Station start = link.GetStop(0).Station;
                Station end = link.GetStop(1).Station;

                if (start == null || end == null)
                    continue;

                lineSnapshot.Links.Add(new LinkSnapshot
                {
                    Id = link.Id,
                    StartStationId = start.Id,
                    EndStationId = end.Id
                });
            }

            state.Lines.Add(lineSnapshot);

            for (int j = 0; j < line.TrainCount; j++)
            {
                Train train = line.GetTrain(j);

                if (train == null)
                    continue;

                var trainSnapshot = new TrainSnapshot
                {
                    Id = train.Id,
                    LineIndex = line.Index,
                    RailcarCount = train.RailcarCount
                };

                for (int k = 0; k < train.RailcarCount; k++)
                {
                    Railcar railcar = train.GetRailcar(k);

                    if (railcar == null)
                        continue;

                    if (railcar.State == RailcarState.MOTHBALLED)
                        continue;

                    trainSnapshot.ActiveRailcarCount++;

                    trainSnapshot.PassengerCount += railcar.PeepCount;
                }

                lineSnapshot.Trains.Add(trainSnapshot);
            }

            state.TrainCount += lineSnapshot.Trains.Count;

        }


        return state;
    }
}
