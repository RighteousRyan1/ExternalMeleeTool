using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EMTDisplay.CamExperiments; 
internal class CinematicCamera {
    public static bool IsEnabled;

    static int _prevSeconds = -1;

    public static void CineCamUpdate(MatchData Match, Microsoft.Xna.Framework.GameTime gameTime) {
        int currentSeconds = gameTime.TotalGameTime.Seconds;

        if (currentSeconds % 4 == 0 && currentSeconds != _prevSeconds) {
            var stdat = StageData.GetStageData();

            var lines = stdat.MapLines;

            if (lines is null) return;

            List<(Vector2 start, Vector2 end)> segments = [];

            foreach (var lineDesc in lines) {
                if (lineDesc.coll_type != CollKind.Top) continue;
                segments.Add((stdat.Vertices[lineDesc.StartIdx], stdat.Vertices[lineDesc.EndIdx]));
            }

            var rand = new Random();
            var (start, end) = segments[rand.Next(segments.Count)];

            float randBetween(float min, float max) {
                var val = rand.NextSingle();
                var randf = val * (max - min) + min;

                return randf;
            }

            var randX = randBetween(start.X, end.X);
            var randY = randBetween(start.Y, end.Y) + 5f;


            var posAlongLine = new Vector2(randX, randY);

            var cam = new MeleeFreeCamera();

            float zRange = 100;
            float zMin = 20;
            float randZ = randBetween(-zRange, -zMin);

            cam.Eye = new Vector3(posAlongLine, randZ);

            var posAvg = Vector3.Zero;
            var ftcount = 0;
            for (int i = 0; i < Match.Fighters.Length; i++) {
                if (Match.Fighters[i].SlotKind != SlotKind.Human) continue;
                ftcount++;
                posAvg += Match.Fighters[i].Position;
            }

            posAvg /= ftcount;

            cam.Focus = posAvg;
            cam.Fov = randBetween(80, 100);

            Console.WriteLine($"{cam.Eye}, {cam.Focus}, {cam.Fov}");

            cam.ApplyToMelee();
        }

        _prevSeconds = currentSeconds;
    }
}
