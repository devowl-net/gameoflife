﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

using Gol.Core.Controls.Models;
using Gol.Core.Data;
using Gol.Core.Prism;

namespace Gol.Core.Algorithm
{
    /// <summary>
    /// Black and White life cycle algorithm.
    /// </summary>
    public class DoubleStateLife : NotificationObject, ILifeControl<bool>
    {
        private readonly object _syncObject = new object();

        private readonly Thread _lifeTimer;

        private readonly TimeSpan _realtimeDelay = TimeSpan.FromMilliseconds(100);

        private bool IsAlive = false;

        //    -1   0   1
        // -1 [ ] [ ] [ ]
        //  0 [ ] [X] [ ]
        //  1 [ ] [ ] [ ]
        private readonly Offset[] _offsets = {

            // Line 1
            new Offset(-1, -1),
            new Offset(0, -1),
            new Offset(1, -1),

            // Line 2
            new Offset(-1, 0),
            new Offset(1, 0),

            // Line 3
            new Offset(-1, 1),
            new Offset(0, 1),
            new Offset(1, 1),
        };

        private MonoLifeGrid<bool> _current;

        private MonoLifeGrid<bool> _previous;

        /// <summary>
        /// Constructor for <see cref="DoubleStateLife"/>.
        /// </summary>
        public DoubleStateLife(MonoLifeGrid<bool> current) 
            : this()
        {
            SetCurrent(current);
        }

        /// <summary>
        /// Constructor for <see cref="DoubleStateLife"/>.
        /// </summary>
        public DoubleStateLife()
        {
            _lifeTimer = new Thread(TimerElapsed);
            _lifeTimer.Start();
        }

        private void TimerElapsed()
        {
            while (true)
            {
                if (IsAlive)
                {
                    LifeStep();
                }

                Thread.Sleep(_realtimeDelay);
            }
        }

        /// <inheritdoc/>
        public MonoLifeGrid<bool> Previous
        {
            get
            {
                return _previous;
            }

            private set
            {
                _previous = value;
                RaisePropertyChanged(() => Previous);
            }
        }

        /// <inheritdoc/>
        public MonoLifeGrid<bool> Current
        {
            get
            {
                return _current;
            }

            private set
            {
                _current = value;
                RaisePropertyChanged(() => Current);
            }
        }

        /// <inheritdoc/>
        public void SetCurrent(MonoLifeGrid<bool> grid)
        {
            if (grid == null)
            {
                Stop();
            }
            else
            {
                Current = grid;
            }
        }

        /// <inheritdoc/>
        public bool Start()
        {
            if (Current == null || Current.Height == 0 || Current.Width == 0)
            {
                return false;
            }
                
            return IsAlive = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            IsAlive = false;
        }

        private int NearCells(int x, int y, MonoLifeGrid<bool> field)
        {
            return _offsets.Sum(offset => IsBlack(x + offset.Dx, y + offset.Dy, field));
        }

        private static int IsBlack(int x, int y, MonoLifeGrid<bool> field)
        {
            if ((0 <= x && x < field.Width) && (0 <= y && y < field.Height))
            {
                return field[x, y] ? 1 : 0;
            }

            return 0;
        }

        private void LifeStep()
        {
            /*
            * У каждой клетки 8 соседних клеток.
            * В каждой клетке может жить существо.
            * Существо с двумя или тремя соседями выживает в следующем поколении, иначе погибает от одиночества или перенаселённости.
            * В пустой клетке с тремя соседями в следующем поколении рождается существо.
            */
            
            Previous = _current.Clone();
            for (int i = 0; i < _current.Width; i ++)
            {
                for (int j = 0; j < _current.Height; j ++)
                {
                    var nearCells = NearCells(i, j, Previous);
                    bool isCreature = Previous[i, j];

                    if (isCreature)
                    {
                        // Существо с двумя или тремя соседями выживает в следующем поколении, иначе погибает от одиночества или перенаселённости.
                        _current[i, j] = nearCells == 2 || nearCells == 3;
                    }
                    else
                    {
                        // В пустой клетке с тремя соседями в следующем поколении рождается существо.
                        _current[i, j] = nearCells == 3;
                    }

                }
            }
            
            RaisePropertyChanged(nameof(Current));
            RaisePropertyChanged(nameof(Previous));
        }

        private class Offset
        {
            /// <summary>
            /// Constructor for <see cref="Offset"/>.
            /// </summary>
            public Offset(int dx, int dy)
            {
                Dx = dx;
                Dy = dy;
            }

            /// <summary>
            /// X offset.
            /// </summary>
            public int Dx { get; }

            /// <summary>
            /// Y offset.
            /// </summary>
            public int Dy { get; }
        }
    }
}