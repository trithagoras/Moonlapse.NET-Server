﻿using System;
using SadConsole;
using MoonlapseClient.States;
using MoonlapseNetworking.Models.Components;
using MoonlapseNetworking.Models;

namespace MoonlapseClient.Consoles
{
    public class GameConsole : SadConsole.Console
    {
        readonly GameState _state;

        public GameConsole(GameState state) : base(Game.Width, Game.Height, FontController.GameFont)
        {
            _state = state;
        }

        public override void Draw(TimeSpan timeElapsed)
        {
            base.Draw(timeElapsed);
            Clear();

            DrawTerrain();

            // todo: draw all entities
            DrawEntities();

            // draw player last
            SetGlyph((Game.Width / 8) - 1, (Game.Height / 4) - 1, 77);
        }

        void DrawEntities()
        {
            foreach (var entity in _state.KnownEntities.Values)
            {
                if (entity == _state.PlayerEntity)
                {
                    continue;
                }

                var pPos = _state.PlayerEntity.GetComponent<Position>();
                var ePos = entity.GetComponent<Position>();

                // either entity has no position component or not loaded yet
                if (ePos == null)
                {
                    continue;
                }

                // Draw other players on top of all else
                if (entity.TypeName == "Player")
                {
                    int halfWidth = (Game.Width / 8) - 1;
                    int halfHeight = (Game.Height / 4) - 1;

                    // todo: check viewport

                    var cx = halfWidth + ePos.X - pPos.X;
                    var cy = halfHeight + ePos.Y - pPos.Y;

                    SetGlyph(cx, cy, 77);
                }
            }
        }

        void DrawTerrain()
        {
            if (_state.PlayerEntity == null || _state.PlayerEntity.GetComponent<Position>() == null)
            {
                return;
            }
            var playerX = _state.PlayerEntity.GetComponent<Position>().X;
            var playerY = _state.PlayerEntity.GetComponent<Position>().Y;
            var room = _state.PlayerEntity.GetComponent<Position>().Room;

            var viewRadius = 10;

            for (int x = -viewRadius; x < viewRadius + 1; x++)
            {
                for (int y = -viewRadius; y < viewRadius + 1; y++)
                {
                    var xx = playerX + x;
                    var yy = playerY + y;

                    if (!room.CoordinateExists(xx, yy))
                    {
                        continue;
                    }

                    int halfWidth = (Game.Width / 8) - 1;
                    int halfHeight = (Game.Height / 4) - 1;
                    int cx = x + halfWidth;
                    int cy = y + halfHeight;

                    var pixel = room.Terrain[yy, xx];
                    SetGlyph(cx, cy, pixel);
                }
            }
        }
    }
}
