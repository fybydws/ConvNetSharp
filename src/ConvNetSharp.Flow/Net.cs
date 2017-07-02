﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers;
using ConvNetSharp.Flow.Ops;
using ConvNetSharp.Volume;

namespace ConvNetSharp.Flow
{
    public class Net<T> : INet<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly Dictionary<string, Volume<T>> _dico = new Dictionary<string, Volume<T>>();

        public Net()
        {
            this.Session = new Session<T>();
        }

        public Session<T> Session { get; }

        public List<Layers.LayerBase<T>> Layers { get; } = new List<Layers.LayerBase<T>>();

        public Op<T> Op { get; set; }

        public Op<T> Cost { get; set; }

        public T Backward(Volume<T> y)
        {
            throw new NotImplementedException();
        }

        public Volume<T> Forward(Volume<T> input, bool isTraining = false)
        {
            this._dico["input"] = input;
            return this.Session.Run(this.Op, this._dico);
        }

        public T GetCostLoss(Volume<T> input, Volume<T> y)
        {
            this._dico["Y"] = y;
            this._dico["input"] = input;
            return this.Session.Run(this.Cost, this._dico).Get(0);
        }

        public List<ParametersAndGradients<T>> GetParametersAndGradients()
        {
            throw new NotImplementedException();
        }

        public int[] GetPrediction()
        {
            var activation = this.Op.Evaluate(this.Session);
            var N = activation.Shape.GetDimension(3);
            var C = activation.Shape.GetDimension(2);
            var result = new int[N];

            for (var n = 0; n < N; n++)
            {
                var maxv = activation.Get(0, 0, 0, n);
                var maxi = 0;

                for (var i = 1; i < C; i++)
                {
                    var output = activation.Get(0, 0, i, n);
                    if (Ops<T>.GreaterThan(output, maxv))
                    {
                        maxv = output;
                        maxi = i;
                    }
                }

                result[n] = maxi;
            }

            return result;
        }

        public void AddLayer(Layers.LayerBase<T> layer)
        {
            var previousLayer = this.Layers.LastOrDefault();

            if (previousLayer != null)
            {
                layer.AcceptParent(previousLayer);
            }

            this.Layers.Add(layer);

            this.Op = this.Layers.Last().Op;

            var lastLayer = layer as Layers.ILastLayer<T>;
            if (lastLayer != null)
            {
                this.Cost = lastLayer.Cost;
                this.Session.Differentiate(this.Cost);
            }
        }

        public static Layers.InputLayer<T> Create()
        {
            return new Layers.InputLayer<T>();
        }
    }
}