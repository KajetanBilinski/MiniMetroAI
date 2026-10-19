
using System.Globalization;

namespace MiniMetroAI.GameBridge.Readers;

internal static class StationReader
{
    public static void Log(Game game)
    {
        var state = GameStateReader.Read(game);

        Plugin.LogSource.LogInfo(
            $"Game state | Score={state.Score} | " +
            $"Paused={state.IsPaused} | " +
            $"Day={state.Day} | " +
            $"Stations={state.StationCount} | " +
            $"Lines={state.LineCount}");

        foreach (var station in state.Stations)
        {
            string x = station.X.ToString(
                "F1", CultureInfo.InvariantCulture);

            string y = station.Y.ToString(
                "F1", CultureInfo.InvariantCulture);

            Plugin.LogSource.LogInfo(
                $"Station ID={station.Id} | " +
                $"Type={station.Type} | " +
                $"Position=({x}, {y}) | " +
                $"Passengers={station.WaitingPassengers}/" +
                $"{station.Capacity} | " +
                $"Lines={station.LineCount} | " +
                $"Interchange={station.IsInterchange}");
        }


        foreach (var line in state.Lines)
        {
            Plugin.LogSource.LogInfo(
                $"Line {line.Index} | " +
                $"Trains={line.TrainCount} | " +
                $"Links={line.Links.Count} | " +
                $"Mothballed={line.IsMothballed}");

            foreach (var link in line.Links)
            {
                Plugin.LogSource.LogInfo(
                    $"  Link {link.Id}: " +
                    $"{link.StartStationId} -> " +
                    $"{link.EndStationId}");
            }


            foreach (var train in line.Trains)
            {
                Plugin.LogSource.LogInfo(
                    $"  Train ID={train.Id} | " +
                    $"Line={train.LineIndex} | " +
                    $"Railcars={train.ActiveRailcarCount}/" +
                    $"{train.RailcarCount} | " +
                    $"Passengers={train.PassengerCount}");
            }

        }

    }
}
