﻿using System;
using System.Linq;
using MoonlapseNetworking;
using MoonlapseNetworking.Packets;
using MoonlapseServer.Utils.Logging;
using MoonlapseNetworking.Models;
using MoonlapseNetworking.Models.Components;
using Microsoft.EntityFrameworkCore;

namespace MoonlapseServer.UserStates
{
    public class EntryState : UserState
    {
        Protocol _protocol;

        bool _readyToChangeState;

        public EntryState(Protocol protocol)
        {
            _protocol = protocol;
            LoginPacketEvent += EntryState_LoginPacketEvent;
            RegisterPacketEvent += EntryState_RegisterPacketEvent;
            OkPacketEvent += EntryState_OkPacketEvent;
        }

        private void EntryState_OkPacketEvent(object sender, PacketEventArgs args)
        {
            if (_readyToChangeState)
            {
                _protocol.UserState = new GameState(_protocol);
            }
        }

        private void EntryState_RegisterPacketEvent(object sender, PacketEventArgs args)
        {
            var p = Packet.FromString<RegisterPacket>(args.PacketString);

            if (!IsStringWellFormed(p.Username) || !IsStringWellFormed(p.Password))
            {
                _protocol.Log($"Registration failed: username or password contains whitespace or is empty");
                _protocol.SendPacket(new DenyPacket { Message = "Fields cannot contain whitespace" });
                return;
            }

            _protocol.Log($"Attempting registration with username={p.Username}");

            var db = _protocol.Server.Db;

            var user = db.Users
                .Where(e => e.Username == p.Username)
                .FirstOrDefault();

            if (user == null)
            {
                // can register :)

                var e = new Entity
                {
                    Name = p.Username,
                    TypeName = "Player"
                };
                db.Add(e);

                var initialRoom = db.Rooms
                    .Where(r => r.Id == 1)
                    .First();

                var pos = e.AddComponent<Position>();
                pos.Room = initialRoom;
                db.Add(pos);

                var u = new User
                {
                    Entity = e,
                    Username = p.Username,
                    Password = p.Password
                };

                db.Add(u);
                db.SaveChanges();

                db.SaveChanges();
                _protocol.Log($"Registration successful: user with username={p.Username}");
                _protocol.SendPacket(new OkPacket { Message = "Register" });
            }
            else
            {
                // user already exists >:(
                _protocol.Log($"Registration failed: user with username={p.Username} already exists", LogContext.Warn);
                _protocol.SendPacket(new DenyPacket { Message = "User already exists" });
            }
        }

        void EntryState_LoginPacketEvent(object sender, PacketEventArgs args)
        {
            var p = Packet.FromString<LoginPacket>(args.PacketString);

            if (!IsStringWellFormed(p.Username) || !IsStringWellFormed(p.Password))
            {
                _protocol.Log($"Login failed: username or password contains whitespace or is empty");
                _protocol.SendPacket(new DenyPacket { Message = "Fields cannot contain whitespace" });
                return;
            }

            var db = _protocol.Server.Db;

            _protocol.Log($"Attempting login with username={p.Username}");

            var user = db.Users
                .Include(u => u.Entity)
                .Where(u => u.Username == p.Username)
                .FirstOrDefault();

            if (user == null)
            {
                // user does not exist
                _protocol.Log($"Login failed: user with username={p.Username} does not exist");
                _protocol.SendPacket(new DenyPacket
                {
                    Message = "User does not exist or incorrect password"
                });
            }
            else
            {
                // user exists
                if (user.Password == p.Password)
                {
                    var entity = db.Entities
                        .Where(e => e == user.Entity)
                        .First();

                    // if player is already logged in
                    if (_protocol.Server.ConnectedProtocols
                        .Where(p => p.PlayerEntity == entity)
                        .FirstOrDefault() != null)
                    {
                        _protocol.Log($"Login failed: user already logged in");
                        _protocol.SendPacket(new DenyPacket
                        {
                            Message = "User already logged in"
                        });
                    }
                    else
                    {
                        _protocol.Login(entity);
                        _readyToChangeState = true;
                    }
                }
                else
                {
                    // incorrect password
                    _protocol.Log($"Login failed: password incorrect");
                    _protocol.SendPacket(new DenyPacket
                    {
                        Message = "User does not exist or incorrect password"
                    });
                }
            }
        }

        /// <summary>
        /// A string to be used in usernames + passwords should not contain spaces or be empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static bool IsStringWellFormed(string s) => !(s.Contains(' ') || s == "");
    }
}
