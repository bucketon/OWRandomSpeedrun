﻿using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using System.Collections;

namespace OuterWildsRandomSpeedrun
{
    public class OuterWildsRandomSpeedrun : ModBehaviour
    {
        protected SpawnPoint[] _spawnPoints;
        protected int _spawnPointIndex = 0;

        protected PlayerSpawner _spawner;

        protected bool _isStarted;

        protected bool _onceFlag;

        protected SpawnLocation _spawnPoint;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(OuterWildsRandomSpeedrun)} is loaded!", MessageType.Success);


            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            };

            ModHelper.HarmonyHelper.EmptyMethod<DebugInputManager>("Awake");
            ModHelper.Events.Subscribe<DebugInputManager>(Events.AfterStart);
            ModHelper.Events.Subscribe<DebugInputManager>(Events.AfterAwake);
            ModHelper.Events.Subscribe<PlayerAudioController>(Events.AfterStart);
            ModHelper.Events.Subscribe<RingWorldController>(Events.AfterStart);
            ModHelper.Events.Event += OnEvent;
        }

        private void Update()
        {
            if (!_isStarted)
            {
                return;
            }

            if (!_onceFlag)
            {
                HandleBasicWarp();
                _onceFlag = true;
            }
            
        }

        private void HandleBasicWarp()
        {
            ModHelper.Console.WriteLine($"Warp to {_spawnPoint.ToString()}!", MessageType.Success);
            _spawner.DebugWarp(_spawner.GetSpawnPoint(_spawnPoint));
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            if (behaviour is DebugInputManager && ev == Events.AfterStart)
            {
                ModHelper.Console.WriteLine("isStarted!", MessageType.Success);
                _isStarted = true;
                GetSpawnPoints();
            }
            if (behaviour is DebugInputManager && ev == Events.AfterAwake)
            {
                if (_isStarted)
                {
                    HandleBasicWarp();
                }
            }
        }

        protected void GetSpawnPoints()
        {
            if (_spawner == null)
            {
                _spawner = GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
                var spawnPointsField = typeof(PlayerSpawner)
                    .GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
                var spawnPoints = spawnPointsField?.GetValue(_spawner) as SpawnPoint[];
                _spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

                ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);
                _spawnPoint = randomSpawnPoint();
            }
        }

        protected SpawnLocation randomSpawnPoint()
        {
            SpawnLocation[] validSpawnPoints = { 
                SpawnLocation.HourglassTwin_1,
                SpawnLocation.HourglassTwin_2,
                SpawnLocation.GasGiant,
                SpawnLocation.BrittleHollow,
                SpawnLocation.DarkBramble,
                SpawnLocation.GasGiantMoon,
                SpawnLocation.QuantumMoon,
                SpawnLocation.LunarLookout,
                SpawnLocation.SignalDish,
                SpawnLocation.SunStation,
                SpawnLocation.TimberHearth_Alt,
            };
            var random = new System.Random((int)Time.time);
            var randIndex = random.Next(0, validSpawnPoints.Length);
            ModHelper.Console.WriteLine($"Spawn point {validSpawnPoints[randIndex]} set, from index {randIndex}", MessageType.Info);
            return validSpawnPoints[randIndex];
        }
    }
}